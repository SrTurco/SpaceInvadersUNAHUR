using Godot;

public partial class Interfaz : CanvasLayer
{
	private Label textoPuntos;
	private Label textoVidas;

	public override void _Ready()
	{
		// Vinculamos los labels con los nodos que creaste en la escena
		textoPuntos = GetNode<Label>("textoPuntos");
		textoVidas = GetNode<Label>("textoVida");
	}

	public override void _Process(double delta)
	{
		if (GameManager.instance != null)
		{
			// Actualizamos el texto sumándole el valor real del GameManager
			textoPuntos.Text = "score: " + GameManager.instance.puntos;
			textoVidas.Text = "vida: " + GameManager.instance.vidas;
		}
	}
}
