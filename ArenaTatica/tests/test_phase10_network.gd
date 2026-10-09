extends SceneTree
const PORT := 19110
var arena: Node
var network: Node
var combat: Node
var turns: Node
var checks := 0
var failures := 0
var joins := 0

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
	network.client_ready.connect(func(_id: int): joins += 1)
	combat = arena.get_node("CombatManager")
	turns = root.get_node("TurnManager")
	await process_frame
	await process_frame
	if OS.get_cmdline_user_args().has("--host"):
		await _host()
	else:
		await _client()
	print("FASE10 RPC %s: %d verificacoes, %d falhas" % [OS.get_cmdline_user_args(), checks, failures])
	arena.free()
	quit(0 if failures == 0 else 1)

func _host() -> void:
	check(network.host(PORT) == OK, "Host inicia")
	if not await until(func(): return network.clients.size() == 1, "Cliente pronto"):
		return
	await until(func(): return network.debug_metrics()["rtt_ms"] >= 0, "RTT medido no Host")
	turns._pass()
	await until(func(): return combat.stats[1].hp == 70, "Apenas dois ataques admitidos")
	await until(func(): return network.suppressed_inputs >= 49, "Host limita RPCs diretos em rajada")
	check(combat.stats[2].pa == 0, "Debounce preserva custo dos ataques admitidos")
	await create_timer(0.4).timeout
	network.close()
	check(turns.get_state()["t"] == "P1" and not turns.match_over, "Host volta ao estado inicial")
	await create_timer(0.6).timeout
	check(network.host(PORT) == OK, "Host reabre")
	await until(func(): return joins == 2, "Reconecta após queda")
	await create_timer(0.5).timeout
	await until(func(): return network.clients.is_empty(), "Cliente encerra sem crash")

func _client() -> void:
	await create_timer(0.2).timeout
	check(network.join("ws://127.0.0.1:%d" % PORT) == OK, "Join")
	if not await until(func(): return network.synced and combat.synced, "Snapshot"):
		return
	await until(func(): return network.debug_metrics()["rtt_ms"] >= 0, "RTT medido no Client")
	await until(func(): return turns.get_state()["t"] == "P2", "Turno P2")
	for index in range(100):
		combat.request_attack(2, 1)
	check(network.suppressed_inputs == 99, "100 cliques enviam só um ataque")
	await until(func(): return combat.stats[1].hp == 85, "Primeiro ataque aplicado")
	await create_timer(0.15).timeout
	for index in range(50):
		combat.rpc_request_attack.rpc_id(1, 2, 1, turns.revision, combat.revision)
	await until(func(): return combat.stats[1].hp == 70, "Host filtra rajada direta")
	if not await until(func(): return network.in_menu, "Queda do Host abre menu"):
		return
	check(not network.active and not network.synced, "Flags desligadas")
	check(network.multiplayer.multiplayer_peer is OfflineMultiplayerPeer, "Peer remoto substituído")
	check(turns.get_state() == {"t": "P1", "pm": 3} and not turns.match_over, "Turnos resetados")
	check(combat.stats[1].hp == 100 and combat.stats[2].hp == 100, "HP resetado")
	check(network._probes.is_empty() and network._rtt.is_empty(), "Sondas descartadas")
	check(arena.get_node("DebugInfoOverlay").menu.visible, "Menu visível")
	check(not network.allow_input(&"move"), "Menu bloqueia comandos antigos")
	turns.pass_turn()
	check(turns.get_state()["t"] == "P1", "Menu bloqueia passar turno")
	await create_timer(0.9).timeout
	check(network.join("ws://127.0.0.1:%d" % PORT) == OK, "Reconexão")
	await until(func(): return network.synced and not network.in_menu, "Nova sessão funciona")
	network.close()
	network.close()
	check(not network.active and turns.get_state()["t"] == "P1", "Fechar duas vezes é seguro")
	check(network.join("ws://127.0.0.1:19998") == OK, "Tentativa em porta indisponível")
	await until(func(): return network.in_menu, "connection_failed tratado")
	network.resume_local()
	check(not network.in_menu, "Retoma modo local")
