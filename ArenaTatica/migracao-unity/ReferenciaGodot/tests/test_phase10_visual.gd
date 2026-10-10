extends SceneTree

func _initialize() -> void:
	_run.call_deferred()

func _run() -> void:
	var arena: Node = load("res://scenes/arena_interativa.tscn").instantiate()
	root.add_child(arena)
	await process_frame
	await process_frame
	var fx: Node = arena.get_node("VisualFXManager")
	var audio: Node = arena.get_node("AudioManager")
	var network: Node = root.get_node("NetworkManager")
	var origin: Vector3 = arena.player.global_position + Vector3.UP
	for index in range(100):
		fx.server_effect(&"breath", origin, origin + Vector3(2, 0, 0), 8)
	assert(audio._players.size() <= 12)
	assert(fx.get_child_count() <= 64)
	await create_timer(2.0).timeout
	assert(fx.get_child_count() == 0, "Partículas e labels liberados ao concluir")
	assert(audio._players.is_empty() and audio.get_child_count() == 0, "Vozes liberadas ao concluir")
	for index in range(20):
		fx.server_effect(&"attack", origin, origin, 1)
	network.close()
	await process_frame
	await process_frame
	assert(fx.get_child_count() == 0 and audio.get_child_count() == 0, "Fechamento limpa efeitos ativos")
	assert(network.allow_input(&"move"))
	for index in range(99):
		assert(not network.allow_input(&"move"))
	await create_timer(0.13).timeout
	assert(network.allow_input(&"move"), "Debounce libera novo comando")
	network.close()
	for index in range(100):
		arena.request_move(arena.UNIT_ID, Vector2i(-1, -1))
	assert(network.suppressed_inputs == 99, "Movimento filtra rajada antes de enviar")
	network.active = true
	network.synced = true
	network._next_probe_ms = Time.get_ticks_msec() + 10000
	network._probes[10000] = {"peer": 1, "time": Time.get_ticks_msec() - 3001}
	network._poll_probes()
	assert(network.debug_metrics()["timeout_percent"] == 100.0, "Sonda expirada registrada")
	assert(network._probes.is_empty(), "Timeout não acumula sondas")
	network.close()
	await RenderingServer.frame_post_draw
	assert(root.get_texture().get_image().save_png("res://.artifacts/phase10/debug.png") == OK)
	print("FASE10 VISUAL: 120 VFX/SFX, limpeza, limites e debounce OK")
	arena.free()
	quit()
