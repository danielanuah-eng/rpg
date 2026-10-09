extends Node3D
## Modelos sólidos procedurais. Frente local: +Z. Sem texturas ou billboards.

const SCENE_FILES := {
	"ciantia": "res://assets/models/CiantiaChibi3D.tscn",
	"dragon_small": "res://assets/models/DragaoDourado3D.tscn",
	"dragon_large": "res://assets/models/DragaoTransplanar3D.tscn"
}

var design_id := ""
var rig: Node3D
var _wings: Array[Node3D] = []
var _clock := 0.0
var animate := true
var _materials: Dictionary = {}


func build(kind: String, height: float = 1.6, use_saved_scene: bool = true) -> void:
	design_id = kind
	name = "ChibiModel3D"
	set_meta(&"_visual_setup_generated", true)
	rig = Node3D.new()
	rig.name = "Geometry"
	add_child(rig)
	var scene_path: String = SCENE_FILES.get(kind, "")
	if use_saved_scene and ResourceLoader.exists(scene_path):
		var scene := load(scene_path) as PackedScene
		var authored := scene.instantiate() as Node3D
		# O template de respawn empacota todos os descendentes como nós locais.
		# Evite empacotar simultaneamente a instância externa e seus filhos.
		authored.scene_file_path = ""
		rig.add_child(authored)
		for wing_name in ["AsaEsquerda", "AsaDireita"]:
			var matches := authored.find_children(wing_name, "Node3D", true, false)
			if not matches.is_empty():
				_wings.append(matches[0] as Node3D)
	elif kind == "ciantia":
		_build_ciantia()
	else:
		_build_dragon(kind == "dragon_large")
	var bounds := geometry_bounds()
	var factor := height / maxf(bounds.size.y, 0.01)
	rig.scale = Vector3.ONE * factor
	rig.position.y = -bounds.position.y * factor
	set_meta(&"design_id", kind)
	set_meta(&"geometry_height", height)


func _process(delta: float) -> void:
	if not animate:
		return
	_clock += delta
	for index in range(_wings.size()):
		var sign_x := -1.0 if index == 0 else 1.0
		_wings[index].rotation.y = sign_x * (0.20 + sin(_clock * 1.8) * 0.055)


func geometry_bounds() -> AABB:
	var meshes: Array[MeshInstance3D] = []
	_find_meshes(rig, meshes)
	var result := AABB()
	var first := true
	for mesh_node in meshes:
		var relative := mesh_node.transform
		var parent := mesh_node.get_parent()
		while parent != self and parent is Node3D:
			relative = (parent as Node3D).transform * relative
			parent = parent.get_parent()
		var box: AABB = relative * mesh_node.get_aabb()
		result = box if first else result.merge(box)
		first = false
	return result


func _find_meshes(node: Node, output: Array[MeshInstance3D]) -> void:
	if node is MeshInstance3D:
		output.append(node as MeshInstance3D)
	for child in node.get_children():
		_find_meshes(child, output)


func _mat(key: String, color: Color, metal: float = 0.0, glow: float = 0.0) -> StandardMaterial3D:
	if _materials.has(key):
		return _materials[key]
	var material := StandardMaterial3D.new()
	material.albedo_color = color
	material.roughness = 0.36 if metal < 0.4 else 0.28
	material.metallic_specular = 0.45
	material.metallic = metal
	if glow > 0.0:
		material.emission_enabled = true
		material.emission = color
		material.emission_energy_multiplier = glow
	_materials[key] = material
	return material


func _mesh(part_name: String, mesh: Mesh, material: Material, parent: Node3D = null) -> MeshInstance3D:
	var node := MeshInstance3D.new()
	node.name = part_name
	node.mesh = mesh
	node.material_override = material
	(parent if parent != null else rig).add_child(node, true)
	return node


func _oval(part_name: String, position_3d: Vector3, radii: Vector3, material: Material, parent: Node3D = null) -> MeshInstance3D:
	var sphere := SphereMesh.new()
	sphere.radius = 1.0
	sphere.height = 2.0
	sphere.radial_segments = 24
	sphere.rings = 12
	var node := _mesh(part_name, sphere, material, parent)
	node.position = position_3d
	node.scale = radii
	return node


func _link(part_name: String, a: Vector3, b: Vector3, radius_a: float, radius_b: float, material: Material, parent: Node3D = null) -> MeshInstance3D:
	var cylinder := CylinderMesh.new()
	cylinder.bottom_radius = radius_a
	cylinder.top_radius = radius_b
	cylinder.height = a.distance_to(b)
	cylinder.radial_segments = 12
	var node := _mesh(part_name, cylinder, material, parent)
	node.position = (a + b) * 0.5
	node.quaternion = Quaternion(Vector3.UP, (b - a).normalized())
	return node


func _tube(part_name: String, points: Array[Vector3], radii: Array[float], material: Material, parent: Node3D = null, sides: int = 10) -> void:
	var smooth_points: Array[Vector3] = []
	var smooth_radii: Array[float] = []
	for segment in range(points.size() - 1):
		for step in range(5):
			var t := float(step) / 5.0
			smooth_points.append(points[segment].cubic_interpolate(points[segment + 1], points[maxi(segment - 1, 0)], points[mini(segment + 2, points.size() - 1)], t))
			smooth_radii.append(lerpf(radii[segment], radii[segment + 1], t))
	smooth_points.append(points[-1])
	smooth_radii.append(radii[-1])
	points = smooth_points
	radii = smooth_radii
	var vertices: Array[Vector3] = []
	var previous_u := Vector3.RIGHT
	for index in range(points.size()):
		var tangent := points[mini(index + 1, points.size() - 1)] - points[maxi(index - 1, 0)]
		tangent = tangent.normalized()
		var u := (previous_u - tangent * previous_u.dot(tangent)).normalized()
		if u.length_squared() < 0.1:
			u = tangent.cross(Vector3.FORWARD).normalized()
		previous_u = u
		var v := tangent.cross(u).normalized()
		for side in range(sides):
			var angle := TAU * float(side) / float(sides)
			vertices.append(points[index] + (u * cos(angle) + v * sin(angle)) * radii[index])
	var surface := SurfaceTool.new()
	surface.begin(Mesh.PRIMITIVE_TRIANGLES)
	for ring in range(points.size() - 1):
		for side in range(sides):
			var a := ring * sides + side
			var c := ring * sides + (side + 1) % sides
			var b := a + sides
			var d := c + sides
			_tri(surface, vertices[a], vertices[c], vertices[b])
			_tri(surface, vertices[b], vertices[c], vertices[d])
	for side in range(sides):
		var next := (side + 1) % sides
		_tri(surface, points[0], vertices[next], vertices[side])
		var start := (points.size() - 1) * sides
		_tri(surface, points[-1], vertices[start + side], vertices[start + next])
	surface.index()
	surface.generate_normals()
	_mesh(part_name, surface.commit(), material, parent)


func _tri(surface: SurfaceTool, a: Vector3, b: Vector3, c: Vector3) -> void:
	surface.add_vertex(a)
	surface.add_vertex(c)
	surface.add_vertex(b)


func _panel(part_name: String, polygon: PackedVector2Array, depth: float, material: Material, parent: Node3D, z: float = 0.0) -> void:
	var surface := SurfaceTool.new()
	surface.begin(Mesh.PRIMITIVE_TRIANGLES)
	var triangles := Geometry2D.triangulate_polygon(polygon)
	for index in range(0, triangles.size(), 3):
		var a := polygon[triangles[index]]
		var b := polygon[triangles[index + 1]]
		var c := polygon[triangles[index + 2]]
		_tri(surface, Vector3(a.x, a.y, z + depth), Vector3(b.x, b.y, z + depth), Vector3(c.x, c.y, z + depth))
		_tri(surface, Vector3(c.x, c.y, z - depth), Vector3(b.x, b.y, z - depth), Vector3(a.x, a.y, z - depth))
	for index in range(polygon.size()):
		var p := polygon[index]
		var q := polygon[(index + 1) % polygon.size()]
		var a := Vector3(p.x, p.y, z + depth)
		var b := Vector3(q.x, q.y, z + depth)
		var c := Vector3(p.x, p.y, z - depth)
		var d := Vector3(q.x, q.y, z - depth)
		_tri(surface, a, c, b)
		_tri(surface, b, c, d)
	surface.generate_normals()
	_mesh(part_name, surface.commit(), material, parent)


func _build_ciantia() -> void:
	var skin := _mat("skin", Color("f4bd93"))
	var blush := _mat("blush", Color("df977d"))
	var black := _mat("coat", Color("191825"))
	var boots := _mat("leather", Color("292334"))
	var purple := _mat("lining", Color("65268c"))
	var gold := _mat("gold", Color("dba744"), 0.5)
	var hair := _mat("hair", Color("b7914c"))
	var hair_light := _mat("hair_light", Color("dfbd79"))
	var hair_shadow := _mat("hair_shadow", Color("b57a31"))
	var white := _mat("eye_white", Color("fff5e7"))
	var green := _mat("emerald", Color("21935c"), 0.10)
	var ink := _mat("ink", Color("14111e"))
	var lip := _mat("lip", Color("a34352"))
	_oval("Torso", Vector3(0, 0.91, 0), Vector3(0.27, 0.35, 0.20), black)
	_oval("Cintura", Vector3(0, 0.66, 0), Vector3(0.245, 0.09, 0.215), boots)
	_link("CintoDourado", Vector3(-0.23, 0.70, 0.13), Vector3(0.23, 0.70, 0.13), 0.025, 0.025, gold)
	_oval("Fivela", Vector3(0.07, 0.70, 0.225), Vector3(0.060, 0.050, 0.025), gold)
	for sign_x in [-1.0, 1.0]:
		_link("Perna", Vector3(sign_x * 0.13, 0.20, 0), Vector3(sign_x * 0.13, 0.66, 0), 0.095, 0.12, black)
		_oval("Bota", Vector3(sign_x * 0.14, 0.12, 0.085), Vector3(0.125, 0.13, 0.205), boots)
		_link("CanoBota", Vector3(sign_x * 0.14, 0.18, 0), Vector3(sign_x * 0.14, 0.41, 0), 0.115, 0.105, boots)
		_link("FivelaBota", Vector3(sign_x * 0.14 - 0.07, 0.32, 0.105), Vector3(sign_x * 0.14 + 0.07, 0.32, 0.105), 0.015, 0.015, gold)
	_coat(black, purple, gold)
	_link("BraçoElevado", Vector3(-0.22, 1.10, 0), Vector3(-0.45, 0.94, 0.12), 0.13, 0.105, black)
	_link("AntebraçoElevado", Vector3(-0.45, 0.94, 0.12), Vector3(-0.53, 1.13, 0.29), 0.10, 0.09, black)
	_link("PunhoDourado", Vector3(-0.52, 1.105, 0.27), Vector3(-0.54, 1.15, 0.31), 0.101, 0.101, gold)
	_oval("MãoMagia", Vector3(-0.55, 1.185, 0.33), Vector3(0.10, 0.055, 0.09), boots)
	_link("BraçoDireito", Vector3(0.23, 1.08, 0), Vector3(0.40, 0.77, 0.08), 0.125, 0.09, black)
	_link("PunhoDireito", Vector3(0.39, 0.80, 0.075), Vector3(0.41, 0.75, 0.09), 0.095, 0.095, gold)
	_oval("MãoDireita", Vector3(0.42, 0.70, 0.10), Vector3(0.08, 0.10, 0.065), boots)
	_oval("Medalhão", Vector3(0, 1.14, 0.19), Vector3(0.046, 0.057, 0.022), gold)
	# Corpo curto, cabeça escultórica dominante e pose assimétrica.
	for part in rig.get_children():
		if part is Node3D:
			part.position.y *= 0.78
			part.scale.y *= 0.78
	_link("Pescoco", Vector3(0,0.86,0), Vector3(0,1.16,0), 0.115,0.14,skin)
	_oval("MassaCabelo", Vector3(0,1.51,-0.12), Vector3(0.52,0.53,0.40), hair)
	_oval("RostoEsculpido", Vector3(0,1.46,0.06), Vector3(0.435,0.43,0.35), skin)
	for side in [-1.0,1.0]:
		_oval("Orelha", Vector3(side*0.42,1.40,0.04), Vector3(0.085,0.105,0.08), skin)
		_oval("Brinco", Vector3(side*0.44,1.29,0.08), Vector3(0.025,0.05,0.025), gold)
		# Olhos semicerrados: esclera em amêndoa e pálpebra oblíqua, sem brilho de bebê.
		var eye := _oval("Olho", Vector3(side*0.177,1.46,0.384), Vector3(0.122,0.067,0.020), white)
		eye.rotation.z = side * 0.14
		_oval("Iris", Vector3(side*0.174,1.446,0.406), Vector3(0.040,0.046,0.008), green)
		_oval("Pupila", Vector3(side*0.174,1.445,0.414), Vector3(0.019,0.037,0.005), ink)
		var lid := _oval("PalpebraEsculpida", Vector3(side*0.177,1.507,0.401), Vector3(0.133,0.053,0.025), skin)
		lid.rotation.z = side * 0.18
		_tube("Delineado", [Vector3(side*0.06,1.472,0.405),Vector3(side*0.17,1.475,0.425),Vector3(side*0.294,1.508,0.382)], [0.007,0.011,0.012], ink)
		_tube("Sobrancelha", [Vector3(side*0.07,1.572,0.397),Vector3(side*0.18,1.594,0.401),Vector3(side*0.29,1.621,0.347)], [0.022,0.028,0.007], hair_shadow)
	_oval("Nariz", Vector3(0,1.38,0.411), Vector3(0.042,0.058,0.060), skin)
	_tube("Boca", [Vector3(-0.060,1.273,0.36),Vector3(0,1.270,0.38),Vector3(0.065,1.282,0.36)], [0.006,0.011,0.005], lip)
	# Mechas largas: volumes afilados com silhueta contínua, como resina esculpida.
	for index in range(7):
		var angle := lerpf(0.83, TAU-0.83, float(index)/6.0)
		var x := sin(angle)
		var z := cos(angle)
		_tube("MechaEsculpida", [Vector3(x*0.30,1.84,z*0.29-0.09),Vector3(x*0.48,1.55,z*0.38-0.10),Vector3(x*0.46,1.17,z*0.36-0.10),Vector3(x*0.55,0.91,z*0.31-0.09),Vector3(x*0.43,0.84,z*0.26-0.06)], [0.18,0.18,0.15,0.10,0.004], hair, null,16)
	_tube("FranjaPrincipal", [Vector3(0.26,1.88,0.10),Vector3(0.04,1.95,0.26),Vector3(-0.27,1.85,0.34),Vector3(-0.39,1.58,0.30),Vector3(-0.37,1.23,0.27)], [0.19,0.19,0.18,0.12,0.006], hair_light,null,20)
	_tube("FranjaLateral", [Vector3(0.23,1.91,0.09),Vector3(0.42,1.71,0.23),Vector3(0.43,1.32,0.20),Vector3(0.50,1.07,0.17)], [0.16,0.16,0.11,0.004], hair,null,20)
	# Dedos da mão de conjuração e livro de capa rígida junto ao quadril.
	for finger in range(3):
		_tube("DedoLuva", [Vector3(-0.57+finger*0.047,0.95,0.35),Vector3(-0.58+finger*0.047,1.015,0.39)], [0.022,0.017], boots)
	var cover := BoxMesh.new()
	cover.size = Vector3(0.19,0.27,0.08)
	var book := _mesh("Grimorio",cover,purple)
	book.position = Vector3(0.29,0.51,0.17)
	book.rotation.z = -0.15
	_base(0.66)



func _coat(outer: Material, inner: Material, trim: Material) -> void:
	var outside := SurfaceTool.new()
	var inside := SurfaceTool.new()
	outside.begin(Mesh.PRIMITIVE_TRIANGLES)
	inside.begin(Mesh.PRIMITIVE_TRIANGLES)
	var hem: Array[Vector3] = []
	var hem_radii: Array[float] = []
	var segments := 24
	for index in range(segments + 1):
		var angle := lerpf(0.67, TAU - 0.67, float(index) / segments)
		hem.append(Vector3(sin(angle) * 0.48, 0.30 + 0.04 * cos(angle * 2.0), cos(angle) * 0.31))
		hem_radii.append(0.022)
		if index == segments:
			continue
		var next := lerpf(0.67, TAU - 0.67, float(index + 1) / segments)
		var a := Vector3(sin(angle) * 0.235, 0.80, cos(angle) * 0.19)
		var b := Vector3(sin(next) * 0.235, 0.80, cos(next) * 0.19)
		var c := hem[-1]
		var d := Vector3(sin(next) * 0.48, 0.30 + 0.04 * cos(next * 2.0), cos(next) * 0.31)
		_tri(outside, a, c, b)
		_tri(outside, b, c, d)
		var ai := a - Vector3(sin(angle), 0, cos(angle)) * 0.035
		var bi := b - Vector3(sin(next), 0, cos(next)) * 0.035
		var ci := c - Vector3(sin(angle), 0, cos(angle)) * 0.035
		var di := d - Vector3(sin(next), 0, cos(next)) * 0.035
		_tri(inside, ai, bi, ci)
		_tri(inside, bi, di, ci)
		_tri(outside, c, ci, d)
		_tri(outside, d, ci, di)
		_tri(outside, a, b, ai)
		_tri(outside, b, bi, ai)
		if index == 0:
			_tri(outside, a, ai, c)
			_tri(outside, c, ai, ci)
		if index == segments - 1:
			_tri(outside, b, d, bi)
			_tri(outside, d, di, bi)
	outside.generate_normals()
	inside.generate_normals()
	_mesh("CasacoComEspessura", outside.commit(), outer)
	_mesh("ForroRoxo", inside.commit(), inner)
	_tube("BainhaDourada", hem, hem_radii, trim, null, 6)
	for sign_x in [-1.0, 1.0]:
		_tube("BordaCasaco", [Vector3(sign_x * 0.18, 1.13, 0.15), Vector3(sign_x * 0.11, 0.92, 0.21), Vector3(sign_x * 0.15, 0.78, 0.16), Vector3(sign_x * 0.30, 0.31, 0.25)], [0.024, 0.022, 0.023, 0.025], trim)


func _build_dragon(large: bool) -> void:
	var body := _mat("scales", Color("5264ae") if large else Color("c59132"), 0.18)
	var light := _mat("chest", Color("92aed2") if large else Color("dfbd70"), 0.10)
	var dark := _mat("wing", Color("7262a6") if large else Color("925d29"), 0.08)
	var edge := _mat("bones", Color("afc5e7") if large else Color("e6c584"), 0.25)
	var eyes := _mat("iris", Color("39c8e1") if large else Color("729a57"), 0.1, 0.18)
	var ink := _mat("ink", Color("11192c"))
	_oval("TroncoHorizontal", Vector3(0,0.72,-0.18), Vector3(0.38,0.31,0.57), body)
	_tube("Pescoco", [Vector3(0,0.78,0.37),Vector3(0,1.05,0.52),Vector3(0,1.28,0.63)], [0.29,0.235,0.20], body)
	_dragon_head(body)
	for sign_x in [-1.0,1.0]:
		_oval("Orbita", Vector3(sign_x*0.25,1.385,0.925), Vector3(0.085,0.031,0.065), ink)
		_oval("OlhoReptiliano", Vector3(sign_x*0.26,1.385,0.951), Vector3(0.062,0.018,0.047), eyes)
		_oval("PupilaVertical", Vector3(sign_x*0.26,1.385,0.989), Vector3(0.007,0.022,0.011), ink)
		_oval("Narina", Vector3(sign_x*0.09,1.307,1.282), Vector3(0.021,0.010,0.009), ink)
		_tube("CristaOrbital", [Vector3(sign_x*0.10,1.405,1.06),Vector3(sign_x*0.24,1.46,0.94),Vector3(sign_x*0.32,1.43,0.66)], [0.018,0.035,0.003], body)
		_tube("Chifre", [Vector3(sign_x*0.18,1.46,0.59),Vector3(sign_x*0.27,1.59,0.38),Vector3(sign_x*0.30,1.72,0.09)], [0.061,0.038,0.002], edge)
		for rear in [true,false]:
			var side := "Esquerda" if sign_x < 0 else "Direita"
			var label := ("Traseira" if rear else "Dianteira") + side
			var hip := Vector3(sign_x*0.27,0.72 if rear else 0.80,-0.65 if rear else 0.30)
			var knee := Vector3(sign_x*0.42,0.44,-0.40 if rear else 0.42)
			var ankle := Vector3(sign_x*0.37,0.14,-0.58 if rear else 0.55)
			_tube("Membro"+label, [hip,knee,ankle], [0.21 if rear else 0.17,0.13,0.095], body)
			_oval("Pata"+label, ankle+Vector3(0,-0.055,0.065), Vector3(0.16,0.10,0.20), body)
			for toe in range(3):
				var base := ankle+Vector3((toe-1)*0.075,-0.07,0.19)
				_link("Garra", base, base+Vector3(0,-0.035,0.10), 0.028,0.001,edge)
	_tube("CaudaVolumetrica", [Vector3(0,0.70,-0.68),Vector3(-0.08,0.55,-1.10),Vector3(-0.30,0.38,-1.48),Vector3(-0.62,0.36,-1.65),Vector3(-0.85,0.53,-1.48)], [0.19,0.14,0.095,0.048,0.002], body)
	for index in range(7):
		var z := -0.85+index*0.23
		var y := 0.90 if z < 0.3 else 1.03+(z-0.3)*0.7
		_link("CristaDorsal", Vector3(0,y,z), Vector3(0,y+0.18,z-0.11), 0.062,0.001,edge)
	for sign_x in [-1.0,1.0]:
		var pivot := Node3D.new()
		pivot.name = "AsaEsquerda" if sign_x < 0 else "AsaDireita"
		pivot.position = Vector3(sign_x*0.21,0.93,0.12)
		pivot.rotation = Vector3(-0.25,sign_x*0.20,0)
		rig.add_child(pivot)
		_wings.append(pivot)
		var outline := PackedVector2Array([Vector2(0,0),Vector2(0.25,0.60),Vector2(0.68,0.92),Vector2(1.05,0.86),Vector2(0.92,0.62),Vector2(0.93,0.45),Vector2(1.28,0.26),Vector2(1.08,0.26),Vector2(0.89,0.15),Vector2(0.80,-0.02),Vector2(0.65,0.11),Vector2(0.42,0.12),Vector2(0.24,-0.12),Vector2(0.1,-0.02)])
		if sign_x < 0:
			for index in range(outline.size()):
				outline[index].x *= -1
			outline.reverse()
		_panel("MembranaEspessa",outline,0.018,dark,pivot)
		for target in [Vector3(1.05,0.86,0),Vector3(1.28,0.26,0),Vector3(0.80,-0.02,0),Vector3(0.24,-0.12,0)]:
			_tube("Nervura", [Vector3.ZERO,Vector3(sign_x*0.25,0.60,0),Vector3(sign_x*target.x,target.y,0)], [0.044,0.030,0.004], edge,pivot,8)
	# Proporção de miniatura: postura baixa, quatro apoios largos.
	for part in rig.get_children():
		if part is Node3D:
			part.position.y *= 0.85
			part.scale.y *= 0.85
	if not large:
		# Familiar dourado: focinho mais fino e cauda serpentina.
		for part in rig.get_children():
			if part is MeshInstance3D and (part.name.begins_with("Focinho") or part.name.begins_with("Cabeca")):
				part.scale.x *= 0.80
	_base(1.03)


func _base(radius: float) -> void:
	var material := _mat("base_resina", Color("161923"))
	material.roughness = 0.30
	var disk := CylinderMesh.new()
	disk.top_radius = radius
	disk.bottom_radius = radius * 1.035
	disk.height = 0.08
	disk.radial_segments = 64
	var node := _mesh("BaseMiniatura",disk,material)
	node.position.y = -0.05


func _dragon_head(material: Material) -> void:
	# Focinho em cunha com quinas arredondadas, em vez de esferas empilhadas.
	var section := [Vector2(-0.7,-1),Vector2(0.7,-1),Vector2(1,-0.5),Vector2(1,0.5),Vector2(0.65,1),Vector2(-0.65,1),Vector2(-1,0.5),Vector2(-1,-0.5)]
	var rings := [Vector4(0.48,0.23,0.18,1.33),Vector4(0.70,0.32,0.22,1.33),Vector4(0.96,0.26,0.16,1.29),Vector4(1.23,0.21,0.11,1.26),Vector4(1.32,0.16,0.08,1.26)]
	var vertices: Array[Vector3] = []
	for ring in rings:
		for point in section:
			vertices.append(Vector3(point.x*ring.y,ring.w+point.y*ring.z,ring.x))
	var surface := SurfaceTool.new()
	surface.begin(Mesh.PRIMITIVE_TRIANGLES)
	for ring_index in range(rings.size()-1):
		for side in range(8):
			var a := ring_index*8+side
			var b := ring_index*8+(side+1)%8
			_tri(surface,vertices[a],vertices[b],vertices[a+8])
			_tri(surface,vertices[b],vertices[b+8],vertices[a+8])
	for side in range(8):
		_tri(surface,Vector3(0,1.33,0.48),vertices[(side+1)%8],vertices[side])
		_tri(surface,Vector3(0,1.26,1.32),vertices[32+side],vertices[32+(side+1)%8])
	surface.index()
	surface.generate_normals()
	_mesh("CabecaEsculpida",surface.commit(),material)
