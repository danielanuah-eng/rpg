extends SceneTree

func _initialize() -> void:
	_capture.call_deferred()

func _capture() -> void:
	var viewport := SubViewport.new()
	viewport.size = Vector2i(1280, 720)
	viewport.own_world_3d = true
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	root.add_child(viewport)
	var arena = load("res://scenes/arena_interativa.tscn").instantiate()
	viewport.add_child(arena)
	await process_frame
	await process_frame
	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path("res://.artifacts"))
	for size in [Vector2i(1280, 720), Vector2i(800, 600)]:
		viewport.size = size
		await process_frame
		await RenderingServer.frame_post_draw
		var error := viewport.get_texture().get_image().save_png("res://.artifacts/phase8-%d.png" % size.x)
		assert(error == OK)
	var combat: Node = arena.get_node("CombatManager")
	for id in [2, 1001, 1002]:
		combat.stats[id].server_damage(100)
	await process_frame
	await process_frame
	await RenderingServer.frame_post_draw
	assert(viewport.get_texture().get_image().save_png("res://.artifacts/phase8-victory.png") == OK)
	viewport.free()
	print("FASE8 CAPTURAS: OK")
	quit()
