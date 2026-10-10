extends SceneTree


func _initialize() -> void:
	_capture.call_deferred()


func _capture() -> void:
	var viewport := SubViewport.new()
	viewport.size = Vector2i(1280, 720)
	viewport.own_world_3d = true
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	root.add_child(viewport)
	var arena := load("res://scenes/arena_interativa.tscn").instantiate() as Node3D
	viewport.add_child(arena)
	for frame in range(6):
		await process_frame
	await RenderingServer.frame_post_draw
	var output_path := "res://.artifacts/visual-setup-arena.png"
	assert(viewport.get_texture().get_image().save_png(output_path) == OK)
	print("CAPTURA VISUAL: " + ProjectSettings.globalize_path(output_path))
	viewport.queue_free()
	await process_frame
	quit()
