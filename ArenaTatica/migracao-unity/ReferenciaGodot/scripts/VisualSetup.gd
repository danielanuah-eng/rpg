# VisualSetup.gd — Godot 4
# Adicione em Projeto > Configurações do Projeto > Autoload.
# As quatro imagens PNG devem ser fornecidas em res://assets/sprites/.
extends Node

const ChibiModels = preload("res://scripts/ChibiModels.gd")
const MODEL_META := &"_visual_setup_model"
@export var use_models_3d := true
var _models: Array[Dictionary] = []

const SPRITES_DIR := "res://assets/sprites/"
const PLAYER_TEXTURE := SPRITES_DIR + "maga_chibi.png"
const LARGE_MONSTER_TEXTURE := SPRITES_DIR + "dragao_grande.png"
const SMALL_MONSTER_TEXTURE := SPRITES_DIR + "dragao_pequeno.png"
const TILE_TEXTURE := SPRITES_DIR + "tile_pedra.png"

const PLAYER_NAMES := ["player", "p1", "p2", "jogador"]
const MONSTER_NAMES := ["monster", "inimigo", "enemy", "esfera"]
const TILE_NAMES := ["tile", "cell"]
const LARGE_NAMES := ["grande", "large", "big", "boss", "chefe", "gigante"]
const SMALL_NAMES := ["pequeno", "small", "mini"]

const SPRITE_META := &"_visual_setup_sprite"
const GENERATED_META := &"_visual_setup_generated"
const TILE_META := &"_visual_setup_tile"

# Na ausência de indicação no nome ou metadado, usa o tamanho em
# unidades de mundo. Ajuste este limite conforme a escala do jogo.
@export var large_monster_threshold: float = 2.5

var _textures: Dictionary = {}
var _warned_paths: Dictionary = {}
var _tile_material: StandardMaterial3D
var _scan_pending := false
var _sibling_visuals: Array[Dictionary] = []


func _process(_delta: float) -> void:
	_update_models(_delta)
	for index in range(_sibling_visuals.size() - 1, -1, -1):
		var entry: Dictionary = _sibling_visuals[index]
		var source = entry["source"]
		var visual = entry["visual"]
		if not is_instance_valid(source) or not is_instance_valid(visual):
			if is_instance_valid(visual):
				visual.queue_free()
			_sibling_visuals.remove_at(index)
			continue
		if not source.is_inside_tree() or source.is_queued_for_deletion():
			visual.visible = false
			continue
		var bounds: AABB = source.global_transform * source.get_aabb()
		visual.global_position = bounds.get_center()
		var height := maxf(float(source.get_meta(&"visual_height", bounds.size.y)), 0.1)
		visual.global_position.y = bounds.position.y + height * 0.5
		visual.pixel_size = height / float(visual.texture.get_height())
		visual.visible = true


func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	_ensure_directory()
	get_tree().node_added.connect(_on_node_added)
	_schedule_scan()


func _ensure_directory() -> void:
	var absolute_path := ProjectSettings.globalize_path(SPRITES_DIR)
	if DirAccess.dir_exists_absolute(absolute_path):
		return

	var error := DirAccess.make_dir_recursive_absolute(absolute_path)
	if error != OK:
		push_warning(
			"VisualSetup: não foi possível criar '%s': %s. "
			% [SPRITES_DIR, error_string(error)]
			+ "Em builds exportadas, res:// pode ser somente leitura; "
			+ "inclua as artes no projeto antes de exportar."
		)


# Também pode ser chamado após alterar nomes, tamanhos ou imagens.
func apply_sprites() -> void:
	_schedule_scan()


func _on_node_added(node: Node) -> void:
	if node == self or node.has_meta(GENERATED_META):
		return
	var ancestor := node.get_parent()
	while ancestor != null:
		if ancestor.has_meta(GENERATED_META):
			return
		ancestor = ancestor.get_parent()
	_schedule_scan()


func _schedule_scan() -> void:
	if _scan_pending:
		return
	_scan_pending = true
	call_deferred("_scan_tree")


func _scan_tree() -> void:
	_scan_pending = false
	if not is_inside_tree():
		return

	var entities: Dictionary = {}
	_collect_meshes(get_tree().root, entities)

	for key in entities:
		var entity := key as Node3D
		if not is_instance_valid(entity) or entity.is_queued_for_deletion():
			continue
		var meshes: Array = entities[key]
		_apply_entity(entity, meshes)


func _collect_meshes(node: Node, entities: Dictionary) -> void:
	if node == self or node.is_queued_for_deletion():
		return
	if node.has_meta(GENERATED_META):
		return

	if node is MeshInstance3D:
		var mesh_node := node as MeshInstance3D
		if mesh_node.mesh != null:
			var entity := _find_entity(mesh_node)
			if entity != null:
				var kind := _classify(entity)
				if kind == "tile":
					_apply_tile(mesh_node)
				elif (
					mesh_node.mesh is BoxMesh
					or mesh_node.mesh is SphereMesh
				):
					if not entities.has(entity):
						entities[entity] = []
					entities[entity].append(mesh_node)

	for child in node.get_children():
		_collect_meshes(child, entities)


func _find_entity(mesh_node: MeshInstance3D) -> Node3D:
	var current: Node = mesh_node
	var candidate: Node3D = null
	var candidate_kind := ""

	while current != null and current != get_tree().root:
		# Não agrupa entidades diferentes através de um contêiner genérico.
		if current != mesh_node and current is PhysicsBody3D:
			if _classify(current) != "":
				return current as Node3D
			if candidate != null:
				return candidate

		if current is Node3D:
			var kind := _classify(current)
			if kind != "":
				if candidate == null:
					candidate = current as Node3D
					candidate_kind = kind
				elif kind != candidate_kind:
					return candidate

				# Um corpo físico é uma raiz inequívoca da entidade.
				if current is PhysicsBody3D:
					return current as Node3D

		current = current.get_parent()

	return candidate


func _classify(node: Node) -> String:
	var normalized := String(node.name).to_lower()
	if _contains_any(normalized, PLAYER_NAMES):
		return "player"
	if _contains_any(normalized, MONSTER_NAMES):
		return "monster"
	if _contains_any(normalized, TILE_NAMES):
		return "tile"
	return ""


func _contains_any(value: String, fragments: Array) -> bool:
	for fragment in fragments:
		if value.contains(String(fragment)):
			return true
	return false


func _apply_entity(entity: Node3D, meshes: Array) -> void:
	if meshes.is_empty():
		return
	if use_models_3d:
		_apply_model(entity, meshes)
		return

	# Um MeshInstance3D oculto também oculta seus filhos visuais.
	# Quando a entidade é a própria malha, a sprite vira sua irmã.
	var anchor: Node3D = entity
	if entity is MeshInstance3D:
		anchor = entity.get_parent() as Node3D
		if anchor == null:
			push_warning(
				"VisualSetup: '%s' precisa de um pai Node3D para receber a sprite."
				% entity.get_path()
			)
			return

	if is_zero_approx(anchor.global_transform.basis.determinant()):
		return

	var bounds := _combined_bounds(meshes, anchor)
	var texture_path := PLAYER_TEXTURE

	if _classify(entity) == "monster":
		if _is_large_monster(entity, meshes):
			texture_path = LARGE_MONSTER_TEXTURE
		else:
			texture_path = SMALL_MONSTER_TEXTURE

	var texture := _load_texture(texture_path)
	if texture == null:
		# Nunca esconde a geometria sem ter uma imagem substituta válida.
		return

	var sprite: Sprite3D = null
	if entity.has_meta(SPRITE_META):
		var existing = entity.get_meta(SPRITE_META)
		if is_instance_valid(existing) and existing is Sprite3D:
			sprite = existing as Sprite3D

	if sprite == null:
		sprite = Sprite3D.new()
		sprite.name = "ChibiVisual"
		sprite.set_meta(GENERATED_META, true)
		anchor.add_child(sprite)
		entity.set_meta(SPRITE_META, sprite)
		if entity is MeshInstance3D:
			sprite.top_level = true
			_sibling_visuals.append({"source": entity, "visual": sprite})

	sprite.texture = texture

	# BILLBOARD_Y_VISIBLE não existe no Godot 4.
	# BILLBOARD_FIXED_Y mantém Y vertical e acompanha a câmera.
	sprite.billboard = BaseMaterial3D.BILLBOARD_FIXED_Y
	sprite.texture_filter = BaseMaterial3D.TEXTURE_FILTER_NEAREST
	sprite.transparent = true
	sprite.shaded = false
	sprite.double_sided = true
	sprite.centered = true
	sprite.no_depth_test = false
	sprite.cast_shadow = GeometryInstance3D.SHADOW_CASTING_SETTING_OFF

	var height := maxf(float(entity.get_meta(&"visual_height", bounds.size.y)), 0.1)
	sprite.pixel_size = height / float(maxi(texture.get_height(), 1))
	sprite.position = bounds.position + bounds.size * 0.5
	sprite.position.y = bounds.position.y + height * 0.5
	sprite.rotation = Vector3.ZERO
	sprite.scale = Vector3.ONE

	for item in meshes:
		var mesh_node := item as MeshInstance3D
		mesh_node.visible = false
	if entity is MeshInstance3D:
		_process(0.0)

	# Somente a representação visual é ocultada.
	# Corpos físicos, CollisionShape3D e scripts permanecem intactos.


func _apply_model(entity: Node3D, meshes: Array) -> void:
	var anchor: Node3D = entity.get_parent() as Node3D if entity is MeshInstance3D else entity
	if anchor == null or is_zero_approx(anchor.global_transform.basis.determinant()):
		return
	if entity.has_meta(MODEL_META):
		var existing = entity.get_meta(MODEL_META)
		if is_instance_valid(existing) and existing is ChibiModels and existing.get_parent() == anchor and existing.get_meta(&"source_id", 0) == entity.get_instance_id() and is_instance_valid(existing.rig) and existing.rig.get_parent() == existing:
			return
		entity.remove_meta(MODEL_META)
		if is_instance_valid(existing) and existing is Node and existing.get_parent() == null and existing.has_meta(GENERATED_META):
			existing.free()
	# Templates de respawn podem copiar a geometria, mas não o rig procedural.
	for child in anchor.get_children():
		if child is ChibiModels and (anchor == entity or child.get_meta(&"source_name", "") == String(entity.name)):
			anchor.remove_child(child)
			child.queue_free()
	if entity.has_meta(SPRITE_META):
		var previous = entity.get_meta(SPRITE_META)
		if is_instance_valid(previous) and previous is Sprite3D:
			previous.queue_free()
		entity.remove_meta(SPRITE_META)
	var kind := "ciantia"
	if _classify(entity) == "monster":
		kind = "dragon_large" # Dourado reservado às skills da Ciantia.
	var bounds := _combined_bounds(meshes, anchor)
	var height := maxf(float(entity.get_meta(&"visual_height", bounds.size.y)), 0.1)
	var model = ChibiModels.new()
	model.build(kind, height)
	model.set_meta(&"source_id", entity.get_instance_id())
	model.set_meta(&"source_name", String(entity.name))
	model.rotation.y = 0.25
	anchor.add_child(model)
	model.position = Vector3(bounds.get_center().x, bounds.position.y, bounds.get_center().z)
	entity.set_meta(MODEL_META, model)
	var source := meshes[0] as MeshInstance3D
	_models.append({
		"entity": entity, "model": model, "source": source,
		"rest_scale": source.scale, "last_position": entity.global_position,
		"heading": model.rotation.y, "sibling": entity is MeshInstance3D
	})
	if entity is MeshInstance3D:
		model.top_level = true
	for item in meshes:
		(item as MeshInstance3D).visible = false
	_update_models(0.0)


func _update_models(delta: float) -> void:
	for index in range(_models.size() - 1, -1, -1):
		var entry: Dictionary = _models[index]
		var entity = entry["entity"]
		var model = entry["model"]
		var source = entry["source"]
		if not is_instance_valid(entity) or not is_instance_valid(model) or not is_instance_valid(source):
			if is_instance_valid(model):
				model.queue_free()
			_models.remove_at(index)
			continue
		if not entity.is_inside_tree() or entity.is_queued_for_deletion():
			model.visible = false
			continue
		model.visible = true
		if bool(entry["sibling"]):
			var box: AABB = entity.global_transform * source.get_aabb()
			model.global_position = Vector3(box.get_center().x, box.position.y, box.get_center().z)
		# A IA anima a escala do placeholder Visual; espelhe só essa animação.
		var rest: Vector3 = entry["rest_scale"]
		var current: Vector3 = source.scale
		model.scale = Vector3(current.x / maxf(absf(rest.x), 0.001), current.y / maxf(absf(rest.y), 0.001), current.z / maxf(absf(rest.z), 0.001))
		var displacement: Vector3 = entity.global_position - entry["last_position"]
		displacement.y = 0.0
		# Saltos de spawn/sincronização não são passos de movimentação.
		if displacement.length_squared() > 0.000001 and displacement.length_squared() <= 1.0:
			var parent_yaw: float = 0.0 if bool(entry["sibling"]) else entity.global_rotation.y
			entry["heading"] = atan2(displacement.x, displacement.z) - parent_yaw
		model.rotation.y = lerp_angle(model.rotation.y, float(entry["heading"]), 1.0 - exp(-delta * 12.0))
		entry["last_position"] = entity.global_position


func _combined_bounds(meshes: Array, anchor: Node3D) -> AABB:
	var inverse := anchor.global_transform.affine_inverse()
	var result := AABB()
	var initialized := false

	for item in meshes:
		var mesh_node := item as MeshInstance3D
		var relative := inverse * mesh_node.global_transform
		var transformed: AABB = relative * mesh_node.get_aabb()

		if initialized:
			result = result.merge(transformed)
		else:
			result = transformed
			initialized = true

	return result


func _is_large_monster(entity: Node3D, meshes: Array) -> bool:
	# Defina o metadado booleano "visual_large" para classificação explícita.
	if entity.has_meta(&"visual_large"):
		return bool(entity.get_meta(&"visual_large"))

	var normalized := String(entity.name).to_lower()
	if _contains_any(normalized, LARGE_NAMES):
		return true
	if _contains_any(normalized, SMALL_NAMES):
		return false
	for item in meshes:
		var current: Node = item as Node
		while current != null and current != entity:
			var mesh_name := String(current.name).to_lower()
			if _contains_any(mesh_name, LARGE_NAMES):
				return true
			if _contains_any(mesh_name, SMALL_NAMES):
				return false
			current = current.get_parent()

	var bounds := AABB()
	var initialized := false

	for item in meshes:
		var mesh_node := item as MeshInstance3D
		var transformed: AABB = (
			mesh_node.global_transform * mesh_node.get_aabb()
		)
		if initialized:
			bounds = bounds.merge(transformed)
		else:
			bounds = transformed
			initialized = true

	var largest_dimension := maxf(
		bounds.size.x,
		maxf(bounds.size.y, bounds.size.z)
	)
	return largest_dimension >= large_monster_threshold


func _apply_tile(mesh_node: MeshInstance3D) -> void:
	if mesh_node.has_meta(TILE_META):
		return

	if _tile_material == null:
		var texture := _load_texture(TILE_TEXTURE)
		if texture == null:
			return

		_tile_material = StandardMaterial3D.new()
		_tile_material.albedo_texture = texture
		_tile_material.albedo_color = Color(0.55, 0.55, 0.55, 1.0)
		_tile_material.texture_filter = BaseMaterial3D.TEXTURE_FILTER_NEAREST
		_tile_material.roughness = 1.0

	mesh_node.material_override = _tile_material
	mesh_node.set_meta(TILE_META, true)


func _load_texture(path: String) -> Texture2D:
	if _textures.has(path):
		return _textures[path] as Texture2D

	var texture: Texture2D = null

	# Funciona com recursos importados e incluídos na exportação.
	if ResourceLoader.exists(path):
		texture = ResourceLoader.load(path) as Texture2D

	# Permite PNGs presentes no disco ainda não importados pelo editor.
	if texture == null and FileAccess.file_exists(path):
		var image := Image.new()
		var error := image.load(path)
		if error == OK and not image.is_empty():
			texture = ImageTexture.create_from_image(image)

	if texture != null:
		_textures[path] = texture
		_warned_paths.erase(path)
		return texture

	if not _warned_paths.has(path):
		_warned_paths[path] = true
		push_warning(
			"VisualSetup: textura ausente ou inválida: '%s'. "
			% path
			+ "Adicione o PNG e chame VisualSetup.apply_sprites() novamente."
		)

	return null

