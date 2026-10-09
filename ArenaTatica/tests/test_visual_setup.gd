extends SceneTree


func _initialize() -> void:
	_run.call_deferred()


func _run() -> void:
	var setup := root.get_node("VisualSetup")
	setup.use_models_3d = false
	assert(ProjectSettings.get_setting("autoload/VisualSetup") == "*res://scripts/VisualSetup.gd")
	assert(DirAccess.dir_exists_absolute(ProjectSettings.globalize_path(setup.SPRITES_DIR)))
	for path in [setup.PLAYER_TEXTURE, setup.LARGE_MONSTER_TEXTURE, setup.SMALL_MONSTER_TEXTURE, setup.TILE_TEXTURE]:
		var image := Image.create(16, 32, false, Image.FORMAT_RGBA8)
		image.fill(Color.WHITE)
		setup._textures[path] = ImageTexture.create_from_image(image)
	var stage := Node3D.new()
	root.add_child(stage)
	var player := CharacterBody3D.new()
	player.name = "Jogador1"
	stage.add_child(player)
	var cube := MeshInstance3D.new()
	cube.mesh = BoxMesh.new()
	player.add_child(cube)
	var collision := CollisionShape3D.new()
	collision.shape = BoxShape3D.new()
	player.add_child(collision)
	var giant := MeshInstance3D.new()
	giant.name = "EnemyGrande"
	giant.mesh = SphereMesh.new()
	stage.add_child(giant)
	var tile := MeshInstance3D.new()
	tile.name = "Cell_1_1"
	tile.mesh = BoxMesh.new()
	stage.add_child(tile)
	await process_frame
	await process_frame
	var visual := player.get_meta(setup.SPRITE_META) as Sprite3D
	assert(visual.texture == setup._textures[setup.PLAYER_TEXTURE])
	assert(visual.billboard == BaseMaterial3D.BILLBOARD_FIXED_Y)
	assert(visual.texture_filter == BaseMaterial3D.TEXTURE_FILTER_NEAREST)
	assert(not cube.visible and not collision.disabled)
	assert(tile.visible and tile.material_override.albedo_texture == setup._textures[setup.TILE_TEXTURE])
	var giant_visual := giant.get_meta(setup.SPRITE_META) as Sprite3D
	assert(giant_visual.texture == setup._textures[setup.LARGE_MONSTER_TEXTURE])
	giant.position = Vector3(8, 0, 5)
	setup._process(0.0)
	assert(giant_visual.global_position.is_equal_approx(giant.global_position))
	setup.apply_sprites()
	await process_frame
	assert(player.get_meta(setup.SPRITE_META) == visual)
	assert(player.get_child_count() == 3)
	var small := MeshInstance3D.new()
	small.name = "MonsterPequeno"
	small.mesh = SphereMesh.new()
	stage.add_child(small)
	await process_frame
	await process_frame
	assert(small.get_meta(setup.SPRITE_META).texture == setup._textures[setup.SMALL_MONSTER_TEXTURE])
	giant.queue_free()
	await process_frame
	await process_frame
	await process_frame
	assert(not is_instance_valid(giant_visual))
	stage.queue_free()
	await process_frame
	await process_frame
	print("VISUAL SETUP: Autoload, diretorio, texturas, billboard, colisao, tile, movimento, duplicacao, spawn tardio e limpeza OK")
	quit()
