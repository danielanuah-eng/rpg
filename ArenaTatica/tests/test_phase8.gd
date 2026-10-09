extends SceneTree

var checks := 0
var failures := 0
var arena: Node
var combat: Node
var game: Node
var hud: Node
var turns: Node


func _initialize() -> void:
	_run.call_deferred()


func check(ok: bool, message: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		push_error(message)


func settle() -> void:
	await process_frame
	await process_frame


func _run() -> void:
	# Regras isoladas; debounce real coberto pelos testes da Fase 10.
	root.get_node("NetworkManager").input_debounce_ms = 0
	arena = load("res://scenes/arena_interativa.tscn").instantiate()
	root.add_child(arena)
	combat = arena.get_node("CombatManager")
	game = arena.get_node("GameLoopManager")
	hud = arena.get_node("HUD")
	turns = root.get_node("TurnManager")
	await settle()
	check(combat.stats.size() == 4, "Jogadores e monstros registrados")
	check(hud.hp_bar.value == 100 and hud.pa_bar.value == 2, "HUD inicial")
	combat.request_attack(1, 1001)
	check(combat.stats[1001].hp == 15 and hud.pa_bar.value == 1, "Ataque ao monstro e sinal PA")
	combat.request_attack(1, 1001)
	await settle()
	check(not arena.has_node("Monsters/Monster1"), "queue_free remove monstro")
	check(not arena.authority._positions.has(&"monster_1"), "Ocupação do monstro removida")
	var route = arena.authority._navigation.find_path(arena.board.model, Vector2i(1, 1), Vector2i(1, 2))
	check(route.size() == 2, "AStar alcança célula liberada")
	check(not game.match_over, "Monstro eliminado não encerra PvEvP")
	var p2: Node = arena.player_2
	combat.stats[2].server_damage(100)
	await settle()
	check(not is_instance_valid(p2) and not arena.units.has(&"player_2"), "Jogador removido sem referência pendente")
	check(not game.match_over, "Sobrevivente precisa derrotar monstro restante")
	turns.pass_turn()
	check(turns.get_state()["t"] == "IA_NEUTRA", "Turno pula jogador eliminado")
	await create_timer(1.5).timeout
	check(turns.get_state()["t"] == "P1", "IA retorna ao jogador vivo")
	check(hud.hp_bar.value == combat.stats[1].hp, "HUD reage ao dano da IA")
	combat.stats[1002].server_damage(30)
	await settle()
	check(game.match_over and game.winner_id == 1, "Último adversário dispara vitória")
	check(hud.overlay.visible and hud.result_label.text.contains("Vitória"), "Painel de vitória visível")
	var version: int = turns.revision
	turns.pass_turn()
	check(turns.revision == version, "Fim bloqueia passar turno")
	check(not arena.authority.request_move(&"player_1", Vector2i(2, 1), "P1"), "Fim bloqueia movimento")
	arena._reset_piece()
	await settle()
	check(arena.has_node("Player2") and arena.has_node("Monsters/Monster1"), "Nova partida recria nós removidos")
	check(combat.stats[2].hp == 100 and combat.stats[1001].hp == 30, "Reset restaura atributos")
	check(not game.match_over and not hud.overlay.visible, "Reset limpa resultado e HUD")
	# Duas mortes na mesma resolução não podem gerar vitória prematura.
	combat.stats[1].server_damage(100)
	combat.stats[2].server_damage(100)
	await settle()
	check(game.match_over and game.winner_id == 0, "Mortes simultâneas: nenhum vencedor")
	check(hud.result_label.text.contains("Derrota"), "Derrota coletiva apresentada")
	arena._reset_piece()
	await settle()
	# Morte durante coroutine da IA; o estado persiste até a animação terminar.
	combat.stats[1].server_damage(95)
	turns.pass_turn()
	turns.pass_turn()
	await create_timer(1.5).timeout
	check(game.dead.has(1), "Dano letal da IA remove jogador")
	check(turns.get_state()["t"] == "P2", "IA continua sem acessar nó removido")
	arena._reset_piece()
	await settle()
	# A nova peça deve recuperar os vínculos de movimento.
	arena.request_move(&"player_1", Vector2i(2, 1))
	await create_timer(0.6).timeout
	check(arena.player.current_cell == Vector2i(2, 1), "Peça recriada volta a movimentar")
	print("FASE8 LOCAL: %d verificacoes, %d falhas" % [checks, failures])
	arena.free()
	quit(0 if failures == 0 else 1)



