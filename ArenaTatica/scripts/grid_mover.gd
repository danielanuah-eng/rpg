extends Node3D
## Executa um caminho já aprovado. Nunca lê entrada nem escolhe destinos.

signal step_started(origin: Vector2i, destination: Vector2i)
signal step_completed(cell: Vector2i)
signal movement_finished(cell: Vector2i, interrupted: bool)

const BoardScript = preload("res://scripts/board.gd")

@export_range(0.05, 1.0, 0.01) var step_duration: float = 0.28
var current_cell: Vector2i = Vector2i.ZERO
var is_moving: bool = false
var step_guard: Callable
var _board: BoardScript
var _path: Array[Vector2i] = []
var _index: int = 0
var _version: int = 0
var _stop_requested: bool = false
var _tween: Tween


func configure(board: BoardScript, cell: Vector2i) -> void:
	cancel_movement()
	_board = board
	current_cell = cell
	global_position = board.cell_to_world(cell)


func cancel_movement() -> void:
	# Cancelamento imediato para reconstrução/correção de estado; não é a parada tática.
	_version += 1
	if _tween != null and _tween.is_valid():
		_tween.kill()
	_path.clear()
	is_moving = false
	_stop_requested = false
	if _board != null and _board.contains_cell(current_cell):
		global_position = _board.cell_to_world(current_cell)


func execute_path(path: Array[Vector2i]) -> bool:
	if is_moving or _board == null or path.size() < 2 or path[0] != current_cell:
		return false
	for index in range(path.size()):
		if not _board.contains_cell(path[index]):
			return false
		if index > 0:
			var delta: Vector2i = path[index] - path[index - 1]
			if absi(delta.x) + absi(delta.y) != 1:
				return false
	_version += 1
	_path = path.duplicate()
	_index = 1
	_stop_requested = false
	is_moving = true
	_start_step.call_deferred(_version)
	return true


func request_stop() -> void:
	# Termina o passo atual e para sobre uma casa, nunca entre duas casas.
	if is_moving:
		_stop_requested = true


func _start_step(version: int) -> void:
	if version != _version or not is_moving:
		return
	if _index >= _path.size():
		_finish(false)
		return
	if _stop_requested:
		_finish(true)
		return
	var destination: Vector2i = _path[_index]
	if step_guard.is_valid() and not bool(step_guard.call(current_cell, destination)):
		_finish(true)
		return
	step_started.emit(current_cell, destination)
	if version != _version or not is_moving:
		return
	if _stop_requested:
		_finish(true)
		return
	_tween = create_tween().set_process_mode(Tween.TWEEN_PROCESS_PHYSICS)
	_tween.set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	_tween.tween_property(self, "global_position", _board.cell_to_world(destination), maxf(step_duration, 0.01))
	_tween.finished.connect(_on_step_arrived.bind(destination, version))


func _on_step_arrived(destination: Vector2i, version: int) -> void:
	if version != _version or not is_moving:
		return
	current_cell = destination
	global_position = _board.cell_to_world(destination)
	_index += 1
	step_completed.emit(destination)
	_start_step.call_deferred(version)


func _finish(interrupted: bool) -> void:
	is_moving = false
	_path.clear()
	movement_finished.emit(current_cell, interrupted)
