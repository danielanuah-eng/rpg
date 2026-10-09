extends SceneTree
var arena: Node
var client: bool = false

func _initialize() -> void:
	client = OS.get_cmdline_user_args().has("client")
	_run.call_deferred()

func _run() -> void:
	arena = load("res://scenes/arena_interativa.tscn").instantiate()
	arena.name = "Arena"
	var monsters := Node3D.new()
	monsters.name = "Monsters"
	arena.add_child(monsters)
	for id in [1, 2]:
		var monster = load("res://scripts/grid_mover.gd").new()
		monster.name = "Monster%d" % id
		monster.step_duration = 0.25 if client else 0.05
		monster.set_meta("id", id)
		monster.set_meta("spawn_cell", Vector2i(1, 2) if id == 1 else Vector2i(4, 1))
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
	var network = root.get_node("NetworkManager")
	var turns = root.get_node("TurnManager")
	if client:
		assert(network.join("ws://127.0.0.1:19289") == OK)
		var seen_neutral: bool = false
		var deadline: int = Time.get_ticks_msec() + 30000
		while Time.get_ticks_msec() < deadline:
			await process_frame
			seen_neutral = seen_neutral or turns.get_state()["t"] == "IA_NEUTRA"
			if seen_neutral and turns.get_state()["t"] == "P1":
				assert(arena.get_node("CombatManager").stats[1].hp == 80)
				assert(monsters.get_node("Monster2").current_cell == Vector2i(2, 1))
				assert(not monsters.get_node("Monster2").is_moving)
				assert(not ai._running)
				print("FASE7 RPC CLIENT OK")
				network.close()
				quit()
				return
		push_error("FASE7 RPC CLIENT TIMEOUT")
		quit(1)
	else:
		assert(network.host(19289) == OK)
		var deadline: int = Time.get_ticks_msec() + 30000
		while network.clients.is_empty() and Time.get_ticks_msec() < deadline:
			await process_frame
		assert(not network.clients.is_empty())
		await create_timer(0.2).timeout
		turns._pass()
		turns._pass()
		await create_timer(0.75).timeout
		assert(turns.get_state()["t"] == "IA_NEUTRA", "Host deve aguardar cliente mais lento")
		while turns.get_state()["t"] == "IA_NEUTRA" and Time.get_ticks_msec() < deadline:
			await process_frame
		assert(turns.get_state()["t"] == "P1")
		assert(arena.get_node("CombatManager").stats[1].hp == 80)
		print("FASE7 RPC HOST OK: barreira de animação do cliente respeitada")
		await create_timer(0.5).timeout
		network.close()
		quit()


