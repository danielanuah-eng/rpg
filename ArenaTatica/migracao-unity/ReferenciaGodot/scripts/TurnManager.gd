extends Node

signal state_changed(state: Dictionary)
signal request_rejected(reason: String)
signal match_reset
signal turn_started(actor: String)

const ORDER: Array[String] = ["P1", "P2", "IA_NEUTRA"]
const MAX_PM: int = 3

var match_over := false

var _state: Dictionary = {"t": "P1", "pm": MAX_PM}
var _moving: bool = false
var _remaining_steps: int = 0
var _pass_pending: bool = false
var revision: int = 0
var _player_stats: Dictionary = {}


func register_player_stats(actor: String, stats: Node) -> void:
	if ORDER.has(actor):
		_player_stats[actor] = weakref(stats)


func _stats_for(actor: String) -> Node:
	if not _player_stats.has(actor):
		return null
	return _player_stats[actor].get_ref()


func _restore_active_pa() -> void:
	if not _authority():
		return
	var actor: String = str(_state["t"])
	var stats := _stats_for(actor)
	if stats != null:
		stats.server_restore_pa()
	turn_started.emit(actor)


func _authority() -> bool:
	return multiplayer.is_server()


func _publish() -> void:
	revision += 1
	state_changed.emit(get_state())
	var network := get_node_or_null("/root/NetworkManager")
	if network != null and network.active:
		for peer_id in network.clients:
			rpc_sync_state.rpc_id(peer_id, get_state(), revision)


@rpc("any_peer", "call_local", "reliable")
func rpc_sync_state(state: Dictionary, version: int) -> void:
	if multiplayer.is_server():
		return
	var network := get_node("/root/NetworkManager")
	if not network.from_server():
		return
	if state.size() != 2 or not ORDER.has(state.get("t")):
		return
	if typeof(state.get("pm")) != TYPE_INT or state["pm"] < 0 or state["pm"] > MAX_PM:
		return
	if version < 0:
		return
	_state = state.duplicate()
	revision = version
	state_changed.emit(get_state())


func get_state() -> Dictionary:
	return _state.duplicate()


func reset() -> void:
	if not _authority():
		return
	match_over = false
	_moving = false
	_remaining_steps = 0
	_pass_pending = false
	_state = {"t": "P1", "pm": MAX_PM}
	match_reset.emit()
	_restore_active_pa()
	_publish()


func can_move(actor: String, path: Array[Vector2i]) -> bool:
	if match_over or not _authority():
		return false
	var stats := _stats_for(actor)
	if stats != null and not stats.is_alive():
		return false
	var cost: int = path.size() - 1
	if _moving or actor != _state["t"] or cost <= 0 or cost > int(_state["pm"]):
		return false
	for i in range(1, path.size()):
		var delta: Vector2i = path[i] - path[i - 1]
		if absi(delta.x) + absi(delta.y) != 1:
			return false
	return true


func begin_move(actor: String, path: Array[Vector2i]) -> bool:
	if not can_move(actor, path):
		return false
	_moving = true
	_remaining_steps = path.size() - 1
	_publish()
	return true


func commit_step() -> bool:
	if match_over or not _authority():
		return false
	if not _moving or _remaining_steps <= 0 or int(_state["pm"]) <= 0:
		return false
	_remaining_steps -= 1
	_state["pm"] = int(_state["pm"]) - 1
	_publish()
	return true


func finish_move() -> void:
	if not _authority() or not _moving:
		return
	_moving = false
	_remaining_steps = 0
	if _pass_pending or int(_state["pm"]) == 0:
		_advance_turn()


func pass_turn() -> void:
	var network := get_node_or_null("/root/NetworkManager")
	if network != null and network.in_menu:
		return
	if network != null and network.active:
		if not network.synced:
			return
		if multiplayer.is_server():
			rpc_request_pass(revision)
		else:
			rpc_request_pass.rpc_id(1, revision)
		return
	_pass()


@rpc("any_peer", "call_local", "reliable")
func rpc_request_pass(expected_revision: int) -> void:
	if not _authority():
		return
	var sender: int = multiplayer.get_remote_sender_id()
	if sender == 0:
		sender = 1
	var network := get_node("/root/NetworkManager")
	if expected_revision != revision or not network.can_control(sender, str(_state["t"])):
		var reason: String = "Aguarde seu turno e o estado atualizado."
		if sender == 1:
			request_rejected.emit(reason)
		elif network.clients.has(sender):
			rpc_rejected.rpc_id(sender, reason)
		return
	_pass()


@rpc("authority", "call_remote", "reliable")
func rpc_rejected(reason: String) -> void:
	if get_node("/root/NetworkManager").from_server():
		request_rejected.emit(reason)



# IA da Fase 7; a Fase 8 pula jogadores eliminados e bloqueia o fim da partida.
var _neutral_ai: WeakRef

func register_neutral_ai(ai: Node) -> void:
	_neutral_ai = weakref(ai)
	if not ai.phase_finished.is_connected(_on_neutral_finished):
		ai.phase_finished.connect(_on_neutral_finished)

func _pass() -> void:
	if match_over or not _authority() or str(_state["t"]) == "IA_NEUTRA":
		return
	if _moving:
		_pass_pending = true
		return
	_advance_turn()

func _advance_turn() -> void:
	if match_over or not _authority() or str(_state["t"]) == "IA_NEUTRA":
		return
	_pass_pending = false
	var next: int = (ORDER.find(str(_state["t"])) + 1) % ORDER.size()
	for attempt in range(ORDER.size()):
		var candidate := _stats_for(ORDER[next])
		if candidate == null or candidate.is_alive():
			break
		next = (next + 1) % ORDER.size()
	_state = {"t": ORDER[next], "pm": MAX_PM}
	_restore_active_pa()
	_publish()
	if str(_state["t"]) == "IA_NEUTRA":
		var ai: Node = _neutral_ai.get_ref() if _neutral_ai != null else null
		if ai == null:
			push_error("IA_NEUTRA: AIManager não registrado")
			return
		ai.start_neutral_turn.call_deferred(revision)

func _on_neutral_finished(version: int) -> void:
	if match_over or not _authority() or version != revision or str(_state["t"]) != "IA_NEUTRA":
		return
	_moving = false
	_remaining_steps = 0
	_pass_pending = false
	var first := _stats_for("P1")
	_state = {"t": "P1" if first == null or first.is_alive() else "P2", "pm": MAX_PM}
	_restore_active_pa()
	_publish()

