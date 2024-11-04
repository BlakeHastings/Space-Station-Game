extends Node

@export var fillShape: Polygon2D
@export var Point: Polygon2D


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass


func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		if Input.is_mouse_button_pressed(MOUSE_BUTTON_LEFT):
			handleLeftMouseButton(event);
	

func handleLeftMouseButton(event: InputEventMouseButton) -> void:
	var polygons = fillShape.polygon;
	polygons.append(event.position)
	fillShape.set_polygons(polygons);
	
	print(fillShape.polygon);
	print("Mouse Button Left Pressed");
	
