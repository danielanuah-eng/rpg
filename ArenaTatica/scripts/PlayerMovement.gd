extends Node3D
## Composição consolidada: input -> pedido -> Host/AStar -> RPC -> animação.

const BoardScript = preload("res://scripts/board.gd")
const MoverScript = preload("res://scripts/grid_mover.gd")
const PickerScript = preload("res://scripts/grid_picker.gd")
const AuthorityScript = preload("res://scripts/move_authority.gd")
const UNIT_ID: StringName = &"player_1"
const UNIT_2: StringName = &"player_2"
const OWNERS: Dictionary = {UNIT_ID: "P1", UNIT_2: "P2"}

@export var spawn_cell := Vector2i(1, 1)
@export var spawn_cell_2 := Vector2i(10, 10)

@onready var board: BoardScript = $Arena/Board
@onready var player: MoverScript = $Player1
@onready var player_2: MoverScript = $Player2
@onready var picker: PickerScript = $GridPicker
@onready var authority: AuthorityScript = $MoveAuthority
@onready var marker: MeshInstance3D = $Destination
@onready var status: Label = $UI/StatusPanel/Margin/Status
@onready var coordinates: Label = $UI/InfoPanel/Margin/Lines/Coordinates
@onready var turns: Node = get_node("/root/TurnManager")
@onready var network: Node = get_node("/root/NetworkManager")

var units: Dictionary = {}
var selected_unit: StringName = UNIT_ID
var turn_label: Label
var _requester: int = 1


func _ready() -> void:
	add_to_group("network_arena")
	set_multiplayer_authority(1, true)
	units = {UNIT_ID: player, UNIT_2: player_2}
	authority.turns = turns
	var panel := _create_ui()
	var blocked: Array[Control] = [$UI/InfoPanel, $UI/StatusPanel, panel]
	picker.configure(board, $Arena/Camera3D, blocked)
	picker.cell_requested.connect(_on_move_intent)
	authority.path_approved.connect(_on_path_approved)
	authority.move_rejected.connect(_on_move_rejected)
	for unit_id in units:
		connect_mover(unit_id, units[unit_id])
	turns.state_changed.connect(_on_turn_changed)
	turns.request_rejected.connect(func(reason: String): status.text = reason)
	board.board_generated.connect(_reset_piece)
	network.session_started.connect(_reset_piece)
	network.session_ended.connect(_reset_piece)
	_reset_piece()


func connect_mover(unit_id: StringName, mover: Node) -> void:
	mover.step_guard = _is_step_allowed.bind(unit_id)
	mover.step_completed.connect(_on_step_completed.bind(unit_id))
	mover.movement_finished.connect(_on_movement_finished.bind(unit_id))


func _create_ui() -> VBoxContainer:
	var panel := VBoxContainer.new()
	panel.name = "TurnPanel"
	panel.position = Vector2(20, 120)
	$UI.add_child(panel)
	turn_label = Label.new()
	turn_label.name = "TurnState"
	panel.add_child(turn_label)
	_add_button(panel, "PassTurn", "Passar Turno", turns.pass_turn)
	var url := LineEdit.new()
	url.name = "ServerURL"
	url.text = "ws://127.0.0.1:9080"
	url.custom_minimum_size.x = 300
	panel.add_child(url)
	var host_button := _add_button(panel, "Host", "Host", func(): network.host())
	host_button.disabled = OS.has_feature("web")
	_add_button(panel, "Join", "Join", func(): network.join(url.text.strip_edges()))
	_add_button(panel, "Leave", "Desconectar", network.close)
	var connection := Label.new()
	connection.name = "Connection"
	connection.text = "Modo local"
	panel.add_child(connection)
	network.status_changed.connect(func(message: String): connection.text = message)
	return panel


func _add_button(panel: Node, node_name: String, text: String, action: Callable) -> Button:
	var button := Button.new()
	button.name = node_name
	button.text = text
	panel.add_child(button)
	button.pressed.connect(action)
	return button


func _reset_piece() -> void:
	var game := get_node_or_null("GameLoopManager")
	if game != null:
		game.restore_entities()
	picker.clear_pending()
	picker.set_process_unhandled_input(false)
	marker.visible = false
	for mover in units.values():
		mover.cancel_movement()
		mover.visible = false
	authority.configure(board.model)
	var spawns := {UNIT_ID: spawn_cell, UNIT_2: spawn_cell_2}
	var occupied: Array[Vector2i] = []
	for unit_id in units:
		var desired: Vector2i = spawns[unit_id]
		desired = Vector2i(clampi(desired.x, 0, board.columns - 1), clampi(desired.y, 0, board.rows - 1))
		var candidates: Array[Vector2i] = [desired]
		candidates.append_array(board.model.get_cells_in_order())
		for cell in candidates:
			if not occupied.has(cell) and authority.register_unit(unit_id, cell, OWNERS[unit_id]):
				units[unit_id].configure(board, cell)
				units[unit_id].visible = true
				occupied.append(cell)
				break
	picker.set_process_unhandled_input(occupied.size() == units.size())
	_on_turn_changed(turns.get_state())
	_update_coordinates()
	status.text = "Selecione sua peça e clique no destino." if occupied.size() == 2 else "São necessárias duas casas livres."
	if network.active and multiplayer.is_server():
		_broadcast_snapshot()


func _local_owner() -> String:
	if not network.active:
		return str(turns.get_state()["t"])
	return "P1" if multiplayer.is_server() else "P2"


func _on_turn_changed(state: Dictionary) -> void:
	selected_unit = &""
	for unit_id in OWNERS:
		if OWNERS[unit_id] == _local_owner():
			selected_unit = unit_id
	turn_label.text = "%s | PM: %d | Controle: %s" % [state["t"], state["pm"], _local_owner()]


func _on_move_intent(destination: Vector2i) -> void:
	if turns.match_over:
		return
	var combat := get_node_or_null("CombatManager")
	if combat != null and combat.handle_cell_click(destination):
		return
	if network.active and not network.synced:
		status.text = "Aguarde a sincronização com o Host."
		return
	for unit_id in units:
		if units[unit_id].visible and units[unit_id].current_cell == destination:
			if OWNERS[unit_id] != _local_owner():
				status.text = "Esta peça pertence ao outro jogador."
				return
			selected_unit = unit_id
			status.text = "Peça %s selecionada." % OWNERS[unit_id]
			return
	request_move(selected_unit, destination)


func request_move(unit_id: StringName, destination: Vector2i) -> void:
	if not network.allow_input(&"move"):
		return
	var target := {"x": destination.x, "z": destination.y}
	if not network.active or multiplayer.is_server():
		_validate_request(1, unit_id, target, turns.revision)
	elif network.synced:
		rpc_request_move.rpc_id(1, str(unit_id), target, turns.revision)


@rpc("any_peer", "call_local", "reliable")
func rpc_request_move(unit_id: String, target: Dictionary, expected_revision: int) -> void:
	if not network.active or not multiplayer.is_server():
		return
	var sender: int = multiplayer.get_remote_sender_id()
	if sender != 0 and not network.allow_input(&"move", sender):
		return
	_validate_request(1 if sender == 0 else sender, StringName(unit_id), target, expected_revision)


func _validate_request(sender: int, unit_id: StringName, target: Dictionary, version: int) -> void:
	if not multiplayer.is_server():
		return
	if turns.match_over or not units.has(unit_id) or not OWNERS.has(unit_id):
		_reject(sender, "Peça inválida; na fase neutra use Passar Turno.")
		return
	var owner: String = OWNERS[unit_id]
	if network.active and not network.can_control(sender, owner):
		_reject(sender, "Esta peça pertence ao outro jogador.")
		return
	if owner != str(turns.get_state()["t"]):
		_reject(sender, "Aguarde o turno da sua peça.")
		return
	if version != turns.revision:
		_reject(sender, "Estado atualizado; repita o comando.")
		return
	if not _valid_cell(target) or authority.is_busy():
		_reject(sender, "Destino inválido ou movimento em andamento.")
		return
	_requester = sender
	authority.request_move(unit_id, Vector2i(target["x"], target["z"]), owner)
	_requester = 1


func _valid_cell(data: Dictionary) -> bool:
	return data.size() == 2 and typeof(data.get("x")) == TYPE_INT and \
		typeof(data.get("z")) == TYPE_INT and data["x"] >= 0 and \
		data["x"] < board.columns and data["z"] >= 0 and data["z"] < board.rows


func _on_path_approved(unit_id: StringName, path: Array[Vector2i]) -> void:
	if not multiplayer.is_server() or not units.has(unit_id):
		return
	if not _execute_path(unit_id, path):
		authority.finish_move(unit_id)
		_reject(_requester, "O movimento não pôde ser iniciado.")
		_broadcast_snapshot()
		return
	var cells: Array[Dictionary] = []
	for cell in path:
		cells.append({"x": cell.x, "z": cell.y})
	var packet := {"u": str(unit_id), "p": cells, "d": units[unit_id].step_duration}
	if network.active:
		for peer_id in network.clients:
			rpc_execute_move.rpc_id(peer_id, packet)


func _execute_path(unit_id: StringName, path: Array[Vector2i]) -> bool:
	if not units.has(unit_id) or not units[unit_id].execute_path(path):
		return false
	marker.global_position = board.cell_to_world(path.back()) + Vector3.UP * 0.025
	marker.visible = true
	status.text = "%s: %d casas." % [OWNERS[unit_id], path.size() - 1]
	return true


@rpc("authority", "call_remote", "reliable")
func rpc_execute_move(packet: Dictionary) -> void:
	if not network.from_server() or not OWNERS.has(StringName(str(packet.get("u", "")))):
		return
	if typeof(packet.get("p")) != TYPE_ARRAY or typeof(packet.get("d")) != TYPE_FLOAT:
		return
	var cells: Array = packet["p"]
	var duration: float = packet["d"]
	if cells.size() < 2 or cells.size() > int(turns.MAX_PM) + 1 or not is_finite(duration) or duration <= 0:
		return
	var path: Array[Vector2i] = []
	for cell in cells:
		if not cell is Dictionary or not _valid_cell(cell):
			return
		path.append(Vector2i(cell["x"], cell["z"]))
	var unit_id := StringName(packet["u"])
	if not units.has(unit_id) or turns.match_over:
		return
	units[unit_id].configure(board, path[0])
	units[unit_id].step_duration = duration
	_execute_path(unit_id, path)


func _is_step_allowed(origin: Vector2i, destination: Vector2i, unit_id: StringName) -> bool:
	if network.active and not multiplayer.is_server():
		return true
	return authority.can_enter_step(unit_id, origin, destination)


func _on_step_completed(cell: Vector2i, unit_id: StringName) -> void:
	if multiplayer.is_server() and not authority.confirm_step(unit_id, cell):
		authority.finish_move(unit_id)
		var accepted := authority.get_unit_cell(unit_id)
		if not board.contains_cell(accepted):
			_reset_piece()
			return
		units[unit_id].configure(board, accepted)
		marker.visible = false
		status.text = "Movimento cancelado na última casa confirmada."
		_broadcast_snapshot()
	_update_coordinates()


func _on_movement_finished(_cell: Vector2i, interrupted: bool, unit_id: StringName) -> void:
	if multiplayer.is_server():
		authority.finish_move(unit_id)
		_broadcast_snapshot()
	marker.visible = false
	_update_coordinates()
	status.text = "Movimento interrompido." if interrupted else "Destino alcançado."


func _broadcast_snapshot() -> void:
	if network.active and multiplayer.is_server():
		for peer_id in network.clients:
			_send_snapshot(peer_id)


func _send_snapshot(peer_id: int) -> void:
	if not multiplayer.is_server() or authority.is_busy():
		return
	var positions: Dictionary = {}
	for unit_id in units:
		var cell := authority.get_unit_cell(unit_id)
		positions[str(unit_id)] = {"x": cell.x, "z": cell.y}
	rpc_snapshot.rpc_id(peer_id, {"u": positions, "s": turns.get_state(), "r": turns.revision})


@rpc("authority", "call_remote", "reliable")
func rpc_snapshot(packet: Dictionary) -> void:
	if not network.from_server() or typeof(packet.get("u")) != TYPE_DICTIONARY:
		return
	if typeof(packet.get("s")) != TYPE_DICTIONARY or typeof(packet.get("r")) != TYPE_INT:
		return
	var positions: Dictionary = packet["u"]
	if positions.size() > 2:
		return
	var occupied: Array[Vector2i] = []
	for key in positions:
		var unit_id := StringName(key)
		if not units.has(unit_id):
			return
		var cell = positions.get(str(unit_id))
		if not cell is Dictionary or not _valid_cell(cell):
			return
		var position_cell := Vector2i(cell["x"], cell["z"])
		if occupied.has(position_cell):
			return
		occupied.append(position_cell)
	var monster_positions: Dictionary = {}
	for unit_id in authority._positions:
		if str(unit_id).begins_with("monster_"):
			monster_positions[unit_id] = authority.get_unit_cell(unit_id)
	authority.configure(board.model)
	for unit_id in monster_positions:
		authority.register_unit(unit_id, monster_positions[unit_id], "IA_NEUTRA")
	for key in positions:
		var unit_id := StringName(key)
		var cell: Dictionary = positions[str(unit_id)]
		var position_cell := Vector2i(cell["x"], cell["z"])
		authority.register_unit(unit_id, position_cell, OWNERS[unit_id])
		units[unit_id].configure(board, position_cell)
		units[unit_id].visible = true
	turns.rpc_sync_state(packet["s"], packet["r"])
	marker.visible = false
	_update_coordinates()
	network.synced = true
	network.status_changed.emit("P2 conectado · controle da peça azul")


func _on_move_rejected(_unit_id: StringName, reason: String) -> void:
	_reject(_requester, reason)


func _reject(peer_id: int, reason: String) -> void:
	if peer_id == 1:
		status.text = reason
	elif network.active and network.clients.has(peer_id):
		rpc_rejected.rpc_id(peer_id, reason)


@rpc("authority", "call_remote", "reliable")
func rpc_rejected(reason: String) -> void:
	if network.from_server():
		status.text = reason


func _update_coordinates() -> void:
	var labels := PackedStringArray()
	for unit_id in OWNERS:
		if units.has(unit_id):
			var cell: Vector2i = units[unit_id].current_cell
			labels.append("%s: X %d · Z %d" % [OWNERS[unit_id], cell.x, cell.y])
		else:
			labels.append("%s: derrotado" % OWNERS[unit_id])
	coordinates.text = " | ".join(labels)
