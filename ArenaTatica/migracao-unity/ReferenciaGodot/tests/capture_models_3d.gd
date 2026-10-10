extends SceneTree

const Models = preload("res://scripts/ChibiModels.gd")
const IDS := ["ciantia", "dragon_small", "dragon_large"]
const TITLES := ["CIANTIA", "DOURADO · SKILLS", "DRAGÃO TRANSPLANAR"]
const FILES := ["CiantiaChibi3D.tscn", "DragaoDourado3D.tscn", "DragaoTransplanar3D.tscn"]


func _initialize() -> void:
	_capture.call_deferred()


func _capture() -> void:
	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path("res://assets/models"))
	var sheet := SubViewport.new()
	sheet.size = Vector2i(1536, 1120)
	sheet.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	root.add_child(sheet)
	var background := ColorRect.new()
	background.color = Color("101622")
	background.size = Vector2(1536, 1120)
	sheet.add_child(background)
	_label(sheet, "MINIATURAS 3D / REVISÃO 03", Vector2(28, 18), 30)
	var angles := [Vector3(1.7, 2.2, 5), Vector3(5, 2.1, 0.10), Vector3(-1.8, 2.2, -5)]
	var view_names := ["FRENTE / 3⁄4", "LATERAL", "COSTAS / 3⁄4"]
	for row in range(3):
		for column in range(3):
			var holder := SubViewportContainer.new()
			holder.position = Vector2(column * 512, 65 + row * 350)
			holder.size = Vector2(512, 304)
			sheet.add_child(holder)
			var viewport := SubViewport.new()
			viewport.size = Vector2i(512, 304)
			viewport.own_world_3d = true
			viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
			holder.add_child(viewport)
			var stage := Node3D.new()
			viewport.add_child(stage)
			var model: Node3D = Models.new()
			model.build(IDS[column], (0.55 if row == 0 else 1.45) if column == 1 else 2.0, false)
			model.animate = false
			stage.add_child(model)
			if row == 0:
				_save_editable(model, FILES[column])
			var environment := WorldEnvironment.new()
			environment.environment = Environment.new()
			environment.environment.background_mode = Environment.BG_COLOR
			environment.environment.background_color = Color("182131")
			environment.environment.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
			environment.environment.ambient_light_color = Color("c4d4ea")
			environment.environment.ambient_light_energy = 0.30
			environment.environment.tonemap_mode = Environment.TONE_MAPPER_FILMIC
			stage.add_child(environment)
			var key := DirectionalLight3D.new()
			key.rotation_degrees = Vector3(-38, -28, 0)
			key.light_energy = 0.48
			key.shadow_enabled = true
			stage.add_child(key)
			var fill := OmniLight3D.new()
			fill.position = Vector3(2, 3, -2)
			fill.omni_range = 8.0
			fill.light_energy = 0.18
			fill.light_color = Color("b8cdff")
			stage.add_child(fill)
			var pedestal := MeshInstance3D.new()
			var disk := CylinderMesh.new()
			disk.top_radius = 0.92
			disk.bottom_radius = 0.94
			disk.height = 0.10
			disk.radial_segments = 48
			pedestal.mesh = disk
			pedestal.position.y = -0.05
			var stone := StandardMaterial3D.new()
			stone.albedo_color = Color("36445b")
			stone.roughness = 0.8
			pedestal.material_override = stone
			stage.add_child(pedestal)
			var camera := Camera3D.new()
			camera.projection = Camera3D.PROJECTION_ORTHOGONAL
			camera.size = 2.8
			stage.add_child(camera)
			camera.position = angles[row]
			camera.look_at(Vector3(0, 1.0, 0))
			camera.current = true
			_label(sheet, TITLES[column] + (" · DETALHE" if column == 1 and row > 0 else "") + "  ·  " + view_names[row], Vector2(column * 512 + 20, 65 + row * 350 + 310), 17)
	for frame in range(10):
		await process_frame
	await RenderingServer.frame_post_draw
	assert(sheet.get_texture().get_image().save_png("res://.artifacts/modelos-chibi-3d.png") == OK)
	sheet.queue_free()
	await process_frame
	print("MODELOS 3D: cenas editáveis e vistas de frente/lado/costas salvas")
	quit()


func _label(parent: Node, text: String, position_2d: Vector2, font_size: int) -> void:
	var label := Label.new()
	label.text = text
	label.position = position_2d
	label.add_theme_font_size_override("font_size", font_size)
	label.add_theme_color_override("font_color", Color("e1e7f1"))
	parent.add_child(label)


func _save_editable(model: Node3D, file: String) -> void:
	var copy := model.duplicate(Node.DUPLICATE_USE_INSTANTIATION) as Node3D
	copy.set_script(null)
	for node in copy.find_children("*", "", true, false):
		node.owner = copy
	var scene := PackedScene.new()
	assert(scene.pack(copy) == OK)
	assert(ResourceSaver.save(scene, "res://assets/models/" + file) == OK)
	copy.free()
