extends SceneTree

const GridModel = preload("res://scripts/grid_model.gd")
const Navigation = preload("res://scripts/grid_navigation.gd")
const InteractiveScene = preload("res://scenes/arena_interativa.tscn")

var _checks: int = 0
var _failures: int = 0


func _initialize() -> void:
	_run.call_deferred()


func _check(condition: bool, description: String) -> void:
	_checks += 1
	if not condition:
		_failures += 1
		push_error("FALHOU: " + description)


func _mouse(viewport: Viewport, point: Vector2, device: int = 0) -> void:
	var event := InputEventMouseButton.new()
	event.device = device
	event.button_index = MOUSE_BUTTON_LEFT
	event.pressed = true
	event.position = point
	viewport.push_input(event, true)
	event = event.duplicate()
	event.pressed = false
	viewport.push_input(event, true)


func _touch(viewport: Viewport, point: Vector2, index: int = 0) -> void:
	var event := InputEventScreenTouch.new()
	event.index = index
	event.pressed = true
	event.position = point
	viewport.push_input(event, true)
	event = event.duplicate()
	event.pressed = false
	viewport.push_input(event, true)


func _settle(player: Node3D) -> void:
	for frame in range(240):
		await physics_frame
		if frame >= 2 and not player.is_moving:
			return
	_check(false, "Movimento precisa terminar dentro do prazo")


func _run() -> void:
	# Regras isoladas; debounce real coberto pelos testes da Fase 10.
	root.get_node("NetworkManager").input_debounce_ms = 0
	var model := GridModel.new()
	model.configure(5, 5)
	var navigation := Navigation.new()
	var path := navigation.find_path(model, Vector2i.ZERO, Vector2i(2, 2))
	_check(path.size() == 5, "Caminho mínimo: 4 passos e 5 nós incluindo a origem")
	for index in range(1, path.size()):
		var delta: Vector2i = path[index] - path[index - 1]
		_check(absi(delta.x) + absi(delta.y) == 1, "Movimentos ortogonais sem saltos")
	_check(navigation.find_path(model, Vector2i(-1, 0), Vector2i.ZERO).is_empty(), "Origem fora do mapa")
	_check(navigation.find_path(model, Vector2i.ZERO, Vector2i(5, 0)).is_empty(), "Destino fora do mapa")
	model.cells[Vector2i(2, 1)]["walkable"] = false
	path = navigation.find_path(model, Vector2i(1, 1), Vector2i(3, 1))
	_check(path.size() == 5 and not path.has(Vector2i(2, 1)), "AStar contorna terreno bloqueado pelo caminho mínimo")
	_check(navigation.find_path(model, Vector2i(1, 1), Vector2i(2, 1)).is_empty(), "Destino bloqueado rejeitado")
	model.configure(5, 5)
	path = navigation.find_path(model, Vector2i(1, 1), Vector2i(3, 1), [Vector2i(2, 1)])
	_check(path.size() == 5 and not path.has(Vector2i(2, 1)), "AStar contorna outra unidade")
	for row in range(5):
		model.cells[Vector2i(2, row)]["walkable"] = false
	_check(navigation.find_path(model, Vector2i(1, 1), Vector2i(3, 1)).is_empty(), "Sem caminho não produz rota parcial")

	var viewport := SubViewport.new()
	viewport.size = Vector2i(1280, 720)
	viewport.own_world_3d = true
	root.add_child(viewport)
	var session = InteractiveScene.instantiate()
	viewport.add_child(session)
	var board = session.board
	var player = session.player
	var authority = session.authority
	# Regressão da Fase 2: testa movimentos sem o limite da Fase 3.
	authority.turns = null
	var camera: Camera3D = session.get_node("Arena/Camera3D")
	player.step_duration = 0.025
	var steps: Array[Vector2i] = []
	var intents: Array[Vector2i] = []
	var rejections: Array[String] = []
	player.step_completed.connect(func(cell: Vector2i): steps.append(cell))
	session.picker.cell_requested.connect(func(cell: Vector2i): intents.append(cell))
	authority.move_rejected.connect(func(_id: StringName, reason: String): rejections.append(reason))
	await physics_frame
	await physics_frame
	# Isola a regressão de movimento do elenco acrescentado na Fase 7.
	for monster_id in [1, 2]:
		authority.unregister_unit(StringName("monster_%d" % monster_id))
	_check(player.current_cell == Vector2i(1, 1), "Peça começa na casa inicial")
	_check(board.columns == 24 and board.rows == 24, "Tabuleiro configurado em 24×24")

	# Exercita o fluxo real de entrada: Viewport -> unhandled_input -> física -> AStar -> Tween.
	var destination := Vector2i(4, 1)
	_mouse(viewport, camera.unproject_position(board.cell_to_world(destination)))
	await _settle(player)
	_check(intents == [destination], "Clique identifica X/Z pelo corpo da célula")
	_check(player.current_cell == destination, "Clique move a peça até o destino")
	_check(steps == [Vector2i(2, 1), Vector2i(3, 1), Vector2i(4, 1)], "Tween percorre cada casa na ordem")
	_check(player.global_position.is_equal_approx(board.cell_to_world(destination)), "Peça termina exatamente no centro")
	_check(authority.get_unit_cell(session.UNIT_ID) == destination, "Estado lógico acompanha casas confirmadas")
	_check(not authority.is_busy(), "Autoridade libera a próxima ação")

	steps.clear()
	intents.clear()
	destination = Vector2i(4, 3)
	var touch_point := camera.unproject_position(board.cell_to_world(destination))
	_touch(viewport, touch_point)
	_mouse(viewport, touch_point, InputEvent.DEVICE_ID_EMULATION)
	await _settle(player)
	_check(intents == [destination], "Toque com mouse emulado produz uma única intenção")
	_check(player.current_cell == destination and steps.size() == 2, "Toque move duas casas")
	intents.clear()
	_touch(viewport, camera.unproject_position(board.cell_to_world(Vector2i(5, 3))), 1)
	_mouse(viewport, Vector2(1, 1))
	_mouse(viewport, session.get_node("UI/InfoPanel").get_global_rect().get_center())
	await _settle(player)
	_check(intents.is_empty() and player.current_cell == destination, "Ignora segundo dedo, fundo e interface")

	_check(not authority.request_move(session.UNIT_ID, destination), "Clicar na própria casa não inicia Tween")
	_check(not authority.request_move(&"desconhecido", Vector2i.ZERO), "Rejeita peça desconhecida")
	_check(not authority.request_move(session.UNIT_ID, Vector2i(99, 99)), "Rejeita destino inválido")
	_check(authority.register_unit(&"other", Vector2i(5, 3)), "Registra ocupação independente de nós visuais")
	_check(not authority.request_move(session.UNIT_ID, Vector2i(5, 3)), "Destino ocupado rejeitado")
	_check(not authority.confirm_step(session.UNIT_ID, Vector2i(8, 8)), "Não confirma passos sem caminho aprovado")

	steps.clear()
	_check(authority.request_move(session.UNIT_ID, Vector2i(4, 6)), "Aprova novo percurso")
	_check(not authority.request_move(session.UNIT_ID, Vector2i(8, 8)), "Segundo clique não substitui Tween em andamento")
	await _settle(player)
	_check(player.current_cell == Vector2i(4, 6) and steps.size() == 3, "Percurso original preservado")

	# Gancho para IA/visão: veta a entrada em uma casa entre dois passos.
	authority.step_rule = func(_id: StringName, _from: Vector2i, to: Vector2i) -> bool: return to != Vector2i(6, 6)
	steps.clear()
	authority.request_move(session.UNIT_ID, Vector2i(7, 6))
	await _settle(player)
	_check(steps == [Vector2i(5, 6)], "Regra externa interrompe antes da casa vetada")
	_check(player.current_cell == Vector2i(5, 6) and authority.get_unit_cell(session.UNIT_ID) == Vector2i(5, 6), "Interrupção mantém visual e lógica sincronizados")
	_check(not authority.is_busy(), "Interrupção libera a autoridade")
	authority.step_rule = Callable()

	# Uma detecção após chegar à casa também pode interromper o restante.
	steps.clear()
	player.step_completed.connect(func(_cell: Vector2i): player.request_stop(), CONNECT_ONE_SHOT)
	authority.request_move(session.UNIT_ID, Vector2i(8, 6))
	await _settle(player)
	_check(steps == [Vector2i(6, 6)] and player.current_cell == Vector2i(6, 6), "Parada solicitada termina sobre uma casa")
	var invalid_jump: Array[Vector2i] = [Vector2i(6, 6), Vector2i(8, 6)]
	var invalid_origin: Array[Vector2i] = [Vector2i(1, 1), Vector2i(1, 2)]
	_check(not player.execute_path(invalid_jump), "Executor recusa salto inválido")
	_check(not player.execute_path(invalid_origin), "Executor recusa origem divergente")

	# Reiniciar o tabuleiro cancela os callbacks antigos e o Tween atual.
	player.step_duration = 0.3
	authority.request_move(session.UNIT_ID, Vector2i(11, 11))
	await physics_frame
	await process_frame
	board.generate_board()
	await create_timer(0.4).timeout
	_check(player.current_cell == Vector2i(1, 1) and not player.is_moving, "Regeneração cancela animação antiga")
	_check(authority.get_unit_cell(session.UNIT_ID) == player.current_cell and not authority.is_busy(), "Regeneração reinicia estado lógico")
	_check(player.global_position.is_equal_approx(board.cell_to_world(player.current_cell)), "Callback antigo não desloca peça reiniciada")

	# Uma confirmação rejeitada deve voltar à posição aceita e permitir tentar de novo.
	player.step_duration = 0.05
	authority.request_move(session.UNIT_ID, Vector2i(3, 1))
	await player.step_started
	authority.finish_move(session.UNIT_ID)
	await _settle(player)
	_check(player.current_cell == Vector2i(1, 1) and player.global_position.is_equal_approx(board.cell_to_world(Vector2i(1, 1))), "Confirmação rejeitada restaura a posição aceita")
	_check(authority.request_move(session.UNIT_ID, Vector2i(2, 1)), "Após correção, peça aceita um novo movimento")
	await _settle(player)
	_check(player.current_cell == Vector2i(2, 1), "Peça não fica travada após rejeição")

	board.model.cells[Vector2i(1, 1)]["walkable"] = false
	session._reset_piece()
	_check(player.current_cell == Vector2i.ZERO and player.visible, "Spawn bloqueado usa a primeira casa livre")
	for cell in board.model.get_cells_in_order():
		board.model.cells[cell]["walkable"] = false
	session._reset_piece()
	_check(not player.visible and not player.is_moving and not authority.is_busy(), "Mapa sem casas livres limpa peça e ação")
	_check(not session.picker.is_processing_unhandled_input(), "Mapa indisponível desabilita pedidos")
	board.generate_board()
	_check(player.visible and player.current_cell == Vector2i(1, 1) and session.picker.is_processing_unhandled_input(), "Reconstrução restaura peça e entrada")

	viewport.queue_free()
	await process_frame
	print("FASE 2: %d verificações, %d falhas." % [_checks, _failures])
	quit(0 if _failures == 0 else 1)

