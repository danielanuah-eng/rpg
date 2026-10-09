extends Node

signal phase_finished(version: int)
enum State { Idle, Chase, Attack }
const Navigation = preload("res://scripts/grid_navigation.gd")
const PLAYER_IDS = {1: &"player_1", 2: &"player_2"}
const DIRECTIONS = [Vector2i.LEFT, Vector2i.RIGHT, Vector2i.UP, Vector2i.DOWN]

@onready var arena: Node = get_parent()
@onready var turns: Node = get_node("/root/TurnManager")
@onready var network: Node = get_node("/root/NetworkManager")
@onready var combat: Node = get_node("../CombatManager")

var _navigation = Navigation.new()
var _monsters: Dictionary = {}
var _states: Dictionary = {}
var _waiting: Dictionary = {}
var _sequence: int = 0
var _last_received: int = -1
var _epoch: int = 0
var _running: bool = false
var _effect: Tween

func _ready() -> void:
	set_multiplayer_authority(1)
	_setup.call_deferred()

func _setup() -> void:
	for monster in get_node("../Monsters").get_children():
		var id: int = int(monster.get_meta("id", 0))
		assert(id > 0 and not _monsters.has(id), "ID de monstro inválido ou repetido")
		_monsters[id] = monster
	turns.register_neutral_ai(self)
	turns.match_reset.connect(func(): _reset.call_deferred())
	network.client_ready.connect(_send_snapshot)
	multiplayer.peer_disconnected.connect(func(id: int): _waiting.erase(id))
	_reset()

func _unit_id(id: int) -> StringName:
	return StringName("monster_%d" % id)

func _reset() -> void:
	_epoch += 1
	_running = false
	_waiting.clear()
	if _effect != null and _effect.is_valid():
		_effect.kill()
	for id in _monsters:
		var monster: Node = _monsters[id]
		monster.cancel_movement()
		monster.get_node("Visual").scale = Vector3.ONE
		var cell: Vector2i = monster.get_meta("spawn_cell")
		monster.configure(arena.board, cell)
		_states[id] = State.Idle
		if multiplayer.is_server():
			# A Fase 2 não expõe atualização avulsa: somente entradas monster_*.
			arena.authority._positions.erase(_unit_id(id))
			assert(arena.authority.register_unit(_unit_id(id), cell, "IA_NEUTRA"), "Spawn de monstro ocupado ou inválido")
	_sequence += 1
	_last_received = -1
	if multiplayer.is_server() and network.active:
		for peer in network.clients:
			_send_snapshot(peer)

func _send_snapshot(peer: int) -> void:
	if not multiplayer.is_server() or not network.active:
		return
	var positions: Dictionary = {}
	for id in _monsters:
		var cell: Vector2i = arena.authority.get_unit_cell(_unit_id(id))
		positions[id] = cell
	rpc_snapshot.rpc_id(peer, _sequence, positions)

@rpc("authority", "call_remote", "reliable", 0)
func rpc_snapshot(sequence: int, positions: Dictionary) -> void:
	if not network.from_server() or sequence < _last_received:
		return
	_epoch += 1
	if _effect != null and _effect.is_valid():
		_effect.kill()
	for id in positions:
		if _monsters.has(id) and positions[id] is Vector2i:
			_monsters[id].configure(arena.board, positions[id])
			_monsters[id].get_node("Visual").scale = Vector3.ONE
	_last_received = sequence

func start_neutral_turn(version: int) -> void:
	if not multiplayer.is_server() or _running or turns.revision != version:
		return
	if turns.get_state()["t"] != "IA_NEUTRA":
		return
	_running = true
	var epoch: int = _epoch
	var ids: Array = _monsters.keys()
	ids.sort()
	for id in ids:
		if epoch != _epoch:
			return
		var packet: Dictionary = _decide(id)
		_sequence += 1
		packet["s"] = _sequence
		_waiting.clear()
		if network.active:
			for peer in network.clients:
				_waiting[peer] = true
				rpc_action.rpc_id(peer, packet)
		var completed: bool = await _animate(packet, epoch)
		if epoch != _epoch:
			return
		if not completed:
			push_error("IA: animação do Host falhou; fase suspensa para validação")
			return
		while not _waiting.is_empty() and epoch == _epoch:
			await get_tree().process_frame
		if epoch != _epoch:
			return
		_states[id] = State.Idle
	_running = false
	phase_finished.emit(version)

func _can_attack(origin: Vector2i, target: Vector2i, monster: Node) -> bool:
	var delta: Vector2i = target - origin
	return absi(delta.x) + absi(delta.y) <= int(monster.get_meta("range", 1)) and combat.has_line_of_sight(origin, target)

func _decide(id: int) -> Dictionary:
	var monster: Node = _monsters[id]
	var origin: Vector2i = arena.authority.get_unit_cell(_unit_id(id))
	var target_id: int = 0
	var distance: int = 2147483647
	for player_id in [1, 2]:
		if not combat.stats[player_id].is_alive():
			continue
		var cell: Vector2i = arena.authority.get_unit_cell(PLAYER_IDS[player_id])
		var delta: Vector2i = cell - origin
		var candidate: int = absi(delta.x) + absi(delta.y)
		if candidate < distance:
			distance = candidate
			target_id = player_id
	var path: Array[Vector2i] = [origin]
	var packet: Dictionary = {"m_id": id, "path": path, "atk": 0, "dmg": 0}
	_states[id] = State.Idle
	if target_id == 0:
		return packet
	var target: Vector2i = arena.authority.get_unit_cell(PLAYER_IDS[target_id])
	if not _can_attack(origin, target, monster):
		_states[id] = State.Chase
		var occupied: Array[Vector2i] = []
		for unit in arena.authority._positions:
			if unit != _unit_id(id):
				occupied.append(arena.authority.get_unit_cell(unit))
		var best: Array[Vector2i] = []
		for direction in DIRECTIONS:
			var route: Array[Vector2i] = _navigation.find_path(arena.board.model, origin, target + direction, occupied)
			if route.size() > 1 and (best.is_empty() or route.size() < best.size()):
				best = route
		var budget: int = maxi(0, int(monster.get_meta("pm", 3)))
		if not best.is_empty():
			path.assign(best.slice(0, mini(best.size(), budget + 1)))
	var destination: Vector2i = path.back()
	# Reserva lógica antes da apresentação; os clientes nunca decidem ocupação.
	arena.authority._positions[_unit_id(id)] = destination
	packet["path"] = path
	if _can_attack(destination, target, monster):
		_states[id] = State.Attack
		var damage: int = combat.stats[target_id].server_damage(maxi(1, int(monster.get_meta("damage", combat.attack_damage))))
		combat.revision += 1
		packet.merge({"atk": target_id, "dmg": damage, "hp": combat.stats[target_id].hp, "pa": combat.stats[target_id].pa, "r": combat.revision}, true)
	return packet

@rpc("authority", "call_remote", "reliable", 0)
func rpc_action(packet: Dictionary) -> void:
	if not network.from_server() or int(packet.get("s", -1)) <= _last_received:
		return
	_last_received = int(packet["s"])
	var target_id: int = int(packet["atk"])
	if target_id != 0 and int(packet["r"]) > combat.revision:
		# apply_snapshot exige o contexto RPC; aplicar antes de qualquer await.
		combat.stats[target_id].apply_snapshot({"hp": int(packet["hp"]), "pa": int(packet["pa"])})
		combat.revision = int(packet["r"])
	var epoch: int = _epoch
	var completed: bool = await _animate(packet, epoch)
	if epoch == _epoch and network.active:
		rpc_animation_done.rpc_id(1, int(packet["s"]), completed)

func _animate(packet: Dictionary, epoch: int) -> bool:
	var monster: Node = _monsters[int(packet["m_id"])]
	var path: Array[Vector2i] = []
	path.assign(packet["path"])
	if path.size() > 1:
		if not monster.execute_path(path):
			push_error("IA: caminho incompatível com a posição do nó %s" % monster.name)
			return false
		while monster.is_moving and epoch == _epoch:
			await get_tree().process_frame
		if epoch != _epoch or monster.current_cell != path.back():
			return false
	if int(packet["atk"]) != 0:
		var visual: Node3D = monster.get_node("Visual")
		_effect = create_tween()
		_effect.tween_property(visual, "scale", Vector3(1.15, 0.85, 1.15), 0.12)
		_effect.tween_property(visual, "scale", Vector3.ONE, 0.18)
		while _effect.is_valid() and _effect.is_running() and epoch == _epoch:
			await get_tree().process_frame
	return epoch == _epoch

@rpc("any_peer", "call_remote", "reliable", 0)
func rpc_animation_done(sequence: int, completed: bool) -> void:
	if not multiplayer.is_server() or not _running or sequence != _sequence:
		return
	var sender: int = multiplayer.get_remote_sender_id()
	if not _waiting.has(sender):
		return
	if not completed:
		push_error("IA: cliente %d não concluiu a animação %d; fase suspensa" % [sender, sequence])
		return
	_waiting.erase(sender)
