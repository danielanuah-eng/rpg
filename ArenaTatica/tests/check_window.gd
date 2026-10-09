extends SceneTree
## Verifica a janela real com as configurações de stretch do projeto.

const ArenaScene = preload("res://scenes/arena.tscn")


func _initialize() -> void:
	_run.call_deferred()


func _run() -> void:
	var arena = ArenaScene.instantiate()
	root.add_child(arena)
	var board = arena.get_node("Board")
	var camera: Camera3D = arena.get_node("Camera3D")
	var checks: int = 0
	for window_size in [Vector2i(1000, 600), Vector2i(600, 900)]:
		root.size = window_size
		await process_frame
		await process_frame
		await RenderingServer.frame_post_draw
		var rect := root.get_visible_rect()
		var actual_ratio: float = float(root.size.x) / float(root.size.y)
		var viewport_ratio: float = rect.size.x / rect.size.y
		if not is_equal_approx(actual_ratio, viewport_ratio):
			push_error("A janela e o viewport têm proporções diferentes")
			quit(1)
			return
		checks += 1
		var bounds: AABB = board.get_local_bounds()
		for index in range(8):
			var corner: Vector3 = board.to_global(bounds.get_endpoint(index))
			if not rect.has_point(camera.unproject_position(corner)):
				push_error("A janela real cortou o tabuleiro")
				quit(1)
				return
			checks += 1
		print("Janela %s: viewport %s, tabuleiro completo." % [root.size, rect.size])
	print("JANELA REAL: %d verificações, 0 falhas." % checks)
	quit()
