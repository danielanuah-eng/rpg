extends Node3D
## Monta a apresentação. Regras de turno e transporte de rede não vivem aqui.

const BoardScript = preload("res://scripts/board.gd")

@export_range(1.0, 2.0, 0.05) var framing_margin: float = 1.15
@export_range(0.0, 5.0, 0.1) var headroom: float = 0.0

@onready var board: BoardScript = $Board
@onready var camera: Camera3D = $Camera3D


func _ready() -> void:
	board.board_generated.connect(frame_board)
	get_viewport().size_changed.connect(frame_board)
	# Board, como filho, já gerou a grelha no seu _ready().
	frame_board()


func frame_board() -> void:
	var bounds := board.get_local_bounds()
	bounds.size.y += headroom
	var center: Vector3 = board.to_global(bounds.get_center())
	var radius: float = 0.0
	for index in range(8):
		var corner: Vector3 = board.to_global(bounds.get_endpoint(index))
		radius = maxf(radius, center.distance_to(corner))

	# Componentes iguais dão azimute de 45° e elevação de ~35,26°.
	var distance: float = maxf(radius * 3.0, 10.0)
	camera.global_position = center + Vector3.ONE.normalized() * distance
	camera.look_at(center, Vector3.UP)
	camera.projection = Camera3D.PROJECTION_ORTHOGONAL
	camera.keep_aspect = Camera3D.KEEP_HEIGHT
	camera.near = 0.05
	camera.far = distance + radius + 10.0
	camera.current = true

	var half_width: float = 0.0
	var half_height: float = 0.0
	for index in range(8):
		var world_corner: Vector3 = board.to_global(bounds.get_endpoint(index))
		var camera_corner := camera.to_local(world_corner)
		half_width = maxf(half_width, absf(camera_corner.x))
		half_height = maxf(half_height, absf(camera_corner.y))

	var viewport_size := get_viewport().get_visible_rect().size
	var aspect: float = maxf(viewport_size.x, 1.0) / maxf(viewport_size.y, 1.0)
	var required_height: float = maxf(half_height * 2.0, half_width * 2.0 / aspect)
	camera.size = maxf(required_height * framing_margin, 1.0)

