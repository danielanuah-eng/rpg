extends CanvasLayer
## RTT de aplicação; TCP/WebSocket não expõe perda real de datagramas.
@onready var network: Node = get_node("/root/NetworkManager")
var label: Label
var menu: ColorRect
var message: Label
var _elapsed := 0.0

func _ready() -> void:
	layer = 20
	label = Label.new()
	label.position = Vector2(20, 108)
	label.add_theme_font_size_override("font_size", 12)
	label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(label)
	menu = ColorRect.new()
	menu.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	menu.color = Color(0.03, 0.04, 0.07, 0.95)
	add_child(menu)
	var center := CenterContainer.new()
	center.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	menu.add_child(center)
	var box := VBoxContainer.new()
	box.custom_minimum_size = Vector2(300, 0)
	center.add_child(box)
	message = Label.new()
	message.text = "Conexão encerrada"
	message.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	box.add_child(message)
	var local_button := Button.new()
	local_button.text = "Jogar localmente"
	local_button.pressed.connect(func(): network.resume_local(); menu.hide())
	box.add_child(local_button)
	var reconnect := Button.new()
	reconnect.text = "Voltar à conexão"
	reconnect.pressed.connect(func(): menu.hide())
	box.add_child(reconnect)
	menu.hide()
	network.connection_lost.connect(_lost)
	network.session_started.connect(menu.hide)
	network.session_ended.connect(menu.hide)
	_update_text()

func _lost(reason: String) -> void:
	message.text = reason
	menu.show()

func _process(delta: float) -> void:
	_elapsed += delta
	if _elapsed >= 0.25:
		_elapsed = 0.0
		_update_text()

func _update_text() -> void:
	var info: Dictionary = network.debug_metrics()
	var ping := "—" if info["rtt_ms"] < 0 else "%d ms" % int(info["rtt_ms"])
	label.text = "%s | FPS %d | RTT %s | Sondas expiradas %.0f%% (%d) | Cliques filtrados %d" % [
		info["role"], Engine.get_frames_per_second(), ping,
		info["timeout_percent"], info["samples"], info["suppressed"]
	]
