extends Node

@export var build_mode: bool;
var buildOverlayScene = preload("res://Buildings/build_mode.tscn");
var buildOverlay: Node;

var debounce = 0;
var current_cursor_shape: Input.CursorShape
# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	if (build_mode):
		enterBuildMode();
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:
	pass


func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		match event.button_index:
				MOUSE_BUTTON_RIGHT:
					debounce = debounce + 1;
					if (!build_mode):
						enterBuildMode();
					else:
						exitBuildMode();
					if (debounce == 2):
						debounce = 0;

func enterBuildMode() -> void:
	if (debounce != 1):
		return;
	
	buildOverlay = buildOverlayScene.instantiate();
	add_child(buildOverlay);
	build_mode = true;
	Input.set_default_cursor_shape(Input.CURSOR_CROSS)

func exitBuildMode() -> void:
	if (debounce == 2):
		return;
		
	build_mode = false;
	buildOverlay.queue_free();
	Input.set_default_cursor_shape(Input.CURSOR_ARROW)
