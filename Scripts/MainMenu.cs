using Godot;
using System;

public partial class MainMenu : CanvasLayer

{
	public override void _Ready()
	{
		// Conectamos el botón de la interfaz para volver al menú
		Button botonMenu = GetNodeOrNull<Button>("Button"); 
		if (botonMenu != null)
		{
			botonMenu.Pressed += AlPresionarMenu;
		}
	}

	public override void _Input(InputEvent @event)
	{
		
		// Si presionan la letra 'Enter', volvemos al menú principal
		if (@event is InputEventKey eventKey && eventKey.Pressed && eventKey.Keycode == Key.Enter)
		{
			ReiniciarJuego();
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
		
		GetTree().ChangeSceneToFile("res://Escenas/MainMenu.tscn");
	}
}
