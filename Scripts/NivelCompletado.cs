using Godot;

public partial class NivelCompletado : CanvasLayer
{
	private Label textoNivel;

	public override void _Ready()
	{
		textoNivel = GetNode<Label>("Label");

		// Mostramos qué nivel se acaba de completar
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
		// Si presionan espacio, pasamos al siguiente nivel
		if (@event.IsActionPressed("ui_accept"))
		{
			if (GameManager.instance != null)
			{
				GameManager.instance.siguienteNivel();
			}
			else
			{
				GetTree().ChangeSceneToFile("res://Escenas/Lvl2.tscn");
			}
		}
	}
}
