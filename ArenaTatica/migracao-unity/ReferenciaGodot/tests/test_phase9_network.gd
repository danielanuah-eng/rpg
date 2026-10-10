extends SceneTree

const PORT := 19099
var arena: Node
var network: Node
var combat: Node
var game: Node
var fx: Node
var received: Array[StringName] = []
var failures := 0
var checks := 0

func _initialize() -> void:
	_run.call_deferred()

func check(ok: bool, label: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		push_error(label)

func until(condition: Callable, label: String) -> bool:
	var deadline := Time.get_ticks_msec() + 10000
	while not condition.call() and Time.get_ticks_msec() < deadline:
		await process_frame
	var ok := bool(condition.call())
	check(ok, label)
	return ok

func _run() -> void:
	arena = load("res://scenes/arena_interativa.tscn").instantiate()
	root.add_child(arena)
	network = root.get_node("NetworkManager")
	network.status_changed.connect(func(message: String): print("NET ", message))
	network.multiplayer.peer_connected.connect(func(id: int): print("PEER ", id))
	combat = arena.get_node("CombatManager")
	game = arena.get_node("GameLoopManager")
	fx = arena.get_node("VisualFXManager")
	fx.effect_received.connect(func(kind: StringName): received.append(kind))
	await process_frame
	await process_frame
	check(arena.get_node("AudioManager")._streams.size() == 4, "Quatro WAV importados")
	if OS.get_cmdline_user_args().has("--host"):
		await _host()
	else:
		await _client()
	print("FASE9 RPC %s: %d verificacoes, %d falhas" % [OS.get_cmdline_user_args(), checks, failures])
	arena.free()
	quit(0 if failures == 0 else 1)

func _host() -> void:
	check(network.host(PORT) == OK, "Host inicia")
	var ai: Node = arena.get_node("AIManager")
	ai._running = true
	combat.stats[1].server_damage(17)
	combat.stats[1].server_spend_pa(1)
	arena.authority._positions[&"player_1"] = Vector2i(0, 1)
	arena.player.configure(arena.board, Vector2i(0, 1))
	if not await until(func(): return not network._pending_ready.is_empty(), "Entrada enfileirada"):
		return
	check(network.clients.is_empty(), "Snapshot aguarda ação atual")
	ai._running = false
	if not await until(func(): return network.clients.size() == 1, "Snapshot enviado ao ficar ocioso"):
		return
	await create_timer(0.3).timeout
	for kind in [&"attack", &"breath", &"turn"]:
		fx.server_effect(kind, Vector3(1, 1, 1), Vector3(2, 1, 1), 7)
	var turns: Node = root.get_node("TurnManager")
	turns._state = {"t": "P2", "pm": 3}
	check(arena.authority.request_move(&"player_2", arena.player_2.current_cell + Vector2i.LEFT, "P2"), "Movimento de P2 iniciado antes da desconexão")
	await until(func(): return network.clients.is_empty(), "Cliente desconecta")
	check(network.active and game.dead.has(2), "Host sobrevive e remove P2")
	check(not arena.authority._positions.has(&"player_2"), "Ocupação removida")
	check(combat.stats[2].hp == 0, "HP de desconectado permanece zero")
	check(not arena.authority.is_busy() and not turns._moving, "Desconexão libera movimento em andamento")
	if not await until(func(): return network.clients.size() == 1, "Reconexão aceita"):
		return
	await create_timer(1.0).timeout

func _client() -> void:
	await create_timer(0.2).timeout
	network.allow_insecure_development = false
	check(network.join("ws://127.0.0.1:%d" % PORT) == ERR_INVALID_PARAMETER, "Produção bloqueia WS")
	network.allow_insecure_development = true
	check(network.join("ws://127.0.0.1:%d" % PORT) == OK, "Cliente conecta")
	if not await until(func(): return network.synced and combat.synced, "Snapshot completo"):
		return
	check(combat.stats[1].hp == 83 and combat.stats[1].pa == 1, "HP/PA tardios exatos")
	check(arena.player.current_cell == Vector2i(0, 1), "Posição tardia exata")
	received.clear()
	fx.server_effect(&"death", Vector3.ZERO, Vector3.ZERO)
	fx.rpc_effect(999, &"death", Vector3.ZERO, Vector3.ZERO, 1)
	check(received.is_empty(), "Cliente não forja efeito autoritativo")
	if not await until(func(): return received.size() == 3, "Três efeitos RPC recebidos"):
		return
	check(received == [&"attack", &"breath", &"turn"], "Sem duplicação; ordem dos VFX/SFX")
	network.close()
	await create_timer(0.5).timeout
	check(network.join("ws://127.0.0.1:%d" % PORT) == OK, "Reconecta")
	await until(func(): return network.synced and combat.synced and game.dead.has(2), "Snapshot após desconexão")
	check(combat.stats[2].hp == 0 and not arena.has_node("Player2"), "Não ressuscita desconectado")
	check(combat.stats[1].hp > 0, "Reconexão preserva sobrevivente")
