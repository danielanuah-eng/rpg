extends SceneTree
## Exporta as cenas existentes, sem regenerar ou alterar os originais.

func _initialize() -> void:
	_export.call_deferred()

func _export() -> void:
	var destination := "res://migracao-unity/Assets/ArenaTatica/Models"
	DirAccess.make_dir_recursive_absolute(ProjectSettings.globalize_path(destination))
	var report: Array[Dictionary] = []
	for model in ["CiantiaChibi3D", "DragaoDourado3D", "DragaoTransplanar3D"]:
		var source := "res://assets/models/%s.tscn" % model
		var packed := load(source) as PackedScene
		if packed == null:
			push_error("Cena ausente: " + source)
			quit(1)
			return
		var scene := packed.instantiate()
		var document := GLTFDocument.new()
		var state := GLTFState.new()
		var target: String = destination + "/" + str(model) + ".glb"
		var error := document.append_from_scene(scene, state)
		if error == OK:
			error = document.write_to_filesystem(state, ProjectSettings.globalize_path(target))
		scene.free()
		if error != OK:
			push_error("Falha ao exportar %s: %s" % [model, error])
			quit(1)
			return
		# Reabre o arquivo exportado e verifica que existem malhas.
		var imported := GLTFState.new()
		error = document.append_from_file(ProjectSettings.globalize_path(target), imported)
		if error != OK or imported.get_meshes().is_empty():
			push_error("GLB inválido: " + target)
			quit(1)
			return
		report.append({"source": source, "output": target, "meshes": imported.get_meshes().size(), "materials": imported.get_materials().size(), "roundtrip": "passed"})
		print("GLB PASS: %s (%s malhas)" % [model, imported.get_meshes().size()])
	var file := FileAccess.open("res://migracao-unity/modelos-exportados.json", FileAccess.WRITE)
	file.store_string(JSON.stringify(report, "\t"))
	file.close()
	quit(0)
