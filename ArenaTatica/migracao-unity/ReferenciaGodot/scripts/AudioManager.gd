extends Node
## Recursos locais; no máximo 12 vozes vivas, liberadas ao terminar.
const PATHS := {
	&"attack": "res://audio/sfx/attack.wav",
	&"breath": "res://audio/sfx/breath.wav",
	&"death": "res://audio/sfx/death.wav",
	&"turn": "res://audio/sfx/turn.wav",
}
var _streams: Dictionary = {}
var _players: Array[AudioStreamPlayer] = []
var _unlocked := false

func _ready() -> void:
	for kind in PATHS:
		var path: String = PATHS[kind]
		var stream: AudioStream = null
		if ResourceLoader.exists(path):
			stream = ResourceLoader.load(path) as AudioStream
		if stream == null:
			push_warning("SFX indisponível: %s" % path)
			continue
		_streams[kind] = stream
	if DisplayServer.get_name() == "headless":
		return
	_unlocked = not OS.has_feature("web")
	get_node("../VisualFXManager").effect_received.connect(play_effect)
	get_node("/root/NetworkManager").session_ended.connect(_stop)

func _input(event: InputEvent) -> void:
	if event is InputEventMouseButton or event is InputEventScreenTouch or event is InputEventKey:
		if event.is_pressed():
			_unlocked = true

func _stop() -> void:
	for player in _players.duplicate():
		_release(player)

func _release(player: AudioStreamPlayer) -> void:
	_players.erase(player)
	if is_instance_valid(player):
		player.stop()
		player.stream = null
		player.queue_free()

func play_effect(kind: StringName) -> void:
	if not _unlocked or not _streams.has(kind):
		return
	if _players.size() >= 12:
		_release(_players[0])
	var selected := AudioStreamPlayer.new()
	selected.bus = "SFX" if AudioServer.get_bus_index("SFX") >= 0 else "Master"
	selected.volume_db = -12.0
	add_child(selected)
	_players.append(selected)
	selected.finished.connect(_release.bind(selected))
	selected.stream = _streams[kind]
	selected.play()
