extends Node
## Autoridade de mortes e resultado; os atributos continuam na Fase 6.

signal entity_removed(entity_id: int)
signal result_received(losers: PackedInt32Array, winner: int, finished: bool)
signal match_restarted

@onready var arena: Node = get_parent()
@onready var combat: Node = get_node("../CombatManager")
@onready var ai: Node = get_node("../AIManager")
@onready var turns: Node = get_node("/root/TurnManager")
@onready var network: Node = get_node("/root/NetworkManager")

var match_over := false
var winner_id := 0
var dead: Dictionary = {}
var _roots: Dictionary = {}
var _templates: Dictionary = {}
var _pending: Dictionary = {}
var _initialized := false
var _scheduled := false


func _ready() -> void:
	set_multiplayer_authority(1)
	_setup.call_deferred()


func _setup() -> void:
	_register(1, arena.player)
	_register(2, arena.player_2)
	for id in ai._monsters:
		var monster: Node = ai._monsters[id]
		var entity_id: int = 1000 + int(id)
		combat.stats[entity_id] = monster.get_node("Stats")
		combat.UNITS[entity_id] = ai._unit_id(id)
		_register(entity_id, monster)
	_initialized = true
	get_node("../HUD").bind_managers()


func _register(id: int, entity: Node) -> void:
	_roots[id] = entity
	var copy := entity.duplicate(Node.DUPLICATE_SCRIPTS)
	for child in copy.find_children("*", "", true, false):
		child.owner = copy
	var packed := PackedScene.new()
	assert(packed.pack(copy) == OK)
	_templates[id] = packed
	copy.free()
	combat.stats[id].died.connect(_on_death.bind(id))


func _on_death(id: int) -> void:
	if not multiplayer.is_server() or match_over or dead.has(id):
		return
	_pending[id] = true
	if not _scheduled:
		_scheduled = true
		_flush.call_deferred()


func _from_host() -> bool:
	var sender := multiplayer.get_remote_sender_id()
	return (multiplayer.is_server() and sender in [0, 1]) or (
		network.active and not multiplayer.is_server() and sender == 1)


func _flush() -> void:
	_scheduled = false
	if not multiplayer.is_server() or _pending.is_empty():
		return
	var losers := PackedInt32Array()
	var ids := _pending.keys()
	_pending.clear()
	for id in ids:
		if dead.has(id):
			continue
		if id <= 2:
			losers.append(id)
		if network.active:
			rpc_remove.rpc(id)
		else:
			rpc_remove(id)
	var humans: Array[int] = []
	for id in [1, 2]:
		if combat.stats[id].is_alive():
			humans.append(id)
	var monsters_alive := false
	for id in combat.stats:
		if id > 1000 and combat.stats[id].is_alive():
			monsters_alive = true
	var finished: bool = humans.is_empty() or (humans.size() == 1 and not monsters_alive)
	var winner: int = humans[0] if finished and humans.size() == 1 else 0
	if not losers.is_empty() or finished:
		if network.active:
			rpc_result.rpc(losers, winner, finished)
		else:
			rpc_result(losers, winner, finished)
	if not finished and dead.has(1 if turns.get_state()["t"] == "P1" else 2):
		if turns.get_state()["t"] != "IA_NEUTRA":
			turns._advance_turn()


@rpc("authority", "call_local", "reliable", 0)
func rpc_remove(id: int) -> void:
	if not _from_host() or dead.has(id) or not _roots.has(id):
		return
	dead[id] = true
	var entity: Node = _roots[id]
	entity.cancel_movement()
	# Preserva o pequeno estado de atributos para snapshots e HUD pós-morte.
	var stats: Node = combat.stats[id]
	stats.reparent(combat)
	stats.name = "DefeatedStats%d" % id
	arena.authority.unregister_unit(combat.UNITS[id])
	if id <= 2:
		arena.units.erase(combat.UNITS[id])
	else:
		ai.remove_monster(id - 1000)
	entity_removed.emit(id)
	entity.get_parent().remove_child(entity)
	entity.queue_free()
	_roots.erase(id)
	arena._update_coordinates()


@rpc("authority", "call_local", "reliable", 0)
func rpc_result(losers: PackedInt32Array, winner: int, finished: bool) -> void:
	if not _from_host():
		return
	match_over = finished
	winner_id = winner
	if finished:
		turns.match_over = true
		ai.stop_match()
		for entity in _roots.values():
			entity.cancel_movement()
		arena.authority._routes.clear()
		arena.authority._next_indices.clear()
		arena.marker.hide()
	result_received.emit(losers, winner, finished)


func _send_state(peer: int) -> void:
	if not multiplayer.is_server():
		return
	var losers := PackedInt32Array()
	for id in dead:
		rpc_remove.rpc_id(peer, id)
		if id <= 2:
			losers.append(id)
	if not losers.is_empty() or match_over:
		rpc_result.rpc_id(peer, losers, winner_id, match_over)


func restore_entities() -> void:
	if not _initialized:
		return
	ai.stop_match()
	_pending.clear()
	for id in dead:
		var entity: Node = _templates[id].instantiate()
		var unused: Node = entity.get_node("Stats")
		entity.remove_child(unused)
		unused.free()
		var stats: Node = combat.stats[id]
		stats.reparent(entity)
		stats.name = "Stats"
		if id <= 2:
			arena.add_child(entity)
			arena.units[combat.UNITS[id]] = entity
			if id == 1:
				arena.player = entity
			else:
				arena.player_2 = entity
			arena.connect_mover(combat.UNITS[id], entity)
		else:
			arena.get_node("Monsters").add_child(entity)
			ai._monsters[id - 1000] = entity
		_roots[id] = entity
	dead.clear()
	match_over = false
	winner_id = 0
	turns.match_over = false
	match_restarted.emit()
