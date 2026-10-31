using Godot;

public partial class GameOverScreen : CanvasLayer
{
	public override void _Ready()
	{
		// Buscamos el botón en la escena y conectamos su señal de presionado
		Button botonMenu = GetNodeOrNull<Button>("Button"); 
		if (botonMenu != null)
		{
			botonMenu.Pressed += AlPresionarMenu;
		}
	}

	public override void _Input(InputEvent @event)
	{
		// Espacio para reiniciar el nivel directamente
		if (@event.IsActionPressed("ui_accept"))
		{
			ReiniciarJuego();
		}
		
		// Si presionan la tecla 'M' (o podés cambiarla por otra), vuelve al menú principal
		if (@event is InputEventKey eventKey && eventKey.Pressed && eventKey.Keycode == Key.M)
		{
			AlPresionarMenu();
		}
	}

	private void ReiniciarJuego()
	{
		if (GameManager.instance != null)
		{
			GameManager.instance.vidas = 3;
			GameManager.instance.puntos = 0;
			GameManager.instance.nivelActual = 1;
		}
		GetTree().ChangeSceneToFile("res://Escenas/Lvl1.tscn");
	}

	private void AlPresionarMenu()
	{
		if (GameManager.instance != null)
		{
			GameManager.instance.vidas = 3;
			GameManager.instance.puntos = 0;
			GameManager.instance.nivelActual = 1;
		}
		
		// Cambiamos a la escena del menú principal
		GetTree().ChangeSceneToFile("res://Escenas/MainMenu.tscn");
	}
}
