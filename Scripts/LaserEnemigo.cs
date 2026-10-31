using Godot;

public partial class LaserEnemigo : Area2D
{
	[Export] public float velocidad = 300.0f;
	private bool yaColisiono = false; // Bandera para evitar doble daño

	public override void _Process(double delta)
	{
		// El láser enemigo baja por la pantalla
		Position += new Vector2(0, velocidad * (float)delta);

		// Si se va por debajo de la pantalla, lo eliminamos
		if (Position.Y > 800)
		{
			QueueFree();
		}
	}

	// Detecta choques contra otras áreas
	private void _on_area_entered(Area2D area)
	{
		VerificarImpacto(area);
	}

	// Detecta choques contra el cuerpo físico del jugador
	private void _on_body_entered(Node2D body)
	{
		VerificarImpacto(body);
	}

	// Función encargada de comprobar si chocamos contra el jugador
	private void VerificarImpacto(Node nodo)
	{
		// Si ya impactó antes, no hacemos nada para que no reste vida doble
		if (yaColisiono) return;

		// Si el láser recién sale y toca a otro alien cercano, lo ignoramos
		if (nodo is Area2D && nodo.Name.ToString().Contains("Area"))
		{
			return; 
		}

		// Verificamos si lo que tocamos es el jugador
		if (nodo != null && (nodo.IsInGroup("Jugador") || nodo.Name.ToString().ToLower().Contains("player") || nodo.Name == "CharacterBody2D" || nodo.GetParent().IsInGroup("Jugador")))
		{
			yaColisiono = true; // Marcamos que ya pegó

			// Si es el jugador, le sacamos una sola vida
			if (GameManager.instance != null)
			{
				GameManager.instance.perderVida();
			}
			
			// Destruimos el láser al impactar
			QueueFree(); 
		}
	}
}
