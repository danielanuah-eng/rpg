extends SceneTree

var checks: int = 0
var failures: int = 0


func _initialize() -> void:
	_run.call_deferred()


func check(ok: bool, message: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		push_error(message)


func _run() -> void:
	# Regras isoladas; debounce real coberto pelos testes da Fase 10.
	root.get_node("NetworkManager").input_debounce_ms = 0
	var viewport := SubViewport.new()
	viewport.size = Vector2i(1280, 720)
	viewport.own_world_3d = true
	root.add_child(viewport)
	var arena = load("res://scenes/arena_interativa.tscn").instantiate()
	viewport.add_child(arena)
	var combat = arena.get_node("CombatManager")
	var turns = root.get_node("TurnManager")
	var p1 = combat.stats[1]
	var p2 = combat.stats[2]
	var events: Array[Dictionary] = []
	combat.attack_resolved.connect(func(event: Dictionary): events.append(event))
	await physics_frame
	await physics_frame
	check(p1.hp == 100 and p2.hp == 100 and p1.pa == 2 and p2.pa == 0, "Somente P1 recebe PA no início da partida")
	for destination in [Vector2i(5, 0), Vector2i(0, 5), Vector2i(5, 5), Vector2i(5, 2), Vector2i(2, 5)]:
		check(combat.has_line_of_sight(Vector2i.ZERO, destination), "LoS livre em múltiplas direções")
		check(combat.has_line_of_sight(destination, Vector2i.ZERO), "LoS livre inversa")
	for pair in [[Vector2i(2, 0), Vector2i(5, 0)], [Vector2i(0, 2), Vector2i(0, 5)], [Vector2i(2, 2), Vector2i(5, 5)]]:
		arena.board.model.cells[pair[0]]["blocks_los"] = true
		check(not combat.has_line_of_sight(Vector2i.ZERO, pair[1]), "Obstáculo bloqueia LoS")
		arena.board.model.cells[pair[0]]["blocks_los"] = false
	arena.board.model.cells[Vector2i(1, 0)]["blocks_los"] = true
	check(not combat.has_line_of_sight(Vector2i.ZERO, Vector2i(1, 1)), "Canto fechado impede tiro diagonal")
	arena.board.model.cells[Vector2i(1, 0)]["blocks_los"] = false
	arena.board.model.cells[Vector2i.ZERO]["blocks_los"] = true
	check(combat.has_line_of_sight(Vector2i.ZERO, Vector2i(3, 0)), "Origem não bloqueia o próprio atacante")
	arena.board.model.cells[Vector2i.ZERO]["blocks_los"] = false
	check(not combat.has_line_of_sight(Vector2i.ZERO, Vector2i.ZERO), "LoS recusa a própria casa")
	check(not combat.has_line_of_sight(Vector2i(-1, 0), Vector2i(1, 1)), "LoS recusa origem inválida")
	arena.board.model.cells[Vector2i(5, 5)]["blocks_los"] = true
	combat.request_attack(1, 2)
	check(p2.hp == 100 and p1.pa == 2, "Ataque bloqueado não gasta PA")
	arena.board.model.cells[Vector2i(5, 5)]["blocks_los"] = false
	combat.request_attack(2, 1)
	combat.request_attack(1, 1)
	combat.request_attack(99, 1)
	check(p1.hp == 100 and p2.hp == 100 and p1.pa == 2, "Rejeita turno, autoataque e ID inválidos")
	var turn_version: int = turns.revision
	var combat_version: int = combat.revision
	arena.get_node("UI/TurnPanel/AttackMode").button_pressed = true
	var camera: Camera3D = arena.get_node("Arena/Camera3D")
	var event := InputEventMouseButton.new()
	event.button_index = MOUSE_BUTTON_LEFT
	event.pressed = true
	event.position = camera.unproject_position(arena.board.cell_to_world(Vector2i(22, 22)))
	viewport.push_input(event, true)
	event = event.duplicate()
	event.pressed = false
	viewport.push_input(event, true)
	await physics_frame
	await physics_frame
	check(p2.hp == 85 and p1.pa == 1, "Clique no modo de ataque aplica dano e custo")
	check(arena.player.current_cell == Vector2i(1, 1) and arena.player_2.current_cell == Vector2i(22, 22), "Ataque não dispara movimento")
	check(events.size() == 1 and events[0].size() == 6 and events[0]["dmg"] == 15, "Evento compacto possui dano calculado pelo Host")
	combat._validate_attack(1, 1, 2, turn_version, combat_version)
	check(p2.hp == 85 and p1.pa == 1, "Repetição antiga não duplica dano")
	arena.player.step_duration = 0.01
	arena.request_move(arena.UNIT_ID, Vector2i(2, 1))
	combat.request_attack(1, 2)
	check(p2.hp == 85 and p1.pa == 1, "Ataque durante movimento é rejeitado")
	for frame in range(120):
		await physics_frame
		if not arena.authority.is_busy():
			break
	check(turns.get_state()["pm"] == 2 and p1.pa == 1, "Atualização de PM não restaura PA")
	combat.request_attack(1, 2)
	combat.request_attack(1, 2)
	check(p2.hp == 70 and p1.pa == 0, "Ataque sem PA não causa dano adicional")
	turns.pass_turn()
	check(p2.pa == 2 and p1.pa == 0 and p2.hp == 70, "Início de P2 restaura apenas PA de P2, sem curar")
	combat.request_attack(2, 1)
	check(p1.hp == 85 and p2.pa == 1, "P2 ataca no seu turno")
	turns.pass_turn()
	check(p1.pa == 0 and p2.pa == 1, "Fase neutra não restaura PA humanos")
	turns.pass_turn()
	# A Fase 7 conclui a IA automaticamente; os dois monstros causam 20 de dano.
	await create_timer(2.0).timeout
	check(p1.pa == 2 and p2.pa == 1 and p1.hp == 65, "Novo turno P1 restaura só seus PA após a IA")
	arena._reset_piece()
	check(p1.hp == 100 and p2.hp == 100 and p1.pa == 2 and p2.pa == 0, "Nova partida reinicia HP e PA")
	p2.server_damage(95)
	combat.request_attack(1, 2)
	check(p2.hp == 0 and events.back()["dmg"] == 5 and p1.pa == 1, "Dano letal limitado ao HP restante")
	combat.request_attack(1, 2)
	check(p1.pa == 1, "Não gasta PA atacando alvo derrotado")
	turns.pass_turn()
	check(p2.pa == 0, "Derrotado não recebe PA")
	check(not arena.authority.request_move(arena.UNIT_2, Vector2i(10, 9), "P2"), "Derrotado não se move")
	combat.request_attack(2, 1)
	check(p1.hp == 100, "Derrotado não ataca")
	viewport.queue_free()
	await process_frame
	print("FASE 6 LOCAL: %d verificações, %d falhas." % [checks, failures])
	quit(0 if failures == 0 else 1)



