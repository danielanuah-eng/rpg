extends CanvasLayer
## HUD local: nenhum RPC, polling ou processamento por frame.

@onready var arena: Node = get_parent()
@onready var combat: Node = get_node("../CombatManager")
@onready var game: Node = get_node("../GameLoopManager")
@onready var turns: Node = get_node("/root/TurnManager")
@onready var network: Node = get_node("/root/NetworkManager")
@onready var owner_label: Label = $Root/HUDPanel/Margin/Stats/Owner
@onready var turn_label: Label = $Root/HUDPanel/Margin/Stats/Turn
@onready var hp_bar: ProgressBar = $Root/HUDPanel/Margin/Stats/HP
@onready var hp_label: Label = $Root/HUDPanel/Margin/Stats/HPText
@onready var pa_bar: ProgressBar = $Root/HUDPanel/Margin/Stats/PA
@onready var pa_label: Label = $Root/HUDPanel/Margin/Stats/PAText
@onready var overlay: ColorRect = $Root/ResultOverlay
@onready var result_label: Label = $Root/ResultOverlay/Center/Panel/Margin/Lines/Result


func bind_managers() -> void:
	for id in [1, 2]:
		combat.stats[id].hp_changed.connect(_attributes_changed)
		combat.stats[id].pa_changed.connect(_attributes_changed)
	turns.state_changed.connect(_turn_changed)
	network.session_started.connect(refresh)
	network.session_ended.connect(refresh)
	game.result_received.connect(_result)
	game.match_restarted.connect(_restart)
	get_viewport().size_changed.connect(_layout)
	$Root/ResultOverlay/Center/Panel/Margin/Lines/Exit.pressed.connect(network.close)
	_layout()
	refresh()


func _layout() -> void:
	var compact := get_viewport().get_visible_rect().size.x < 1000
	var panel: Control = $Root/HUDPanel
	panel.offset_top = 20.0 if compact else 120.0
	panel.offset_bottom = 96.0 if compact else 330.0
	owner_label.visible = not compact
	turn_label.visible = not compact
	$Root/HUDPanel/Margin/Stats.add_theme_constant_override("separation", 0 if compact else 6)
	hp_bar.custom_minimum_size.y = 8 if compact else 20
	pa_bar.custom_minimum_size.y = 8 if compact else 20


func local_id() -> int:
	if network.active:
		return 1 if multiplayer.is_server() else 2
	return 2 if turns.get_state()["t"] == "P2" else 1


func _attributes_changed(_value: int, _maximum: int) -> void:
	refresh()


func _turn_changed(_state: Dictionary) -> void:
	refresh()


func refresh() -> void:
	var id := local_id()
	var stats: Node = combat.stats[id]
	owner_label.text = "Seu jogador: P%d" % id if network.active else "Controle local: P%d" % id
	turn_label.text = "Turno: %s" % str(turns.get_state()["t"]).replace("IA_NEUTRA", "Monstros")
	hp_bar.max_value = stats.max_hp
	hp_bar.value = stats.hp
	hp_label.text = "HP  %d / %d" % [stats.hp, stats.max_hp]
	pa_bar.max_value = stats.max_pa
	pa_bar.value = stats.pa
	pa_label.text = "PA  %d / %d" % [stats.pa, stats.max_pa]


func _result(losers: PackedInt32Array, winner: int, finished: bool) -> void:
	refresh()
	if not network.active:
		# O modo local alterna o controle de P1/P2 no mesmo computador.
		if not finished:
			return
		result_label.text = "Vitória — P%d" % winner if winner > 0 else "Derrota — jogadores eliminados"
	else:
		var id := local_id()
		if winner == id:
			result_label.text = "Vitória"
		elif losers.has(id) or game.dead.has(id) or finished:
			result_label.text = "Derrota"
		else:
			return
	overlay.show()


func _restart() -> void:
	overlay.hide()
	refresh()
