extends Node3D
## Composição local: intenção -> validação -> execução -> confirmação por casa.

const BoardScript = preload("res://scripts/board.gd")
const MoverScript = preload("res://scripts/grid_mover.gd")
const PickerScript = preload("res://scripts/grid_picker.gd")
const AuthorityScript = preload("res://scripts/move_authority.gd")
const UNIT_ID: StringName = &"player_1"

@export var spawn_cell: Vector2i = Vector2i(1, 1)

@onready var board: BoardScript = $Arena/Board
@onready var player: MoverScript = $Player
@onready var picker: PickerScript = $GridPicker
@onready var authority: AuthorityScript = $MoveAuthority
@onready var marker: MeshInstance3D = $Destination
@onready var status: Label = $UI/StatusPanel/Margin/Status
@onready var coordinates: Label = $UI/InfoPanel/Margin/Lines/Coordinates
@onready var _turns: Node = get_node("/root/TurnManager")
var turn_label: Label


func _ready() -> void:
	authority.turns = _turns
	var turn_panel := VBoxContainer.new()
	turn_panel.name = "TurnPanel"
	turn_panel.position = Vector2(20, 120)
	$UI.add_child(turn_panel)
	turn_label = Label.new()
	turn_label.name = "TurnState"
	turn_panel.add_child(turn_label)
	var pass_button := Button.new()
	pass_button.name = "PassTurn"
	pass_button.text = "Passar Turno"
	turn_panel.add_child(pass_button)
	pass_button.pressed.connect(_turns.pass_turn)
	_turns.state_changed.connect(_on_turn_changed)
	var blocked: Array[Control] = [$UI/InfoPanel, $UI/StatusPanel]
	blocked.append(turn_panel)
	picker.configure(board, $Arena/Camera3D, blocked)
	picker.cell_requested.connect(_on_move_intent)
	authority.path_approved.connect(_on_path_approved)
	authority.move_rejected.connect(_on_move_rejected)
	player.step_guard = _is_step_allowed
	player.step_completed.connect(_on_step_completed)
	player.movement_finished.connect(_on_movement_finished)
	board.board_generated.connect(_reset_piece)
	_reset_piece()


func _reset_piece() -> void:
	picker.clear_pending()
	picker.set_process_unhandled_input(false)
	player.cancel_movement()
	marker.visible = false
	var cell := Vector2i(clampi(spawn_cell.x, 0, board.columns - 1), clampi(spawn_cell.y, 0, board.rows - 1))
	authority.configure(board.model)
	if not board.model.get_cell_data(cell).get("walkable", false):
		cell = Vector2i(-1, -1)
		for candidate in board.model.get_cells_in_order():
			if board.model.get_cell_data(candidate).get("walkable", false):
				cell = candidate
				break
	if not authority.register_unit(UNIT_ID, cell):
		player.visible = false
		coordinates.text = "Tabuleiro sem casas disponíveis."
		status.text = "Libere uma casa do tabuleiro para posicionar a peça."
		return
	player.visible = true
	player.configure(board, cell)
	picker.set_process_unhandled_input(true)
	_update_coordinates(cell)
	status.text = "Clique ou toque em uma casa para mover a peça."


func _on_move_intent(destination: Vector2i) -> void:
	# Futuro cliente: enviar apenas UNIT_ID e destino ao Host neste ponto.
	# O Host recalcula o caminho; nunca confia num caminho enviado pelo cliente.
	authority.request_move(UNIT_ID, destination, str(_turns.get_state()["t"]))


func _on_path_approved(unit_id: StringName, path: Array[Vector2i]) -> void:
	if unit_id != UNIT_ID:
		return
	if not player.execute_path(path):
		authority.finish_move(unit_id)
		status.text = "O movimento não pôde ser iniciado. Selecione outra casa."
		return
	var destination: Vector2i = path.back()
	marker.global_position = board.cell_to_world(destination) + Vector3.UP * 0.025
	marker.visible = true
	status.text = "Destino: X %d · Z %d  |  %d casas" % [destination.x, destination.y, path.size() - 1]


func _is_step_allowed(origin: Vector2i, destination: Vector2i) -> bool:
	return authority.can_enter_step(UNIT_ID, origin, destination)


func _on_step_completed(cell: Vector2i) -> void:
	if not authority.confirm_step(UNIT_ID, cell):
		# Restaura a última posição aceita, sem deixar o executor preso na rejeitada.
		authority.finish_move(UNIT_ID)
		var confirmed_cell := authority.get_unit_cell(UNIT_ID)
		if not board.contains_cell(confirmed_cell):
			_reset_piece()
			return
		player.configure(board, confirmed_cell)
		marker.visible = false
		_update_coordinates(confirmed_cell)
		status.text = "Movimento cancelado. Selecione outra casa."
		return
	_update_coordinates(cell)


func _on_movement_finished(cell: Vector2i, interrupted: bool) -> void:
	authority.finish_move(UNIT_ID)
	marker.visible = false
	_update_coordinates(cell)
	status.text = "Movimento interrompido nesta casa." if interrupted else "Destino alcançado. Escolha outra casa."


func _on_move_rejected(_unit_id: StringName, reason: String) -> void:
	status.text = reason


func _update_coordinates(cell: Vector2i) -> void:
	coordinates.text = "Tabuleiro %d × %d  |  Peça: X %d · Z %d" % [board.columns, board.rows, cell.x, cell.y]


func _on_turn_changed(state: Dictionary) -> void:
	turn_label.text = "%s | PM: %d" % [state["t"], state["pm"]]
