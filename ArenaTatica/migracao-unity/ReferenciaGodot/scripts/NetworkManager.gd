extends Node

signal session_started
signal session_ended
signal client_ready(peer_id: int)
signal status_changed(message: String)
signal connection_lost(message: String)

const SERVER: int = 1
const PORT: int = 9080

var active: bool = false
var synced: bool = false
var player_2: int = 0
var clients: Array[int] = []
var allow_insecure_development: bool = OS.is_debug_build()
var _pending_ready: Dictionary = {}
var _session_generation := 0
var in_menu := false
var _closing := false
var _failure_pending := false
var input_debounce_ms := 120
var suppressed_inputs := 0
var _input_times: Dictionary = {}
var _probe_serial := 0
var _next_probe_ms := 0
var _probes: Dictionary = {}
var _probe_requests: Dictionary = {}
var _rtt: Dictionary = {}
var _probe_results: Array[bool] = []
const PROBE_TIMEOUT_MS := 3000


func allow_input(action: StringName, sender: int = 0) -> bool:
	if in_menu or _closing:
		return false
	if sender != 0 and (not active or not multiplayer.is_server() or not clients.has(sender)):
		return false
	var key := "%d:%s" % [sender, action]
	var now := Time.get_ticks_msec()
	if now - int(_input_times.get(key, -100000)) < input_debounce_ms:
		suppressed_inputs += 1
		return false
	_input_times[key] = now
	return true


func _record_probe(success: bool) -> void:
	_probe_results.append(success)
	if _probe_results.size() > 60:
		_probe_results.pop_front()


func debug_metrics() -> Dictionary:
	var rtt := -1.0
	for value in _rtt.values():
		rtt = maxf(rtt, float(value))
	var missed := _probe_results.count(false)
	return {"role": "Local" if not active else ("Host" if multiplayer.is_server() else "Client"),
		"rtt_ms": rtt, "samples": _probe_results.size(), "pending": _probes.size(),
		"timeout_percent": 0.0 if _probe_results.is_empty() else 100.0 * missed / _probe_results.size(),
		"suppressed": suppressed_inputs}


func _poll_probes() -> void:
	if not active or not synced:
		return
	var now := Time.get_ticks_msec()
	for id in _probes.keys():
		if now - int(_probes[id]["time"]) >= PROBE_TIMEOUT_MS:
			_rtt.erase(_probes[id]["peer"])
			_probes.erase(id)
			_record_probe(false)
	if now < _next_probe_ms:
		return
	_next_probe_ms = now + 1000
	var peers: Array = clients.duplicate() if multiplayer.is_server() else [SERVER]
	for peer_id in peers:
		_probe_serial += 1
		_probes[_probe_serial] = {"peer": peer_id, "time": now}
		rpc_probe.rpc_id(peer_id, _probe_serial)


@rpc("any_peer", "call_remote", "reliable", 0)
func rpc_probe(id: int) -> void:
	var sender := multiplayer.get_remote_sender_id()
	if not active or not synced or id <= 0:
		return
	if (multiplayer.is_server() and not clients.has(sender)) or (not multiplayer.is_server() and sender != SERVER):
		return
	var now := Time.get_ticks_msec()
	if now - int(_probe_requests.get(sender, -100000)) < 500:
		return
	_probe_requests[sender] = now
	rpc_probe_reply.rpc_id(sender, id)


@rpc("any_peer", "call_remote", "reliable", 0)
func rpc_probe_reply(id: int) -> void:
	var sender := multiplayer.get_remote_sender_id()
	if not active or not _probes.has(id) or _probes[id]["peer"] != sender:
		return
	var elapsed := Time.get_ticks_msec() - int(_probes[id]["time"])
	_probes.erase(id)
	if elapsed >= PROBE_TIMEOUT_MS:
		_record_probe(false)
		return
	_rtt[sender] = float(elapsed)
	_record_probe(true)


func _arena() -> Node:
	return get_tree().get_first_node_in_group("network_arena")


func _ready() -> void:
	multiplayer.peer_connected.connect(_peer_connected)
	multiplayer.peer_disconnected.connect(_peer_disconnected)
	multiplayer.connected_to_server.connect(_connected)
	multiplayer.connection_failed.connect(_connection_failed)
	multiplayer.server_disconnected.connect(_connection_failed)


func host(port: int = PORT) -> Error:
	if OS.has_feature("web"):
		status_changed.emit("No navegador, use Join; execute o Host nativo.")
		return ERR_UNAVAILABLE
	if port < 1 or port > 65535:
		return ERR_INVALID_PARAMETER
	close()
	var peer := WebSocketMultiplayerPeer.new()
	var error: Error = peer.create_server(port)
	if error != OK:
		status_changed.emit(error_string(error))
		return error
	multiplayer.multiplayer_peer = peer
	active = true
	synced = true
	session_started.emit()
	status_changed.emit("Host P1 · porta %d" % port)
	return OK


func join(url: String = "ws://127.0.0.1:9080") -> Error:
	url = url.strip_edges()
	if not (url.begins_with("ws://") or url.begins_with("wss://")):
		status_changed.emit("Informe uma URL ws:// ou wss://.")
		return ERR_INVALID_PARAMETER
	var secure_required := not allow_insecure_development
	if OS.has_feature("web"):
		secure_required = secure_required or str(JavaScriptBridge.eval("window.location.protocol")) == "https:"
	if secure_required and not url.begins_with("wss://"):
		status_changed.emit("Use WSS com certificado válido; WS somente em desenvolvimento.")
		return ERR_INVALID_PARAMETER
	close()
	var peer := WebSocketMultiplayerPeer.new()
	var error: Error = peer.create_client(url)
	if error != OK:
		status_changed.emit(error_string(error))
		return error
	multiplayer.multiplayer_peer = peer
	active = true
	status_changed.emit("Conectando…")
	return OK


func close() -> void:
	if _closing:
		return
	_closing = true
	_session_generation += 1
	_pending_ready.clear()
	active = false
	synced = false
	in_menu = false
	_failure_pending = false
	player_2 = 0
	clients.clear()
	_input_times.clear()
	suppressed_inputs = 0
	_probes.clear()
	_probe_requests.clear()
	_probe_results.clear()
	_rtt.clear()
	_next_probe_ms = 0
	var old_peer := multiplayer.multiplayer_peer
	multiplayer.multiplayer_peer = OfflineMultiplayerPeer.new()
	if old_peer != null:
		old_peer.close()
	var arena := _arena()
	if arena != null:
		arena.get_node("AIManager").stop_match()
	session_ended.emit()
	get_node("/root/TurnManager").reset()
	_closing = false
	status_changed.emit("Modo local")


func can_control(peer_id: int, turn: String) -> bool:
	if not active:
		return peer_id == SERVER
	if not multiplayer.is_server():
		return false
	if turn == "P1" or turn == "IA_NEUTRA":
		return peer_id == SERVER
	return turn == "P2" and peer_id == player_2 and clients.has(peer_id)


func from_server() -> bool:
	var sender: int = multiplayer.get_remote_sender_id()
	return (multiplayer.is_server() and sender == 0) or (
		active and not multiplayer.is_server() and sender == SERVER)


func _peer_connected(peer_id: int) -> void:
	if not multiplayer.is_server():
		return
	if player_2 != 0:
		multiplayer.multiplayer_peer.disconnect_peer(peer_id)
		return
	player_2 = peer_id


func _connected() -> void:
	session_started.emit()
	status_changed.emit("P2 conectado; aguardando estado do Host.")
	var generation := _session_generation
	while active and generation == _session_generation:
		var arena := _arena()
		var game: Node = arena.get_node_or_null("GameLoopManager") if arena != null else null
		if game != null and game._initialized:
			rpc_ready.rpc_id(SERVER)
			return
		await get_tree().process_frame


@rpc("any_peer", "call_remote", "reliable")
func rpc_ready() -> void:
	var sender: int = multiplayer.get_remote_sender_id()
	if not active or not multiplayer.is_server() or sender != player_2:
		return
	if clients.has(sender) or _pending_ready.has(sender):
		return
	_pending_ready[sender] = Time.get_ticks_msec()


func _process(_delta: float) -> void:
	_poll_probes()
	if not active or not multiplayer.is_server() or _pending_ready.is_empty():
		return
	var arena := _arena()
	if arena == null:
		return
	var game: Node = arena.get_node("GameLoopManager")
	var ai: Node = arena.get_node("AIManager")
	var combat: Node = arena.get_node("CombatManager")
	if not game._initialized:
		return
	for peer_id in _pending_ready.keys():
		if not multiplayer.get_peers().has(peer_id):
			_pending_ready.erase(peer_id)
			continue
		if Time.get_ticks_msec() - int(_pending_ready[peer_id]) > 15000:
			_pending_ready.erase(peer_id)
			multiplayer.multiplayer_peer.disconnect_peer(peer_id)
			continue
		if arena.authority.is_busy() or ai._running or not game._pending.is_empty():
			continue
		_pending_ready.erase(peer_id)
		# Ordem confiável no canal 0; sem await entre os snapshots.
		game._send_state(peer_id)
		ai._send_snapshot(peer_id)
		combat._send_stats(peer_id)
		arena._send_snapshot(peer_id)
		clients.append(peer_id)
		client_ready.emit(peer_id)
		status_changed.emit("Host P1 · P2 sincronizado")


func _peer_disconnected(peer_id: int) -> void:
	if _closing or not active:
		return
	_rtt.erase(peer_id)
	_probe_requests.erase(peer_id)
	for id in _probes.keys():
		if _probes[id]["peer"] == peer_id:
			_probes.erase(id)
	for action in [&"move", &"attack"]:
		_input_times.erase("%d:%s" % [peer_id, action])
	_pending_ready.erase(peer_id)
	clients.erase(peer_id)
	if not multiplayer.is_server() or peer_id != player_2:
		return
	player_2 = 0
	var arena := _arena()
	if arena == null:
		return
	var game: Node = arena.get_node("GameLoopManager")
	var combat: Node = arena.get_node("CombatManager")
	var ai: Node = arena.get_node("AIManager")
	var turns: Node = get_node("/root/TurnManager")
	ai._waiting.erase(peer_id)
	if game._initialized and not game.dead.has(2):
		var entity: Node = game._roots.get(2)
		if is_instance_valid(entity):
			entity.cancel_movement()
		if str(turns.get_state()["t"]) == "P2":
			turns._moving = false
			turns._remaining_steps = 0
			turns._pass_pending = false
		combat.stats[2].server_damage(combat.stats[2].hp)
		combat.revision += 1
		game._pending[2] = true
		game._flush()
		for remaining_peer in clients:
			combat._send_stats(remaining_peer)
	status_changed.emit("P2 desconectado; entidade removida. Host continua ativo.")


func _connection_failed() -> void:
	if _closing or not active or _failure_pending:
		return
	_failure_pending = true
	_finish_failure.call_deferred(_session_generation)


func _finish_failure(generation: int) -> void:
	if generation != _session_generation or not active:
		return
	close()
	in_menu = true
	var message := "Host desconectado ou conexão indisponível. Escolha como continuar."
	status_changed.emit(message)
	connection_lost.emit(message)


func resume_local() -> void:
	in_menu = false
	status_changed.emit("Modo local")
