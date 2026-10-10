extends SceneTree

var session: Node
var network: Node
var turns: Node
var failures: int = 0
var checks: int = 0
var animations: Dictionary = {"player_1": 0, "player_2": 0}
const PORT: int = 19084


func _initialize() -> void:
	_run.call_deferred()


func check(ok: bool, message: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		push_error(message)
		if session != null:
			print("DEBUG: ", turns.get_state(), " r=", turns.revision, " cell=", session.player.current_cell, " status=", session.status.text)


func until(condition: Callable, message: String) -> bool:
	var deadline: int = Time.get_ticks_msec() + 20000
	while not condition.call() and Time.get_ticks_msec() < deadline:
		await process_frame
	var success: bool = bool(condition.call())
	check(success, message)
	return success


func _run() -> void:
	network = root.get_node("NetworkManager")
	network.status_changed.connect(func(message: String): print("REDE: ", message))
	turns = root.get_node("TurnManager")
	session = load("res://scenes/arena_interativa.tscn").instantiate()
	root.add_child(session)
	session.player.step_duration = 0.08
	session.player_2.step_duration = 0.08
	session.player.step_started.connect(func(_from: Vector2i, _to: Vector2i): animations["player_1"] += 1)
	session.player_2.step_started.connect(func(_from: Vector2i, _to: Vector2i): animations["player_2"] += 1)
	if OS.get_cmdline_user_args().has("--host"):
		await _host()
	else:
		await create_timer(0.7).timeout
		await _client()
	print("FASE 5 %s: %d verificações, %d falhas." % [OS.get_cmdline_user_args(), checks, failures])
	session.free()
	quit(0 if failures == 0 else 1)


func _host() -> void:
	check(network.host(PORT) == OK, "Host WebSocket inicia")
	if not await until(func(): return network.clients.size() == 1, "Cliente conecta"):
		return
	await create_timer(0.5).timeout
	check(turns.get_state() == {"t": "P1", "pm": 3}, "Cliente não falsifica estado nem passa turno P1")
	check(session.player.current_cell == Vector2i(1, 1), "Cliente fora do turno não move")
	check(session.player_2.current_cell == Vector2i(22, 22), "Peça 2 permanece na própria casa inicial")
	session._on_move_intent(session.player_2.current_cell)
	check(session.selected_unit == session.UNIT_ID and session.status.text.contains("outro jogador"), "Clique do Host na peça 2 é rejeitado")
	session.rpc_request_move(str(session.UNIT_2), {"x": 10, "z": 9}, turns.revision)
	check(not session.player_2.is_moving and turns.get_state()["pm"] == 3, "Host não pode solicitar movimento da peça 2")
	session.board.model.cells[Vector2i(2, 1)]["walkable"] = false
	session._on_move_intent(Vector2i(3, 1))
	check(not session.player.is_moving and turns.get_state()["pm"] == 3, "Host valida custo AStar com desvio")
	session.board.model.cells[Vector2i(2, 1)]["walkable"] = true
	session.player._board = null
	session._on_move_intent(Vector2i(2, 1))
	check(not session.authority.is_busy() and not session.player.is_moving and turns.get_state()["pm"] == 3, "Falha ao iniciar animação libera rota sem consumir PM")
	session.player.configure(session.board, Vector2i(1, 1))
	session._on_move_intent(Vector2i(4, 1))
	await until(func(): return turns.get_state()["t"] == "P2", "Três casas passam turno para P2")
	session._on_move_intent(Vector2i(5, 1))
	turns.pass_turn()
	check(not session.player.is_moving and turns.get_state()["t"] == "P2", "Host não controla P2")
	await until(func(): return turns.get_state()["t"] == "IA_NEUTRA", "Cliente conclui movimento e passa turno")
	check(session.player_2.current_cell == Vector2i(22, 20), "Host aplica duas casas solicitadas pelo cliente")
	check(session.player.current_cell == Vector2i(4, 1), "Movimento de P2 preserva a peça 1")
	check(animations["player_1"] == 3 and animations["player_2"] == 2, "Host anima as duas peças por casa")
	check(turns.get_state()["pm"] == 3, "IA recebe três PM")
	session.request_move(session.UNIT_ID, Vector2i(5, 1))
	check(not session.player.is_moving and turns.get_state()["t"] == "IA_NEUTRA", "Fase neutra não permite mover peça humana")
	turns.pass_turn()
	session._on_move_intent(Vector2i(2, 1))
	await until(func(): return not network.active, "Desconexão retorna ao modo local")
	check(turns.get_state() == {"t": "P1", "pm": 3}, "Desconexão reinicia turnos")
	check(session.player.current_cell == Vector2i(1, 1), "Desconexão reinicia posição")
	check(session.player_2.current_cell == Vector2i(22, 22), "Desconexão reinicia também a peça 2")
	check(network.host(PORT) == OK, "Host pode reabrir sessão")
	session.player.step_duration = 0.7
	session._on_move_intent(Vector2i(4, 1))
	await until(func(): return network.clients.size() == 1, "Reconexão durante movimento")
	check(session.player.is_moving, "Cliente tardio entra durante percurso")
	await until(func(): return turns.get_state()["t"] == "IA_NEUTRA", "Cliente tardio recebe snapshot e passa P2")
	check(session.player.current_cell == Vector2i(4, 1), "Snapshot preserva destino autoritativo")
	check(session.player_2.current_cell == Vector2i(22, 22), "Snapshot tardio preserva peça 2")
	await create_timer(0.2).timeout


func _client() -> void:
	check(network.join("ws://127.0.0.1:%d" % PORT) == OK, "Cliente inicia WebSocket")
	if not await until(func(): return network.synced, "Cliente recebe snapshot inicial"):
		return
	check(turns.get_state() == {"t": "P1", "pm": 3}, "Snapshot inicial contém turno e PM")
	check(session.player.current_cell == Vector2i(1, 1) and session.player_2.current_cell == Vector2i(22, 22), "Snapshot inicial contém ambas as peças")
	session._on_move_intent(session.player.current_cell)
	check(session.selected_unit == session.UNIT_2 and session.status.text.contains("outro jogador"), "Clique do cliente na peça 1 é rejeitado")
	session.rpc_request_move.rpc_id(1, str(session.UNIT_ID), {"x": 2, "z": 1}, turns.revision)
	session.rpc_request_move.rpc_id(1, str(session.UNIT_2), {"x": 2, "z": 1}, turns.revision)
	turns.rpc_request_pass.rpc_id(1, turns.revision)
	turns.rpc_sync_state.rpc_id(1, {"t": "P2", "pm": 3}, 10000)
	await until(func(): return turns.get_state()["t"] == "P2" and session.player.current_cell == Vector2i(4, 1), "Recebe movimento de P1 e turno P2")
	session.rpc_request_move.rpc_id(1, str(session.UNIT_ID), {"x": 5, "z": 1}, turns.revision)
	session.rpc_request_move.rpc_id(1, str(session.UNIT_2), {"x": 10, "z": 6}, turns.revision)
	session.rpc_request_move.rpc_id(1, str(session.UNIT_2), {"x": -1, "z": 1}, turns.revision)
	session.rpc_request_move.rpc_id(1, str(session.UNIT_2), {"x": 4.5, "z": 1}, turns.revision)
	session.rpc_request_move.rpc_id(1, str(session.UNIT_2), {"x": 10, "z": 9}, 0)
	await create_timer(0.25).timeout
	check(turns.get_state() == {"t": "P2", "pm": 3}, "Rejeita distância excessiva, coordenadas inválidas e revisão antiga")
	check(session.player.current_cell == Vector2i(4, 1), "Pedidos rejeitados não alteram posição")
	check(session.player_2.current_cell == Vector2i(22, 22), "Pedido de peça alheia no próprio turno não move nenhuma peça")
	session._on_move_intent(Vector2i(22, 20))
	await until(func(): return turns.get_state()["pm"] == 1 and session.player_2.current_cell == Vector2i(22, 20), "Cliente recebe movimento de duas casas e PM 1")
	await create_timer(0.15).timeout
	check(session.player.current_cell == Vector2i(4, 1), "Cliente mantém peça 1 enquanto movimenta peça 2")
	check(animations["player_1"] > 0 and animations["player_2"] > 0, "Cliente anima ambas as peças via RPC")
	session._on_move_intent(Vector2i(22, 20))
	turns.rpc_request_pass.rpc_id(1, 0)
	await create_timer(0.15).timeout
	check(turns.get_state() == {"t": "P2", "pm": 1}, "Destino atual e pedido antigo não consomem turno")
	turns.pass_turn()
	await until(func(): return turns.get_state()["t"] == "P1" and turns.get_state()["pm"] == 1 and session.player.current_cell == Vector2i(2, 1), "Ciclo e posição sincronizados")
	network.close()
	check(turns.get_state() == {"t": "P1", "pm": 3}, "Cliente desconectado volta ao modo local")
	await create_timer(0.3).timeout
	check(network.join("ws://127.0.0.1:%d" % PORT) == OK, "Cliente reconecta")
	await until(func(): return multiplayer_connected(), "Handshake da reconexão")
	check(not network.synced, "Cliente tardio aguarda fim do movimento")
	await until(func(): return network.synced, "Snapshot após percurso em andamento")
	check(session.player.current_cell == Vector2i(4, 1), "Snapshot tardio contém posição final")
	check(session.player_2.current_cell == Vector2i(22, 22), "Snapshot tardio contém a posição da peça 2")
	check(turns.get_state() == {"t": "P2", "pm": 3}, "Snapshot tardio contém PM e turno corretos")
	turns.pass_turn()
	await until(func(): return turns.get_state()["t"] == "IA_NEUTRA", "Cliente reconectado controla P2")


func multiplayer_connected() -> bool:
	return network.active and network.multiplayer.multiplayer_peer.get_connection_status() == MultiplayerPeer.CONNECTION_CONNECTED
