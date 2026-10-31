using Godot;

public partial class NivelCompletado : CanvasLayer
{
	private Label textoNivel;

	public override void _Ready()
	{
		textoNivel = GetNode<Label>("Label");

		// Mostramos dinámicamente el número de nivel completado
		if (GameManager.instance != null)
		{
			textoNivel.Text = "¡Nivel " + GameManager.instance.nivelActual + " Completado!\n\nPresioná ESPACIO para continuar";
		}
		else
		{
			textoNivel.Text = "¡Horda Exterminada!\n\nPresioná ESPACIO para continuar";
		}
	}

	public override void _Input(InputEvent @event)
	{
		// Detectamos si el jugador presiona la barra espaciadora
		if (@event.IsActionPressed("ui_accept"))
		{
			if (GameManager.instance != null)
			{
				GameManager.instance.siguienteNivel();
			}
			else
			{
				// Por si probás la escena suelta
				GetTree().ChangeSceneToFile("res://Escenas/Lvl2.tscn");
			}
		}
	}
}
