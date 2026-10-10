extends SceneTree

const Models = preload("res://scripts/ChibiModels.gd")


func _initialize() -> void:
	_run.call_deferred()


func _run() -> void:
	var orphan_baseline := int(Performance.get_monitor(Performance.OBJECT_ORPHAN_NODE_COUNT))
	var setup := root.get_node("VisualSetup")
	assert(setup.use_models_3d)
	var arena := load("res://scenes/arena_interativa.tscn").instantiate() as Node3D
	root.add_child(arena)
	await process_frame
	await process_frame
	for path in ["Player1", "Player2", "Monsters/Monster1", "Monsters/Monster2"]:
		var entity := arena.get_node(path) as Node3D
		var model = entity.get_meta(setup.MODEL_META)
		assert(model is Models)
		if path.begins_with("Monsters/"):
			assert(model.design_id == "dragon_large", "Dourado não é inimigo")
			assert(model.find_children("Pata*", "MeshInstance3D", true, false).size() == 4)
		assert(model.is_visible_in_tree())
		assert(not entity.get_node("Visual").visible)
		assert(model.find_children("*", "Sprite3D", true, false).is_empty())
		assert(model.find_children("*", "CollisionObject3D", true, false).is_empty())
		assert(model.find_children("*", "MeshInstance3D", true, false).size() > 25)
		var box: AABB = model.geometry_bounds()
		assert(box.size.x > 0.3 and box.size.z > 0.2)
		assert(absf(box.size.y - float(entity.get_meta(&"visual_height"))) < 0.02)
		assert(absf(box.position.y) < 0.01)
		setup.apply_sprites()
		await process_frame
		assert(entity.get_meta(setup.MODEL_META) == model)
		var count := 0
		for child in entity.get_children():
			if child is Models:
				count += 1
		assert(count == 1)
	var player := arena.get_node("Player1") as Node3D
	var player_model := player.get_meta(setup.MODEL_META) as Node3D
	var start := player_model.global_position
	player.position.x += 0.5
	setup._process(0.1)
	assert(player_model.global_position.is_equal_approx(start + Vector3(0.5, 0, 0)))
	player.position.x -= 0.5
	var monster := arena.get_node("Monsters/Monster1") as Node3D
	var monster_model := monster.get_meta(setup.MODEL_META) as Node3D
	monster.get_node("Visual").scale = Vector3(1.15, 0.85, 1.15)
	setup._process(0.1)
	assert(monster_model.scale.is_equal_approx(Vector3(1.15, 0.85, 1.15)))
	monster.get_node("Visual").scale = Vector3.ONE
	setup._process(5.0)
	var before := monster_model.global_transform
	arena.get_node("Arena/Camera3D").position += Vector3(3, 0, -2)
	setup._process(0.1)
	assert(monster_model.global_transform.is_equal_approx(before), "Modelo não deve girar para a câmera")
	var giant := arena.get_node("Monsters/Monster2")
	arena.get_node("CombatManager").stats[1002].server_damage(1000)
	for frame in range(4):
		await process_frame
	assert(not is_instance_valid(giant))
	arena.get_node("GameLoopManager").restore_entities()
	for frame in range(4):
		await process_frame
	var restored := arena.get_node("Monsters/Monster2")
	var restored_model = restored.get_meta(setup.MODEL_META)
	assert(restored_model is Models and restored_model.is_visible_in_tree())
	assert(restored_model.rig.get_parent() == restored_model)
	assert(restored_model.design_id == "dragon_large")
	assert(restored.find_children("ChibiModel3D*", "Node3D", false, false).size() == 1)
	arena.queue_free()
	await process_frame
	await process_frame
	# Dois placeholders que são irmãos não podem apagar o modelo um do outro.
	var holder := Node3D.new()
	root.add_child(holder)
	for number in range(2):
		var placeholder := MeshInstance3D.new()
		placeholder.name = "Enemy%d" % number
		placeholder.mesh = SphereMesh.new()
		holder.add_child(placeholder)
	await process_frame
	await process_frame
	assert(holder.get_node("Enemy0").has_meta(setup.MODEL_META))
	assert(holder.get_node("Enemy1").has_meta(setup.MODEL_META))
	assert(is_instance_valid(holder.get_node("Enemy0").get_meta(setup.MODEL_META)))
	assert(is_instance_valid(holder.get_node("Enemy1").get_meta(setup.MODEL_META)))
	holder.queue_free()
	await process_frame
	await process_frame
	assert(int(Performance.get_monitor(Performance.OBJECT_ORPHAN_NODE_COUNT)) <= orphan_baseline, "Não devem restar nós órfãos")
	print("CHIBI 3D: volume, altura, pés, materiais, unicidade, movimento, ataque, câmera, respawn, irmãos e limpeza OK")
	quit()
