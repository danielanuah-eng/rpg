extends Node
## Transforma clique/toque numa intenção de célula; não movimenta peças.

signal cell_requested(cell: Vector2i)

const BoardScript = preload("res://scripts/board.gd")

var _board: BoardScript
var _camera: Camera3D
var _blocked_controls: Array[Control] = []
var _screen_position: Vector2
var _has_pending: bool = false


func configure(board: BoardScript, camera: Camera3D, blocked_controls: Array[Control] = []) -> void:
	_board = board
	_camera = camera
	_blocked_controls = blocked_controls
	clear_pending()


func clear_pending() -> void:
	_has_pending = false


func _unhandled_input(event: InputEvent) -> void:
	if _board == null or _camera == null or _has_pending:
		return
	var screen_position: Vector2
	if event is InputEventScreenTouch:
		if not event.pressed or event.canceled or event.index != 0:
			return
		screen_position = event.position
	elif event is InputEventMouseButton:
		# O toque já é tratado acima: ignora seu mouse emulado, sem duplicar pedidos.
		if event.device == InputEvent.DEVICE_ID_EMULATION:
			return
		if not event.pressed or event.button_index != MOUSE_BUTTON_LEFT:
			return
		screen_position = event.position
	else:
		return
	for control in _blocked_controls:
		if is_instance_valid(control) and control.is_visible_in_tree() and control.get_global_rect().has_point(screen_position):
			return
	_screen_position = screen_position
	_has_pending = true
	get_viewport().set_input_as_handled()


func _physics_process(_delta: float) -> void:
	if not _has_pending:
		return
	_has_pending = false
	var origin := _camera.project_ray_origin(_screen_position)
	var end := origin + _camera.project_ray_normal(_screen_position) * _camera.far
	var query := PhysicsRayQueryParameters3D.create(origin, end, BoardScript.BOARD_COLLISION_MASK)
	var hit := _board.get_world_3d().direct_space_state.intersect_ray(query)
	if hit.is_empty():
		return
	var collider: Node = hit["collider"] as Node
	if collider == null or not _board.is_ancestor_of(collider) or not collider.has_meta("board_cell"):
		return
	var cell: Vector2i = collider.get_meta("board_cell")
	if _board.contains_cell(cell):
		cell_requested.emit(cell)
