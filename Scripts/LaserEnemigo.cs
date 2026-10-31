using Godot;

public partial class LaserEnemigo : Area2D
{
	[Export] public float velocidad = 300.0f;

	public override void _Process(double delta)
	{
		// El láser se mueve hacia abajo
		Position += new Vector2(0, velocidad * (float)delta);

		// Si sale de la pantalla por abajo, se elimina
		if (Position.Y > 800)
		{
			QueueFree();
		}
	}

	// Detecta cuando choca con un Area2D
	private void _on_area_entered(Area2D area)
	{
		VerificarImpacto(area);
	}

	// Detecta cuando choca con un CharacterBody2D o cuerpo físico (como el Player)
	private void _on_body_entered(Node2D body)
	{
		VerificarImpacto(body);
	}

	private void VerificarImpacto(Node nodo)
{
	// Si el láser se toca a sí mismo o a otro alien (Area2D), lo ignoramos para que no explote solo
	if (nodo is Area2D && nodo.Name.ToString().Contains("Area"))
	{
		return; 
	}

	GD.Print("Tocó a un objeto válido: " + nodo.Name);

	// Verificamos si es el jugador (por grupo, por nombre o por ser el CharacterBody2D)
	if (nodo != null && (nodo.IsInGroup("Jugador") || nodo.Name.ToString().ToLower().Contains("player") || nodo.Name == "CharacterBody2D" || nodo.GetParent().IsInGroup("Jugador")))
	{
		GD.Print("¡Impacto confirmado en el jugador!");
		if (GameManager.instance != null)
		{
			GameManager.instance.perderVida();
		}
		QueueFree(); // Destruimos el láser tras dañar al jugador
	}
}
}
