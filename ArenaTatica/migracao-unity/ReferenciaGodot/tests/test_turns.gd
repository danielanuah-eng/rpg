extends SceneTree

const Model = preload("res://scripts/grid_model.gd")
const Authority = preload("res://scripts/move_authority.gd")
const Scene = preload("res://scenes/arena_interativa.tscn")

var failures: int = 0
var checks: int = 0

class NeutralStub extends Node:
	signal phase_finished(version: int)
	func start_neutral_turn(_version: int) -> void:
		pass


func _initialize() -> void:
	_run.call_deferred()


func check(ok: bool, message: String) -> void:
	checks += 1
	if not ok:
		failures += 1
		push_error(message)


func _run() -> void:
	var turns = root.get_node("TurnManager")
	var neutral := NeutralStub.new()
	root.add_child(neutral)
	turns.register_neutral_ai(neutral)
	var model := Model.new()
	model.configure(6, 6)
	var authority := Authority.new()
	root.add_child(authority)
	authority.turns = turns
	authority.configure(model)
	authority.register_unit(&"u", Vector2i.ZERO)
	check(not authority.request_move(&"u", Vector2i(4, 0)), "Rejeita quatro casas")
	check(not authority.request_move(&"u", Vector2i(1, 0), "P2"), "Rejeita jogador fora do turno")
	model.cells[Vector2i(1, 0)]["walkable"] = false
	check(not authority.request_move(&"u", Vector2i(2, 0)), "Usa custo AStar com desvio, não Manhattan")
	model.cells[Vector2i(1, 0)]["walkable"] = true
	check(authority.request_move(&"u", Vector2i(3, 0)), "Aprova três casas")
	check(not authority.request_move(&"u", Vector2i(1, 1)), "Bloqueia pedido durante movimento")
	for x in range(1, 4):
		check(authority.confirm_step(&"u", Vector2i(x, 0)), "Confirma passo")
		check(turns.get_state()["pm"] == 3 - x, "Desconta um PM por casa")
	authority.finish_move(&"u")
	check(turns.get_state() == {"t": "P2", "pm": 3}, "Zero PM avança uma vez")
	authority.finish_move(&"u")
	check(turns.get_state()["t"] == "P2", "Finalização duplicada não avança")
	turns.pass_turn()
	check(turns.get_state()["t"] == "IA_NEUTRA", "Inclui IA neutra")
	turns.pass_turn()
	check(turns.get_state()["t"] == "IA_NEUTRA", "Jogador não pula IA")
	neutral.phase_finished.emit(turns.revision)
	check(turns.get_state()["t"] == "P1", "Fecha ciclo")
	check(authority.request_move(&"u", Vector2i(5, 0)), "Aprova movimento parcial")
	authority.confirm_step(&"u", Vector2i(4, 0))
	authority.finish_move(&"u")
	check(turns.get_state() == {"t": "P1", "pm": 2}, "Interrupção cobra só casas percorridas")
	check(authority.request_move(&"u", Vector2i(5, 0)), "Retoma com PM restantes")
	turns.pass_turn()
	turns.pass_turn()
	check(turns.get_state()["t"] == "P1", "Passagem aguarda movimento")
	authority.confirm_step(&"u", Vector2i(5, 0))
	authority.finish_move(&"u")
	check(turns.get_state() == {"t": "P2", "pm": 3}, "Passagem pendente ocorre uma vez")
	var snapshot: Dictionary = turns.get_state()
	snapshot["pm"] = 999
	check(turns.get_state()["pm"] == 3, "Snapshot não altera estado interno")
	authority.free()
	var session = Scene.instantiate()
	root.add_child(session)
	await process_frame
	await process_frame
	session.player.step_duration = 0.01
	check(session.authority.request_move(session.UNIT_ID, Vector2i(3, 2)), "Integra caminho ao Tween")
	for frame in range(120):
		await physics_frame
		if frame > 2 and not session.player.is_moving:
			break
	check(session.player.current_cell == Vector2i(3, 2), "Bloco percorre três casas")
	check(turns.get_state() == {"t": "P2", "pm": 3}, "Tween encerra turno")
	session.get_node("UI/TurnPanel/PassTurn").pressed.emit()
	check(turns.get_state()["t"] == "IA_NEUTRA", "Botão passa turno")
	session.free()
	neutral.free()
	print("FASE 3: %d verificações, %d falhas." % [checks, failures])
	quit(0 if failures == 0 else 1)
