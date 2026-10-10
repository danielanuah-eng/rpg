extends RefCounted
## Caminhos em coordenadas lógicas (X, Z), sem câmera ou animação.

const GridModel = preload("res://scripts/grid_model.gd")


func find_path(model: GridModel, origin: Vector2i, destination: Vector2i,
		occupied: Array[Vector2i] = []) -> Array[Vector2i]:
	if not model.contains_cell(origin) or not model.contains_cell(destination):
		return []
	if not model.get_cell_data(origin).get("walkable", false):
		return []
	if not model.get_cell_data(destination).get("walkable", false):
		return []
	if occupied.has(destination):
		return []
	var astar := AStarGrid2D.new()
	astar.region = Rect2i(0, 0, model.columns, model.rows)
	astar.diagonal_mode = AStarGrid2D.DIAGONAL_MODE_NEVER
	astar.default_compute_heuristic = AStarGrid2D.HEURISTIC_MANHATTAN
	astar.default_estimate_heuristic = AStarGrid2D.HEURISTIC_MANHATTAN
	# Sem saltos: cada nó devolvido corresponde a uma casa percorrida.
	astar.jumping_enabled = false
	astar.update()
	for cell in model.get_cells_in_order():
		var blocked: bool = not model.get_cell_data(cell).get("walkable", false)
		astar.set_point_solid(cell, blocked or occupied.has(cell))
	return astar.get_id_path(origin, destination, false)
