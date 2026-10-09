extends SceneTree


func _initialize() -> void:
	_capture.call_deferred()


func _capture() -> void:
	var viewport := SubViewport.new()
	viewport.size = Vector2i(1280, 720)
	viewport.own_world_3d = true
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	root.add_child(viewport)
	var session = load("res://scenes/arena_interativa.tscn").instantiate()
	viewport.add_child(session)
	await process_frame
	await process_frame
	await RenderingServer.frame_post_draw
	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path("res://.artifacts"))
	var result: Error = viewport.get_texture().get_image().save_png("res://.artifacts/phase5.png")
	print("FASE 5 PREVIA: ", error_string(result))
	viewport.queue_free()
	await process_frame
	quit(0 if result == OK else 1)
