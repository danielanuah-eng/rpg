extends SceneTree

func _initialize() -> void:
	_run.call_deferred()

func _run() -> void:
	var arena = load("res://scenes/arena_interativa.tscn").instantiate()
	var monsters := Node3D.new()
	monsters.name = "Monsters"
	arena.add_child(monsters)
	for id in [1, 2]:
		var monster = load("res://scripts/grid_mover.gd").new()
		monster.name = "Monster%d" % id
		monster.step_duration = 0.05
		monster.set_meta("id", id)
		monster.set_meta("spawn_cell", Vector2i(1, 2) if id == 1 else Vector2i(4, 1))
		monster.set_meta("pm", 3)
		monster.set_meta("damage", 10)
		var visual := Node3D.new()
		visual.name = "Visual"
		monster.add_child(visual)
		monsters.add_child(monster)
	var ai = load("res://scripts/AIManager.gd").new()
	ai.name = "AIManager"
	arena.add_child(ai)
	root.add_child(arena)
	await process_frame
	await process_frame
	var turns = root.get_node("TurnManager")
	assert(arena.authority.get_unit_cell(&"monster_1") == Vector2i(1, 2))
	turns.pass_turn()
	assert(turns.get_state()["t"] == "P2")
	turns.pass_turn()
	assert(turns.get_state()["t"] == "IA_NEUTRA")
	turns.pass_turn()
	assert(turns.get_state()["t"] == "IA_NEUTRA")
	await create_timer(2.0).timeout
	assert(turns.get_state()["t"] == "P1")
	assert(arena.get_node("CombatManager").stats[1].hp == 80)
	assert(arena.authority.get_unit_cell(&"monster_2") == Vector2i(2, 1))
	assert(not monsters.get_node("Monster2").is_moving)
	# Turno seguinte: ambos atacam outra vez; sem sobreposição de células.
	turns.pass_turn()
	turns.pass_turn()
	await create_timer(1.0).timeout
	assert(turns.get_state()["t"] == "P1")
	assert(arena.get_node("CombatManager").stats[1].hp == 60)
	print("FASE7 OK: Chase, Attack, dano, ocupação, bloqueio manual e retorno P1")
	quit()
