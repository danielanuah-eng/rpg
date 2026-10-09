extends SceneTree

var checks: int = 0
var failures: int = 0


func _initialize() -> void:
	_run.call_deferred()


func check(ok: bool, message: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		push_error(message)


func _run() -> void:
	var model = load("res://scripts/grid_model.gd").new()
	model.configure(5, 5)
	var authority = load("res://scripts/move_authority.gd").new()
	root.add_child(authority)
	var turns = root.get_node("TurnManager")
	authority.turns = turns
	authority.configure(model)
	check(authority.register_unit(&"player_1", Vector2i(1, 1), "P1"), "Registra peça 1 com dono P1")
	check(authority.register_unit(&"player_2", Vector2i(2, 1), "P2"), "Registra peça 2 com dono P2")
	check(not authority.request_move(&"player_1", Vector2i(1, 2), "P2"), "P2 não pode usar peça 1 nem por chamada interna")
	check(not authority.request_move(&"player_2", Vector2i(2, 2), "P1"), "P1 não pode usar peça 2 nem por chamada interna")
	check(not authority.request_move(&"player_1", Vector2i(2, 1), "P1"), "Destino ocupado rejeitado")
	check(not authority.request_move(&"player_1", Vector2i(3, 1), "P1"), "Desvio AStar pela outra peça excede PM")
	check(turns.get_state() == {"t": "P1", "pm": 3}, "Rejeições preservam PM e turno")
	check(authority.request_move(&"player_1", Vector2i(1, 2), "P1"), "P1 movimenta sua peça")
	check(authority.confirm_step(&"player_1", Vector2i(1, 2)), "Confirma passo de P1")
	authority.finish_move(&"player_1")
	check(authority.get_unit_cell(&"player_2") == Vector2i(2, 1), "Movimento de P1 preserva posição de P2")
	turns.pass_turn()
	check(not authority.request_move(&"player_1", Vector2i(1, 3), "P1"), "Dono correto continua impedido fora de seu turno")
	check(authority.request_move(&"player_2", Vector2i(2, 2), "P2"), "P2 movimenta sua peça em seu turno")
	check(authority.confirm_step(&"player_2", Vector2i(2, 2)), "Confirma passo de P2")
	authority.finish_move(&"player_2")
	check(turns.get_state() == {"t": "P2", "pm": 2}, "P2 gasta PM próprios")
	check(authority.get_unit_cell(&"player_1") == Vector2i(1, 2), "Movimento de P2 preserva posição de P1")
	authority.free()
	print("FASE 5 POSSE: %d verificações, %d falhas." % [checks, failures])
	quit(0 if failures == 0 else 1)
