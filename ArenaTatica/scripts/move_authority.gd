extends Node
## Validação local substituível pela autoridade do Host nas fases de rede.
## Não conhece mouse, câmera, nós de peças ou Tweens.

signal path_approved(unit_id: StringName, path: Array[Vector2i])
signal move_rejected(unit_id: StringName, reason: String)
signal step_committed(unit_id: StringName, cell: Vector2i)

const GridModel = preload("res://scripts/grid_model.gd")
const Navigation = preload("res://scripts/grid_navigation.gd")

# Gancho futuro: função(unit_id, origem, próxima_casa) -> bool.
# Executado antes de cada passo. Pode consultar Linha de Visão/IA.
var step_rule: Callable
var turns: Node
var _model: GridModel
var _navigation := Navigation.new()
var _positions: Dictionary = {}
var _owners: Dictionary = {}
var _routes: Dictionary = {}
var _next_indices: Dictionary = {}


func configure(model: GridModel) -> void:
	_model = model
	_positions.clear()
	_owners.clear()
	_routes.clear()
	_next_indices.clear()
	if turns != null:
		turns.reset()


func register_unit(unit_id: StringName, cell: Vector2i, owner: String = "") -> bool:
	if _model == null or _positions.has(unit_id) or not _model.contains_cell(cell):
		return false
	if not _model.get_cell_data(cell).get("walkable", false) or _positions.values().has(cell):
		return false
	_positions[unit_id] = cell
	if not owner.is_empty():
		_owners[unit_id] = owner
	return true


func unregister_unit(unit_id: StringName) -> void:
	_positions.erase(unit_id)
	_owners.erase(unit_id)
	_routes.erase(unit_id)
	_next_indices.erase(unit_id)


func get_unit_cell(unit_id: StringName) -> Vector2i:
	return _positions.get(unit_id, Vector2i(-1, -1))


func is_busy() -> bool:
	return not _routes.is_empty()


func request_move(unit_id: StringName, destination: Vector2i, actor: String = "P1") -> bool:
	if not _positions.has(unit_id):
		return _reject(unit_id, "Peça desconhecida.")
	if _owners.has(unit_id) and _owners[unit_id] != actor:
		return _reject(unit_id, "Esta peça pertence ao outro jogador.")
	if is_busy():
		return _reject(unit_id, "Aguarde a peça terminar o movimento.")
	var origin: Vector2i = _positions[unit_id]
	if destination == origin:
		return _reject(unit_id, "A peça já está nessa casa.")
	var occupied: Array[Vector2i] = []
	for other_id in _positions:
		if other_id != unit_id:
			occupied.append(_positions[other_id])
	var path := _navigation.find_path(_model, origin, destination, occupied)
	if path.size() < 2:
		return _reject(unit_id, "Não existe um caminho livre até essa casa.")
	if turns != null and not turns.begin_move(actor, path):
		return _reject(unit_id, "Turno inválido ou caminho excede os PM disponíveis.")
	_routes[unit_id] = path.duplicate()
	_next_indices[unit_id] = 1
	path_approved.emit(unit_id, path.duplicate())
	return true


func can_enter_step(unit_id: StringName, origin: Vector2i, destination: Vector2i) -> bool:
	if not _routes.has(unit_id) or get_unit_cell(unit_id) != origin:
		return false
	var path: Array[Vector2i] = _routes[unit_id]
	var index: int = _next_indices[unit_id]
	if index >= path.size() or path[index] != destination:
		return false
	if not _model.get_cell_data(destination).get("walkable", false):
		return false
	for other_id in _positions:
		if other_id != unit_id and _positions[other_id] == destination:
			return false
	return not step_rule.is_valid() or bool(step_rule.call(unit_id, origin, destination))


func confirm_step(unit_id: StringName, destination: Vector2i) -> bool:
	if not _routes.has(unit_id):
		return false
	var path: Array[Vector2i] = _routes[unit_id]
	var index: int = _next_indices[unit_id]
	if index >= path.size() or path[index] != destination:
		return false
	if turns != null and not turns.commit_step():
		return false
	_positions[unit_id] = destination
	_next_indices[unit_id] = index + 1
	step_committed.emit(unit_id, destination)
	return true


func finish_move(unit_id: StringName) -> void:
	var had_route: bool = _routes.has(unit_id)
	_routes.erase(unit_id)
	_next_indices.erase(unit_id)
	if had_route and turns != null:
		turns.finish_move()


func _reject(unit_id: StringName, reason: String) -> bool:
	move_rejected.emit(unit_id, reason)
	return false
