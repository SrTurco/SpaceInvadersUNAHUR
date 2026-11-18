using Godot;

public enum TipoPowerUp
{
// Define los distintos tipos de mejoras que pueden soltar los enemigos especiales
	TripleTiro,
	DisparoRapido,
	Escudo
}

public partial class PowerUp : Area2D
{
	[Export] public TipoPowerUp tipo;
	[Export] public float velocidadCaida = 150.0f;

	public override void _Ready()
	{
		// Activo únicamente el sprite correspondiente al tipo de Power-Up asignado
		GetNode<Sprite2D>("DisparoTres").Visible = (tipo == TipoPowerUp.TripleTiro);
		GetNode<Sprite2D>("Rayo").Visible = (tipo == TipoPowerUp.DisparoRapido);
		GetNode<Sprite2D>("Escudo").Visible = (tipo == TipoPowerUp.Escudo);
	}

	public override void _Process(double delta)
	{
		Position += new Vector2(0, velocidadCaida * (float)delta);
	// Si el jugador no lo agarra y sale por debajo de la pantalla, lo elimino de memoria
		if (Position.Y > 800)
		{
			QueueFree();
		}
	}

	private void _on_body_entered(Node2D body)
	{
		// Verifico que el cuerpo que lo tocó sea el jugador
		if (body is Player jugador)
		{
			jugador.ActivarPowerUp(tipo);
			QueueFree();
		}
	}
}
