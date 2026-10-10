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
	assert(audio._streams.size() == 4)
	var origin: Vector3 = arena.player.global_position + Vector3.UP
	var target: Vector3 = origin + Vector3(2, 0, 0)
	for kind in [&"attack", &"breath", &"death"]:
		fx.server_effect(kind, origin, target, 23)
	assert(audio._players.any(func(player: AudioStreamPlayer): return player.playing), "SFX iniciado")
	await create_timer(0.15).timeout
	assert(fx.get_child_count() == 3, "Três efeitos renderizados")
	assert(audio._players.size() <= 12, "Limite de vozes nativas")
	await RenderingServer.frame_post_draw
	var screenshot := root.get_texture().get_image()
	assert(screenshot.save_png("res://.artifacts/phase9/vfx.png") == OK)
	await create_timer(1.5).timeout
	assert(fx.get_child_count() == 0, "Efeitos temporários liberados")
	assert(audio._players.is_empty(), "Vozes temporárias liberadas")
	print("FASE9 VISUAL: VFX, áudio e limpeza OK")
	arena.free()
	quit()
