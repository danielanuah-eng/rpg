extends Node

signal attack_resolved(event: Dictionary)

var UNITS: Dictionary = {1: &"player_1", 2: &"player_2"}
const OWNERS: Dictionary = {1: "P1", 2: "P2"}

@export_range(1, 10000) var attack_damage: int = 15
@export_range(1, 100) var attack_cost: int = 1

@onready var arena: Node = get_parent()
@onready var turns: Node = get_node("/root/TurnManager")
@onready var network: Node = get_node("/root/NetworkManager")
@onready var stats: Dictionary = {1: get_node("../Player1/Stats"), 2: get_node("../Player2/Stats")}

var revision: int = 0
var synced: bool = false
var attack_mode: bool = false


func _ready() -> void:
	set_multiplayer_authority(1)
	for id in stats:
		turns.register_player_stats(OWNERS[id], stats[id])
	turns.match_reset.connect(_reset_stats)
	turns.turn_started.connect(_publish_stats)
	network.session_started.connect(_session_changed)
	network.session_ended.connect(_session_changed)
	_install_ui.call_deferred()


func _install_ui() -> void:
	var panel: Node = arena.get_node("UI/TurnPanel")
	var button := Button.new()
	button.name = "AttackMode"
	button.text = "Atacar: clique na peça adversária"
	button.toggle_mode = true
	button.toggled.connect(func(enabled: bool): attack_mode = enabled)
	panel.add_child(button)



func _session_changed() -> void:
	synced = false


func _reset_stats() -> void:
	if multiplayer.is_server():
		for value in stats.values():
			value.server_reset()


func _publish_stats(_actor: String) -> void:
	if not multiplayer.is_server():
		return
	revision += 1
	synced = true
	if network.active:
		for peer_id in network.clients:
			_send_stats(peer_id)


func _stats_snapshot() -> Dictionary:
	var packet: Dictionary = {}
	for id in stats:
		packet[id] = stats[id].snapshot()
	return packet


func _send_stats(peer_id: int) -> void:
	if multiplayer.is_server() and network.active:
		rpc_stats.rpc_id(peer_id, {"r": revision, "p": _stats_snapshot()})


@rpc("authority", "call_remote", "reliable")
func rpc_stats(packet: Dictionary) -> void:
	if not network.from_server() or typeof(packet.get("r")) != TYPE_INT or typeof(packet.get("p")) != TYPE_DICTIONARY:
		return
	var players: Dictionary = packet["p"]
	if players.size() != stats.size() or packet["r"] < 0 or (synced and packet["r"] < revision):
		return
	for id in stats:
		if not players.get(id) is Dictionary or not stats[id].valid_snapshot(players[id]):
			return
	for id in stats:
		stats[id].apply_snapshot(players[id])
	revision = packet["r"]
	synced = true


func handle_cell_click(cell: Vector2i) -> bool:
	if not attack_mode:
		return false
	var actor: String = arena._local_owner()
	var attacker: int = 1 if actor == "P1" else (2 if actor == "P2" else 0)
	for id in UNITS:
		if stats.has(id) and stats[id].is_alive() and arena.authority.get_unit_cell(UNITS[id]) == cell:
			request_attack(attacker, id)
			return true
	arena.status.text = "Clique numa peça adversária."
	return true


func request_attack(attacker: int, target: int) -> void:
	if not network.allow_input(&"attack"):
		return
	if network.active and (not network.synced or not synced):
		arena.status.text = "Aguarde a sincronização."
		return
	if multiplayer.is_server():
		_validate_attack(1, attacker, target, turns.revision, revision)
	else:
		rpc_request_attack.rpc_id(1, attacker, target, turns.revision, revision)


@rpc("any_peer", "call_local", "reliable")
func rpc_request_attack(attacker: int, target: int, turn_version: int, combat_version: int) -> void:
	if not network.active or not multiplayer.is_server():
		return
	var sender: int = multiplayer.get_remote_sender_id()
	if sender != 0 and not network.allow_input(&"attack", sender):
		return
	_validate_attack(1 if sender == 0 else sender, attacker, target, turn_version, combat_version)


func _validate_attack(sender: int, attacker: int, target: int, turn_version: int, combat_version: int) -> void:
	if not multiplayer.is_server():
		return
	if turns.match_over or not OWNERS.has(attacker) or not stats.has(target) or attacker == target:
		_reject(sender, "Atacante ou alvo inválido.")
		return
	if network.active and not network.can_control(sender, OWNERS[attacker]):
		_reject(sender, "O atacante pertence ao outro jogador.")
		return
	if str(turns.get_state()["t"]) != OWNERS[attacker] or arena.authority.is_busy():
		_reject(sender, "Aguarde seu turno e o fim do movimento.")
		return
	if turn_version != turns.revision or combat_version != revision:
		_reject(sender, "Pedido antigo; repita o ataque.")
		return
	if not stats[attacker].is_alive() or not stats[target].is_alive() or stats[attacker].pa < attack_cost:
		_reject(sender, "Peça derrotada ou PA insuficientes.")
		return
	var origin: Vector2i = arena.authority.get_unit_cell(UNITS[attacker])
	var destination: Vector2i = arena.authority.get_unit_cell(UNITS[target])
	if not has_line_of_sight(origin, destination):
		_reject(sender, "Linha de visão bloqueada.")
		return
	if not stats[attacker].server_spend_pa(attack_cost):
		return
	var damage: int = stats[target].server_damage(attack_damage)
	revision += 1
	var event := {"atk": attacker, "tgt": target, "dmg": damage, "hp": stats[target].hp, "pa": stats[attacker].pa, "r": revision}
	_show_result(event)
	if network.active:
		for peer_id in network.clients:
			rpc_damage.rpc_id(peer_id, event)


func has_line_of_sight(origin: Vector2i, destination: Vector2i) -> bool:
	if origin == destination or not arena.board.contains_cell(origin) or not arena.board.contains_cell(destination):
		return false
	var delta: Vector2i = destination - origin
	var nx: int = absi(delta.x)
	var ny: int = absi(delta.y)
	var sx: int = signi(delta.x)
	var sy: int = signi(delta.y)
	var cell: Vector2i = origin
	var ix: int = 0
	var iy: int = 0
	while ix < nx or iy < ny:
		var horizontal: int = (2 * ix + 1) * ny
		var vertical: int = (2 * iy + 1) * nx
		if horizontal == vertical:
			if _blocks(cell + Vector2i(sx, 0)) or _blocks(cell + Vector2i(0, sy)):
				return false
			cell += Vector2i(sx, sy)
			ix += 1
			iy += 1
		elif horizontal < vertical:
			cell.x += sx
			ix += 1
		else:
			cell.y += sy
			iy += 1
		if _blocks(cell):
			return false
	return true


func _blocks(cell: Vector2i) -> bool:
	return not arena.board.contains_cell(cell) or bool(arena.board.model.get_cell_data(cell).get("blocks_los", false))


@rpc("authority", "call_remote", "reliable")
func rpc_damage(event: Dictionary) -> void:
	if not network.from_server() or not synced:
		return
	for key in ["atk", "tgt", "dmg", "hp", "pa", "r"]:
		if typeof(event.get(key)) != TYPE_INT:
			return
	var attacker: int = event["atk"]
	var target: int = event["tgt"]
	if not stats.has(attacker) or not stats.has(target) or attacker == target or event["r"] <= revision or event["dmg"] < 0:
		return
	var attacker_state: Dictionary = stats[attacker].snapshot()
	var target_state: Dictionary = stats[target].snapshot()
	attacker_state["pa"] = event["pa"]
	target_state["hp"] = event["hp"]
	if not stats[attacker].valid_snapshot(attacker_state) or not stats[target].valid_snapshot(target_state):
		return
	stats[attacker].apply_snapshot(attacker_state)
	stats[target].apply_snapshot(target_state)
	revision = event["r"]
	_show_result(event)


func _show_result(event: Dictionary) -> void:
	arena.status.text = "Peça %d atacou alvo %d: %d de dano." % [event["atk"], event["tgt"], event["dmg"]]
	attack_resolved.emit(event.duplicate())


func _reject(peer_id: int, reason: String) -> void:
	if peer_id == 1:
		arena.status.text = reason
	elif network.active and network.clients.has(peer_id):
		rpc_rejected.rpc_id(peer_id, reason)


@rpc("authority", "call_remote", "reliable")
func rpc_rejected(reason: String) -> void:
	if network.from_server():
		arena.status.text = reason
