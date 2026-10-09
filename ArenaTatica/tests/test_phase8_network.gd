extends SceneTree

const PORT := 19088
var arena: Node
var game: Node
var combat: Node
var network: Node
var hud: Node
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
	var deadline := Time.get_ticks_msec() + 12000
	while not condition.call() and Time.get_ticks_msec() < deadline:
		await process_frame
	var ok := bool(condition.call())
	check(ok, label)
	return ok


func _run() -> void:
	arena = load("res://scenes/arena_interativa.tscn").instantiate()
	root.add_child(arena)
	game = arena.get_node("GameLoopManager")
	combat = arena.get_node("CombatManager")
	hud = arena.get_node("HUD")
	network = root.get_node("NetworkManager")
	await process_frame
	await process_frame
	if OS.get_cmdline_user_args().has("--host"):
		await _host()
	else:
		await _client()
	print("FASE8 RPC %s: %d verificacoes, %d falhas" % [OS.get_cmdline_user_args(), checks, failures])
	arena.free()
	quit(0 if failures == 0 else 1)


func _host() -> void:
	check(network.host(PORT) == OK, "Host inicia")
	if not await until(func(): return network.clients.size() == 1, "Cliente conecta"):
		return
	await create_timer(0.5).timeout
	check(not game.match_over and game.dead.is_empty(), "Cliente não forja morte/resultado")
	var turns: Node = root.get_node("TurnManager")
	turns._pass()
	turns._pass()
	await until(func(): return turns.get_state()["t"] == "P1", "IA conclui com ACK do cliente")
	check(combat.stats[1].hp == 80, "Dano PvEvP preservado no Host")
	await create_timer(0.2).timeout
	# Dano letal usa o mesmo caminho validado do combate normal.
	combat.attack_damage = 100
	combat.request_attack(1, 2)
	await create_timer(0.4).timeout
	check(game.dead.has(2) and not game.match_over, "Derrota individual antes do fim global")
	combat.request_attack(1, 1001)
	await create_timer(0.3).timeout
	combat.stats[1].server_restore_pa()
	combat._publish_stats("P1")
	combat.request_attack(1, 1002)
	await until(func(): return game.match_over, "Último monstro encerra partida")
	check(hud.result_label.text == "Vitória", "Host recebe vitória local")
	await until(func(): return network.clients.is_empty(), "Cliente confirmou derrota e saiu")
	check(network.active and not arena.has_node("Player2") and game.match_over, "Desconexão preserva Host e estado")
	check(network.host(PORT) == OK, "Segunda sessão inicia")
	# Entrada tardia após mortes: reproduzir remoção e resultado ao conectar.
	combat.stats[2].server_damage(100)
	combat.stats[1001].server_damage(30)
	combat.stats[1002].server_damage(30)
	await until(func(): return game.match_over, "Resultado antes da entrada tardia")
	await until(func(): return network.clients.size() == 1, "Cliente tardio conecta")
	await create_timer(1.0).timeout


func _client() -> void:
	await create_timer(0.3).timeout
	check(network.join("ws://127.0.0.1:%d" % PORT) == OK, "Join inicia")
	if not await until(func(): return network.synced and combat.synced, "Snapshots iniciais"):
		return
	check(hud.local_id() == 2 and hud.hp_bar.value == 100, "HUD mostra jogador cliente")
	game.rpc_remove(1)
	game.rpc_result(PackedInt32Array([1]), 2, true)
	check(not game.dead.has(1) and not game.match_over, "Chamadas locais não autoritativas recusadas")
	var turns: Node = root.get_node("TurnManager")
	await until(func(): return combat.stats[1].hp == 80 and turns.get_state()["t"] == "P1", "IA e retorno do turno sincronizados")
	check(arena.authority.get_unit_cell(&"monster_2") == Vector2i(2, 1), "Cliente atualiza célula atacável do monstro")
	if not await until(func(): return game.dead.has(2), "Morte recebida via RPC"):
		return
	await until(func(): return hud.overlay.visible, "Resultado individual recebido")
	check(hud.overlay.visible and hud.result_label.text == "Derrota", "Cliente recebe derrota imediata")
	check(hud.hp_bar.value == 0, "HP letal chega antes da remoção")
	check(not arena.has_node("Player2"), "Nó de rede removido no cliente")
	check(not arena.authority._positions.has(&"player_2"), "Grade cliente libera jogador")
	await until(func(): return game.match_over, "Fim global recebido")
	check(game.winner_id == 1 and game.dead.size() == 3, "Peers concordam sobre resultado e mortes")
	await create_timer(0.5).timeout
	network.close()
	await create_timer(0.5).timeout
	check(network.join("ws://127.0.0.1:%d" % PORT) == OK, "Reconexão tardia")
	await until(func(): return game.match_over and network.synced and combat.synced, "Snapshot tardio completo")
	check(game.dead.size() == 3 and hud.result_label.text == "Derrota", "Entrada tardia reproduz resultado")
	check(not arena.has_node("Monsters/Monster2") and not arena.has_node("Player2"), "Entrada tardia não ressuscita entidades")
