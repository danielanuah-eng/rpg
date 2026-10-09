extends SceneTree
## Captura a própria renderização do projeto, sem fotografar o desktop.
## Execute com janela gráfica e --script res://tests/capture_preview.gd.

const ArenaScene = preload("res://scenes/arena.tscn")


func _initialize() -> void:
	_capture.call_deferred()


func _capture() -> void:
	var viewport := SubViewport.new()
	viewport.own_world_3d = true
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	root.add_child(viewport)
	var arena = ArenaScene.instantiate()
	viewport.add_child(arena)
	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path("res://.artifacts"))
	for screen in [Vector2i(1280, 720), Vector2i(720, 1280)]:
		viewport.size = screen
		await process_frame
		await RenderingServer.frame_post_draw
		var filename := "res://.artifacts/preview-%dx%d.png" % [screen.x, screen.y]
		var result := viewport.get_texture().get_image().save_png(filename)
		if result != OK:
			push_error("Não foi possível salvar a prévia: %s" % filename)
			quit(1)
			return
		print("Prévia salva: " + ProjectSettings.globalize_path(filename))
	viewport.queue_free()
	await process_frame
	quit()
