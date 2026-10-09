extends SceneTree
## Execute com --headless --path . --script res://tests/run_tests.gd

const GridModel = preload("res://scripts/grid_model.gd")
const ArenaScene = preload("res://scenes/arena.tscn")

var _checks: int = 0
var _failures: int = 0


func _initialize() -> void:
	_run.call_deferred()


func _check(condition: bool, description: String) -> void:
	_checks += 1
	if not condition:
		_failures += 1
		push_error("FALHOU: " + description)


func _run() -> void:
	var model := GridModel.new()
	model.configure(12, 12)
	_check(model.cells.size() == 144, "Modelo inicial tem 144 células")
	_check(not model.contains_cell(Vector2i(-1, 0)), "Rejeita coluna negativa")
	_check(not model.contains_cell(Vector2i(12, 0)), "Rejeita coluna fora do limite")
	_check(not model.contains_cell(Vector2i(0, 12)), "Rejeita linha fora do limite")
	_check(model.get_cell_data(Vector2i(-1, 0)).is_empty(), "Célula inexistente não inventa dados")
	var data: Dictionary = model.get_cell_data(Vector2i.ZERO)
	data["walkable"] = false
	_check(model.get_cell_data(Vector2i.ZERO)["walkable"], "Cópia de leitura não modifica o mapa")
	model.configure(0, -1)
	_check(model.cells.size() == 1, "Modelo normaliza dimensões inválidas")

	var viewport := SubViewport.new()
	viewport.size = Vector2i(1280, 720)
	viewport.own_world_3d = true
	root.add_child(viewport)
	var arena = ArenaScene.instantiate()
	viewport.add_child(arena)
	var board = arena.get_node("Board")
	var camera: Camera3D = arena.get_node("Camera3D")
	await process_frame

	_check(board.get_node("Tiles").get_child_count() == 576, "Cena cria 576 blocos")
	_check(camera.projection == Camera3D.PROJECTION_ORTHOGONAL, "Câmera ortográfica")
	_check(camera.current, "Câmera ativa")
	var world_environment: WorldEnvironment = arena.get_node("WorldEnvironment")
	_check(world_environment.environment.ambient_light_source == Environment.AMBIENT_SOURCE_COLOR, "Ambiente usa cor, sem depender de Sky")

	for cell in board.model.get_cells_in_order():
		_check(board.world_to_cell(board.cell_to_world(cell)) == cell, "Ida e volta da célula %s" % cell)
		var body: StaticBody3D = board.get_node("Tiles/Cell_%d_%d" % [cell.x, cell.y])
		_check(body.get_meta("board_cell") == cell, "Colisão identifica célula %s" % cell)
		_check(is_equal_approx(body.position.y + board.tile_height * 0.5, 0.0), "Topo no plano Y=0")

	# Testa as bordas lógicas: o máximo é exclusivo.
	var bounds: AABB = board.get_local_bounds()
	var outside: Vector3 = board.to_global(Vector3(bounds.end.x + 0.001, 0.0, 0.0))
	_check(not board.contains_cell(board.world_to_cell(outside)), "Ponto além da borda não é selecionável")

	# Uma arena deslocada/rotacionada não pode quebrar a conversão de células.
	board.position = Vector3(3.0, 2.0, -5.0)
	board.rotation.y = 0.7
	board.scale = Vector3.ONE * 1.5
	for cell in board.model.get_cells_in_order():
		_check(board.world_to_cell(board.cell_to_world(cell)) == cell, "Conversão com transformação %s" % cell)
	board.transform = Transform3D.IDENTITY
	arena.frame_board()

	# O raio físico precisa atingir o bloco e respeitar a máscara de consulta.
	await physics_frame
	await physics_frame
	var center: Vector3 = board.cell_to_world(Vector2i(2, 3))
	var query := PhysicsRayQueryParameters3D.create(center + Vector3.UP * 2.0, center + Vector3.DOWN * 2.0, 1)
	var hit: Dictionary = board.get_world_3d().direct_space_state.intersect_ray(query)
	_check(not hit.is_empty(), "Raio atinge piso")
	if not hit.is_empty():
		_check(hit["collider"].get_meta("board_cell") == Vector2i(2, 3), "Raio identifica célula correta")
		_check(is_equal_approx(hit["position"].y, 0.0), "Raio encontra o topo do piso")
	query.collision_mask = 2
	_check(board.get_world_3d().direct_space_state.intersect_ray(query).is_empty(), "Consulta de obstáculos ignora piso")

	# Após reduzir o mapa, a física deve remover blocos antigos e incluir o novo.
	var removed_center: Vector3 = board.cell_to_world(Vector2i(11, 11))
	await process_frame
	board.columns = 1
	board.rows = 1
	board.generate_board()
	await physics_frame
	await physics_frame
	query = PhysicsRayQueryParameters3D.create(removed_center + Vector3.UP * 2.0, removed_center + Vector3.DOWN * 2.0, 1)
	_check(board.get_world_3d().direct_space_state.intersect_ray(query).is_empty(), "Regeneração remove colisões antigas")
	query = PhysicsRayQueryParameters3D.create(Vector3.UP * 2.0, Vector3.DOWN * 2.0, 1)
	hit = board.get_world_3d().direct_space_state.intersect_ray(query)
	_check(not hit.is_empty(), "Regeneração adiciona colisão nova")
	if not hit.is_empty():
		_check(hit["collider"].get_meta("board_cell") == Vector2i.ZERO, "Colisão nova pertence ao mapa novo")
	await process_frame

	# Tela horizontal, vertical e quadrada; grelha mínima, retangular e máxima.
	for dimensions in [Vector2i(12, 12), Vector2i(10, 6), Vector2i.ONE, Vector2i(32, 32)]:
		board.columns = dimensions.x
		board.rows = dimensions.y
		board.generate_board()
		await process_frame
		_check(board.model.cells.size() == dimensions.x * dimensions.y, "Regeneração do modelo %s" % dimensions)
		_check(board.get_child_count() == 1, "Regeneração não duplica Tiles")
		_check(board.get_node("Tiles").get_child_count() == dimensions.x * dimensions.y, "Regeneração visual %s" % dimensions)
		for screen in [Vector2i(1280, 720), Vector2i(720, 1280), Vector2i(600, 600)]:
			viewport.size = screen
			await process_frame
			var current_bounds: AABB = board.get_local_bounds()
			for index in range(8):
				var corner: Vector3 = board.to_global(current_bounds.get_endpoint(index))
				var pixel: Vector2 = camera.unproject_position(corner)
				var depth: float = -camera.to_local(corner).z
				_check(pixel.x > 0.0 and pixel.x < screen.x and pixel.y > 0.0 and pixel.y < screen.y, "Canto visível: mapa %s, tela %s, canto %d" % [dimensions, screen, index])
				_check(depth > camera.near and depth < camera.far, "Canto dentro dos planos de recorte")

	# Tamanho não unitário precisa manter coerência entre malha e coordenadas.
	board.columns = 3
	board.rows = 5
	board.cell_size = 2.0
	board.tile_height = 0.6
	board.generate_board()
	for cell in board.model.get_cells_in_order():
		_check(board.world_to_cell(board.cell_to_world(cell)) == cell, "Conversão com células de tamanho 2")
	var first: Array[Vector2i] = board.model.get_cells_in_order()
	board.generate_board()
	_check(first == board.model.get_cells_in_order(), "Geração repete a mesma ordem lógica")

	viewport.queue_free()
	await process_frame
	print("FASE 1: %d verificações, %d falhas." % [_checks, _failures])
	quit(0 if _failures == 0 else 1)

