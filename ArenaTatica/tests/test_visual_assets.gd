extends SceneTree


func _initialize() -> void:
	_run.call_deferred()


func _run() -> void:
	var setup := root.get_node("VisualSetup")
	setup.use_models_3d = false
	for path in [setup.PLAYER_TEXTURE, setup.LARGE_MONSTER_TEXTURE, setup.SMALL_MONSTER_TEXTURE, setup.TILE_TEXTURE]:
		assert(ResourceLoader.exists(path), "Arte ausente: " + path)
		var texture := load(path) as Texture2D
		assert(texture != null and texture.get_width() > 0)
		if path != setup.TILE_TEXTURE:
			assert(texture.get_image().detect_alpha() != Image.ALPHA_NONE, "Sprite sem alfa: " + path)
	var arena := load("res://scenes/arena_interativa.tscn").instantiate() as Node3D
	root.add_child(arena)
	await process_frame
	await process_frame
	for entity_path in ["Player1", "Player2", "Monsters/Monster1", "Monsters/Monster2"]:
		var entity := arena.get_node(entity_path)
		assert(entity.has_meta(setup.SPRITE_META), "Sprite ausente: " + entity_path)
		var sprite := entity.get_meta(setup.SPRITE_META) as Sprite3D
		assert(sprite != null and sprite.is_visible_in_tree())
		assert(sprite.billboard == BaseMaterial3D.BILLBOARD_FIXED_Y)
		assert(not entity.get_node("Visual").visible)
	var tile := arena.get_node("Arena/Board/Tiles/Cell_0_0/Visual") as MeshInstance3D
	assert(tile.material_override.albedo_texture != null)
	assert(tile.is_visible_in_tree())
	arena.queue_free()
	await process_frame
	print("VISUAIS REAIS: quatro PNGs, alfa, sprites dos jogadores/monstros e textura do piso OK")
	quit()
