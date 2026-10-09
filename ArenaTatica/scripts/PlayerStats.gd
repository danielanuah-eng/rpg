extends Node

signal changed
signal hp_changed(value: int, maximum: int)
signal pa_changed(value: int, maximum: int)
signal died

var _death_reported := false

@export_range(1, 10000) var max_hp: int = 100
@export_range(1, 100) var max_pa: int = 2

var _hp: int = 100
var _pa: int = 0
var hp: int:
	get:
		return _hp
var pa: int:
	get:
		return _pa


func _ready() -> void:
	_hp = max_hp


func is_alive() -> bool:
	return _hp > 0


func snapshot() -> Dictionary:
	return {"hp": _hp, "pa": _pa}


func server_reset() -> void:
	if multiplayer.is_server():
		_death_reported = false
		_hp = max_hp
		_pa = 0
		_publish_changes()


func server_restore_pa() -> void:
	if multiplayer.is_server():
		_pa = max_pa if is_alive() else 0
		_publish_changes()


func server_spend_pa(cost: int) -> bool:
	if not multiplayer.is_server() or not is_alive() or cost <= 0 or _pa < cost:
		return false
	_pa -= cost
	_publish_changes()
	return true


func server_damage(amount: int) -> int:
	if not multiplayer.is_server() or amount <= 0:
		return 0
	var damage: int = mini(amount, _hp)
	_hp -= damage
	_publish_changes()
	return damage


func _publish_changes() -> void:
	changed.emit()
	hp_changed.emit(_hp, max_hp)
	pa_changed.emit(_pa, max_pa)
	if multiplayer.is_server() and _hp <= 0 and not _death_reported:
		_death_reported = true
		died.emit()


func valid_snapshot(data: Dictionary) -> bool:
	return data.size() == 2 and typeof(data.get("hp")) == TYPE_INT and \
		typeof(data.get("pa")) == TYPE_INT and data["hp"] >= 0 and \
		data["hp"] <= max_hp and data["pa"] >= 0 and data["pa"] <= max_pa


func apply_snapshot(data: Dictionary) -> void:
	if multiplayer.is_server() or multiplayer.get_remote_sender_id() != 1:
		return
	if valid_snapshot(data):
		_hp = data["hp"]
		_pa = data["pa"]
		_publish_changes()
