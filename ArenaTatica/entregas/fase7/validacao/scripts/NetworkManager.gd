extends Node

signal session_started
signal session_ended
signal client_ready(peer_id: int)
signal status_changed(message: String)

const SERVER: int = 1
const PORT: int = 9080

var active: bool = false
var synced: bool = false
var player_2: int = 0
var clients: Array[int] = []


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
	if not (url.begins_with("ws://") or url.begins_with("wss://")):
		status_changed.emit("Informe uma URL ws:// ou wss://.")
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
	if multiplayer.multiplayer_peer != null:
		multiplayer.multiplayer_peer.close()
	multiplayer.multiplayer_peer = OfflineMultiplayerPeer.new()
	active = false
	synced = false
	player_2 = 0
	clients.clear()
	session_ended.emit()
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
	rpc_ready.rpc_id(SERVER)
	status_changed.emit("P2 conectado; aguardando estado do Host.")


@rpc("any_peer", "call_remote", "reliable")
func rpc_ready() -> void:
	var sender: int = multiplayer.get_remote_sender_id()
	if not active or not multiplayer.is_server() or sender != player_2:
		return
	if clients.has(sender):
		return
	clients.append(sender)
	status_changed.emit("Host P1 · P2 conectado")
	client_ready.emit(sender)


func _peer_disconnected(peer_id: int) -> void:
	if multiplayer.is_server() and peer_id == player_2:
		close()
		status_changed.emit("P2 desconectado. Modo local.")


func _connection_failed() -> void:
	close()
	status_changed.emit("Conexão encerrada ou indisponível. Modo local.")
