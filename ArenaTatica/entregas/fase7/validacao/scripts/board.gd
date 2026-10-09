extends Node3D
## Apresentação procedural da grelha e conversão entre célula e mundo.

signal board_generated

const GridModel = preload("res://scripts/grid_model.gd")
# Máscaras são bits: camada 1 = 1, camada 2 = 2, camada 3 = 4.
const BOARD_COLLISION_MASK: int = 1 << 0

@export_range(1, 32, 1) var columns: int = 12
@export_range(1, 32, 1) var rows: int = 12
@export_range(0.5, 4.0, 0.1) var cell_size: float = 1.0
@export_range(0.1, 1.0, 0.05) var tile_height: float = 0.25

var model: GridModel = GridModel.new()
var _tiles_root: Node3D


func _ready() -> void:
	generate_board()


func generate_board() -> void:
	# @export_range limita o Inspector; estas linhas também validam código.
	columns = clampi(columns, 1, 32)
	rows = clampi(rows, 1, 32)
	cell_size = clampf(cell_size, 0.5, 4.0)
	tile_height = clampf(tile_height, 0.1, 1.0)

	if is_instance_valid(_tiles_root):
		remove_child(_tiles_root)
		_tiles_root.queue_free()

	model.configure(columns, rows)
	_tiles_root = Node3D.new()
	_tiles_root.name = "Tiles"
	add_child(_tiles_root)

	# Malha, forma e materiais são compartilhados entre todos os blocos.
	var tile_mesh := BoxMesh.new()
	tile_mesh.size = Vector3(cell_size * 0.96, tile_height, cell_size * 0.96)
	var tile_shape := BoxShape3D.new()
	tile_shape.size = tile_mesh.size

	var light_material := StandardMaterial3D.new()
	light_material.albedo_color = Color(0.65, 0.70, 0.77)
	light_material.roughness = 1.0
	var dark_material := StandardMaterial3D.new()
	dark_material.albedo_color = Color(0.30, 0.36, 0.44)
	dark_material.roughness = 1.0

	for cell in model.get_cells_in_order():
		var body := StaticBody3D.new()
		body.name = "Cell_%d_%d" % [cell.x, cell.y]
		body.collision_layer = BOARD_COLLISION_MASK
		body.collision_mask = 0
		body.set_meta("board_cell", cell)
		_tiles_root.add_child(body)
		body.position = cell_to_local(cell) - Vector3(0.0, tile_height * 0.5, 0.0)

		var visual := MeshInstance3D.new()
		visual.name = "Visual"
		visual.mesh = tile_mesh
		if (cell.x + cell.y) % 2 == 0:
			visual.material_override = light_material
		else:
			visual.material_override = dark_material
		body.add_child(visual)

		var collision := CollisionShape3D.new()
		collision.name = "Collision"
		collision.shape = tile_shape
		body.add_child(collision)

	board_generated.emit()


func contains_cell(cell: Vector2i) -> bool:
	return model.contains_cell(cell)


func cell_to_local(cell: Vector2i) -> Vector3:
	var offset_x: float = float(columns - 1) * cell_size * 0.5
	var offset_z: float = float(rows - 1) * cell_size * 0.5
	# Devolve o centro do TOPO da célula: Y = 0.
	return Vector3(cell.x * cell_size - offset_x, 0.0, cell.y * cell_size - offset_z)


func cell_to_world(cell: Vector2i) -> Vector3:
	return to_global(cell_to_local(cell))


func world_to_cell(world_position: Vector3) -> Vector2i:
	var point := to_local(world_position)
	var offset_x: float = float(columns - 1) * cell_size * 0.5
	var offset_z: float = float(rows - 1) * cell_size * 0.5
	# Conversão lógica: não é um teste de colisão e inclui os vãos visuais.
	# O chamador sempre deve conferir contains_cell(resultado).
	return Vector2i(
		floori((point.x + offset_x) / cell_size + 0.5),
		floori((point.z + offset_z) / cell_size + 0.5)
	)


func get_local_bounds() -> AABB:
	var width: float = columns * cell_size
	var depth: float = rows * cell_size
	return AABB(
		Vector3(-width * 0.5, -tile_height, -depth * 0.5),
		Vector3(width, tile_height, depth)
	)

