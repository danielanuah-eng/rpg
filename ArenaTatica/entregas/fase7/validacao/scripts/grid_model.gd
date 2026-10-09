extends RefCounted
## Dados da grelha, independentes de cena, câmera, entrada e rede.
## Uma célula usa (coluna, linha); a linha corresponde ao eixo Z no mundo.

var columns: int = 0
var rows: int = 0
var cells: Dictionary = {}


func configure(new_columns: int, new_rows: int) -> void:
	columns = maxi(new_columns, 1)
	rows = maxi(new_rows, 1)
	cells.clear()
	for row in range(rows):
		for column in range(columns):
			cells[Vector2i(column, row)] = {
				"walkable": true,
				"blocks_los": false,
				"movement_cost": 1
			}


func contains_cell(cell: Vector2i) -> bool:
	return cells.has(cell)


func get_cell_data(cell: Vector2i) -> Dictionary:
	# Uma cópia impede que a apresentação altere as regras por acidente.
	if not contains_cell(cell):
		return {}
	var data: Dictionary = cells[cell]
	return data.duplicate(true)


func get_cells_in_order() -> Array[Vector2i]:
	# Ordem explícita; não depende de IDs de nós ou da física local.
	var result: Array[Vector2i] = []
	for row in range(rows):
		for column in range(columns):
			result.append(Vector2i(column, row))
	return result

