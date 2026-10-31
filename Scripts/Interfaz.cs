using Godot;

public partial class Interfaz : CanvasLayer
{
	private Label textoPuntos;
	private Label textoVidas;

	public override void _Ready()
	{
		// Buscamos los textos en la interfaz visual
		textoPuntos = GetNode<Label>("textoPuntos");
		textoVidas = GetNode<Label>("textoVida");
	}

	public override void _Process(double delta)
	{
		// Actualizamos en tiempo real los puntos y vidas en pantalla según el GameManager
		if (GameManager.instance != null)
		{
			textoPuntos.Text = "score: " + GameManager.instance.puntos;
			textoVidas.Text = "vida: " + GameManager.instance.vidas;
		}
	}
}
