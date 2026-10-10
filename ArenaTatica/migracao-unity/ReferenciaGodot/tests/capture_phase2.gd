extends SceneTree

const InteractiveScene = preload("res://scenes/arena_interativa.tscn")


func _initialize() -> void:
	_capture.call_deferred()


func _save(filename: String) -> void:
	await process_frame
	await RenderingServer.frame_post_draw
	var result := root.get_texture().get_image().save_png("res://.artifacts/" + filename)
	if result != OK:
		push_error("Erro ao salvar " + filename)
		quit(1)
	print("Prévia salva: " + filename)


func _capture() -> void:
	var session = InteractiveScene.instantiate()
	root.add_child(session)
	await physics_frame
	await physics_frame
	await _save("phase2-inicio.png")
	var camera: Camera3D = session.get_node("Arena/Camera3D")
	var event := InputEventMouseButton.new()
	event.button_index = MOUSE_BUTTON_LEFT
	event.pressed = true
	event.position = camera.unproject_position(session.board.cell_to_world(Vector2i(7, 5)))
	root.push_input(event, true)
	event = event.duplicate()
	event.pressed = false
	root.push_input(event, true)
	await create_timer(4.0).timeout
	if session.player.current_cell != Vector2i(7, 5) or session.player.is_moving:
		push_error("O clique na janela real não terminou na casa esperada")
		quit(1)
		return
	await _save("phase2-movimento.png")
	root.size = Vector2i(720, 1000)
	await _save("phase2-vertical.png")
	print("JANELA REAL: clique moveu a peça de (1, 1) para (7, 5).")
	quit()
