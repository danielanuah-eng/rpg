extends Node3D
## Apresentação autoritativa: somente coordenadas e IDs viajam pela rede.

signal effect_received(kind: StringName)

const MAX_EFFECTS := 64
@onready var network: Node = get_node("/root/NetworkManager")
@onready var combat: Node = get_node("../CombatManager")
@onready var game: Node = get_node("../GameLoopManager")
@onready var turns: Node = get_node("/root/TurnManager")
var _serial := 0
var _last_serial := 0

func _ready() -> void:
	set_multiplayer_authority(1)
	combat.attack_resolved.connect(_on_attack)
	game.entity_removed.connect(_on_removed)
	turns.turn_started.connect(_on_turn)
	network.session_started.connect(_reset)
	network.session_ended.connect(_reset)

func _reset() -> void:
	_serial = 0
	_last_serial = 0
	for child in get_children():
		child.queue_free()

func _on_attack(event: Dictionary) -> void:
	if not multiplayer.is_server():
		return
	var attacker: Node3D = game._roots.get(int(event["atk"]))
	var target: Node3D = game._roots.get(int(event["tgt"]))
	if is_instance_valid(attacker) and is_instance_valid(target):
		server_effect(&"attack", attacker.global_position + Vector3.UP,
			target.global_position + Vector3.UP, int(event["dmg"]))

func _on_removed(id: int) -> void:
	if not multiplayer.is_server():
		return
	var entity: Node3D = game._roots.get(id)
	if is_instance_valid(entity):
		server_effect(&"death", entity.global_position, entity.global_position)

func _on_turn(_actor: String) -> void:
	server_effect(&"turn", Vector3.ZERO, Vector3.ZERO)

func server_effect(kind: StringName, origin: Vector3, target: Vector3, damage: int = 0) -> void:
	if network._closing or not multiplayer.is_server() or not origin.is_finite() or not target.is_finite():
		return
	if kind not in [&"attack", &"breath", &"death", &"turn"]:
		return
	_serial += 1
	_render(kind, origin, target, damage)
	if network.active:
		for peer in network.clients:
			rpc_effect.rpc_id(peer, _serial, kind, origin, target, damage)

@rpc("authority", "call_remote", "reliable", 0)
func rpc_effect(serial: int, kind: StringName, origin: Vector3, target: Vector3, damage: int) -> void:
	if not network.from_server() or serial <= _last_serial:
		return
	if not origin.is_finite() or not target.is_finite():
		return
	if kind not in [&"attack", &"breath", &"death", &"turn"]:
		return
	_last_serial = serial
	_render(kind, origin, target, damage)

func _render(kind: StringName, origin: Vector3, target: Vector3, damage: int) -> void:
	effect_received.emit(kind)
	if kind == &"turn" or DisplayServer.get_name() == "headless":
		return
	if get_child_count() >= MAX_EFFECTS:
		return
	var root := Node3D.new()
	add_child(root)
	root.global_position = target
	root.child_exiting_tree.connect(func(_child: Node):
		if root.get_child_count() <= 1 and not root.is_queued_for_deletion():
			root.queue_free())
	match kind:
		&"attack":
			_particles(root, Vector3.ZERO, Vector3.UP, false, Color.ORANGE)
		&"death":
			_particles(root, Vector3.UP * 0.5, Vector3.UP, false, Color.PURPLE)
		&"breath":
			var direction := target - origin
			if direction.length_squared() > 0.001:
				_particles(root, origin - target, direction.normalized(), true,
					Color.ORANGE_RED, direction.length())
			_particles(root, Vector3.ZERO, Vector3.UP, false, Color.ORANGE)
	if damage > 0:
		var label := Label3D.new()
		label.text = str(damage)
		label.font_size = 64
		label.pixel_size = 0.008
		label.modulate = Color(1.0, 0.3, 0.2)
		label.outline_size = 12
		label.billboard = BaseMaterial3D.BILLBOARD_ENABLED
		root.add_child(label)
		var tween := label.create_tween().set_parallel(true)
		tween.tween_property(label, "position:y", 1.4, 0.9)
		tween.tween_property(label, "modulate:a", 0.0, 0.9)
		tween.chain().tween_callback(label.queue_free)

func _particles(root: Node3D, offset: Vector3, direction: Vector3,
	breath: bool, tint: Color, distance: float = 1.0) -> void:
	var particles := CPUParticles3D.new()
	particles.emitting = false
	particles.one_shot = true
	particles.amount = 40 if breath else 20
	particles.lifetime = 0.45
	particles.explosiveness = 0.15 if breath else 1.0
	particles.direction = direction
	particles.spread = 7.0 if breath else 100.0
	particles.gravity = Vector3.ZERO if breath else Vector3(0, -2, 0)
	particles.initial_velocity_min = distance / 0.45 if breath else 0.5
	particles.initial_velocity_max = distance / 0.45 if breath else 2.5
	var mesh := SphereMesh.new()
	mesh.radius = 0.055
	mesh.height = 0.11
	mesh.radial_segments = 6
	mesh.rings = 3
	var material := StandardMaterial3D.new()
	material.shading_mode = BaseMaterial3D.SHADING_MODE_UNSHADED
	material.albedo_color = tint
	mesh.material = material
	particles.mesh = mesh
	particles.finished.connect(particles.queue_free)
	root.add_child(particles)
	particles.position = offset
	particles.emitting = true
