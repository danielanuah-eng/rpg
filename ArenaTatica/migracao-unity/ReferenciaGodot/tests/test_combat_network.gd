extends SceneTree

const PORT: int = 19086
var arena: Node
var combat: Node
var turns: Node
var network: Node
var checks: int = 0
var failures: int = 0
var events: Array[Dictionary] = []


func _initialize() -> void:
	_run.call_deferred()


func check(ok: bool, message: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		push_error(message)


func until(condition: Callable, message: String) -> bool:
	var deadline: int = Time.get_ticks_msec() + 15000
	while not condition.call() and Time.get_ticks_msec() < deadline:
		await process_frame
	var ok: bool = bool(condition.call())
	check(ok, message)
	return ok


func _run() -> void:
	network = root.get_node("NetworkManager")
	turns = root.get_node("TurnManager")
	arena = load("res://scenes/arena_interativa.tscn").instantiate()
	root.add_child(arena)
	combat = arena.get_node("CombatManager")
	combat.attack_resolved.connect(func(event: Dictionary): events.append(event))
	arena.player_2.step_duration = 0.02
	if OS.get_cmdline_user_args().has("--host"):
		await _host()
	else:
		await create_timer(0.7).timeout
		await _client()
	print("FASE 6 REDE %s: %d verificações, %d falhas." % [OS.get_cmdline_user_args(), checks, failures])
	arena.free()
	quit(0 if failures == 0 else 1)


func _host() -> void:
	check(network.host(PORT) == OK, "Host inicia")
	if not await until(func(): return network.clients.size() == 1, "Cliente conecta"):
		return
	await create_timer(0.4).timeout
	check(combat.stats[1].hp == 100 and combat.stats[2].hp == 100, "Ataques de peça alheia ou fora do turno são rejeitados")
	arena.board.model.cells[Vector2i(5, 5)]["blocks_los"] = true
	combat.request_attack(1, 2)
	check(combat.stats[1].pa == 2 and combat.stats[2].hp == 100, "LoS também bloqueia o Host")
	turns.pass_turn()
	if not await until(func(): return arena.player_2.current_cell == Vector2i(10, 9) and not arena.authority.is_busy(), "P2 conclui teste de ataque bloqueado e movimento"):
		return
	check(combat.stats[1].hp == 100 and combat.stats[2].pa == 2, "Host valida seu próprio mapa de obstáculos")
	arena.board.model.cells[Vector2i(5, 5)]["blocks_los"] = false
	if not await until(func(): return turns.get_state()["t"] == "IA_NEUTRA", "P2 conclui ataques e passa turno"):
		return
	check(combat.stats[1].hp == 70 and combat.stats[2].pa == 0, "Host confirma somente dois ataques de P2")
	check(events.size() == 2 and events[0]["atk"] == 2 and events[0]["dmg"] == 15, "Host emite resultados compactos")
	turns.pass_turn()
	check(combat.stats[1].pa == 2 and combat.stats[2].pa == 0 and combat.stats[1].hp == 70, "PA restaurados só no início do turno ativo")
	combat.request_attack(1, 2)
	if not await until(func(): return not network.active, "Cliente recebe ataque do Host e desconecta"):
		return
	check(combat.stats[1].hp == 100 and combat.stats[2].hp == 100, "Fim da sessão reinicia atributos")
	check(network.host(PORT) == OK, "Nova sessão inicia")
	combat.request_attack(1, 2)
	check(combat.stats[2].hp == 85 and combat.stats[1].pa == 1, "Host pode atacar antes da entrada de P2")
	await until(func(): return network.clients.size() == 1, "Cliente entra na sessão já alterada")
	await create_timer(0.8).timeout


func _client() -> void:
	check(network.join("ws://127.0.0.1:%d" % PORT) == OK, "Cliente inicia conexão")
	if not await until(func(): return network.synced and combat.synced, "Snapshot inicial de atributos recebido"):
		return
	check(combat.stats[1].pa == 2 and combat.stats[2].pa == 0, "Cliente recebe PA apenas de P1 no primeiro turno")
	combat.rpc_request_attack.rpc_id(1, 1, 2, turns.revision, combat.revision)
	combat.rpc_request_attack.rpc_id(1, 2, 1, turns.revision, combat.revision)
	check(not combat.stats[2].server_spend_pa(1), "Cliente não altera PA localmente")
	check(combat.stats[1].server_damage(50) == 0, "Cliente não calcula dano autoritativo")
	combat.stats[1].apply_snapshot({"hp": 1, "pa": 0})
	check(combat.stats[1].hp == 100, "Snapshot chamado fora de RPC do Host é recusado")
	if not await until(func(): return turns.get_state()["t"] == "P2" and combat.stats[2].pa == 2, "Turno P2 sincroniza restauração de PA"):
		return
	check(combat.has_line_of_sight(Vector2i(22, 22), Vector2i(1, 1)), "Mapa local sem obstáculo não decide validade do ataque")
	combat.rpc_request_attack.rpc_id(1, 1, 2, turns.revision, combat.revision)
	combat.request_attack(2, 1)
	await until(func(): return arena.status.text.contains("visão bloqueada"), "Servidor rejeita ataque por LoS mesmo com mapa cliente livre")
	check(combat.stats[1].hp == 100 and combat.stats[2].pa == 2, "Rejeição não altera HP nem PA")
	arena.request_move(arena.UNIT_2, Vector2i(10, 9))
	await until(func(): return turns.get_state()["pm"] == 2 and arena.player_2.current_cell == Vector2i(10, 9), "Movimento existente continua funcionando")
	await create_timer(0.3).timeout
	check(combat.stats[2].pa == 2, "Movimento não gasta ou restaura PA")
	var turn_version: int = turns.revision
	var combat_version: int = combat.revision
	combat.request_attack(2, 1)
	await until(func(): return combat.stats[1].hp == 85 and combat.stats[2].pa == 1, "Dano e PA de P2 sincronizados")
	combat.rpc_request_attack.rpc_id(1, 2, 1, turn_version, combat_version)
	await create_timer(0.1).timeout
	check(combat.stats[1].hp == 85 and combat.stats[2].pa == 1, "Repetição de pedido antigo não duplica dano")
	combat.request_attack(2, 1)
	await until(func(): return combat.stats[1].hp == 70 and combat.stats[2].pa == 0, "Segundo ataque consome último PA")
	combat.request_attack(2, 1)
	await create_timer(0.1).timeout
	check(combat.stats[1].hp == 70 and combat.stats[2].pa == 0, "Ataque sem PA é rejeitado")
	check(events.size() == 2 and events[0].size() == 6, "Cliente recebe payload compacto de seis campos")
	turns.pass_turn()
	await until(func(): return turns.get_state()["t"] == "P1" and combat.stats[2].hp == 85, "Ataque do Host sincronizado no cliente")
	check(combat.stats[1].hp == 70 and combat.stats[1].pa == 1 and combat.stats[2].pa == 0, "Restauração não cura nem restaura PA do inativo")
	network.close()
	await create_timer(0.3).timeout
	check(network.join("ws://127.0.0.1:%d" % PORT) == OK, "Cliente reconecta")
	await until(func(): return network.synced and combat.synced, "Snapshot tardio recebido")
	check(combat.stats[2].hp == 85 and combat.stats[1].pa == 1 and combat.stats[2].pa == 0, "Snapshot tardio preserva dano e PA atuais")
