using Godot;

public partial class LaserEnemigo : Area2D
{
	[Export] public float velocidad = 300.0f;
	private bool yaColisiono = false; // Para evitar que nos reste vida dos veces

	public override void _Process(double delta)
	{
		// El láser del enemigo baja hacia el jugador
		Position += new Vector2(0, velocidad * (float)delta);

		// Si se va por abajo de la pantalla, lo borramos
		if (Position.Y > 800)
		{
			QueueFree();
		}
	}

	private void _on_area_entered(Area2D area)
	{
		VerificarImpacto(area);
	}

	private void _on_body_entered(Node2D body)
	{
		VerificarImpacto(body);
	}

	// Función para chequear si le pegamos al jugador posta
	private void VerificarImpacto(Node nodo)
	{
		if (yaColisiono) return;

		// Si recién sale y toca a otro alien por accidente, lo ignoramos
		if (nodo is Area2D && nodo.Name.ToString().Contains("Area"))
		{
			return; 
		}

		// Me fijo si lo que tocamos es el jugador
		if (nodo != null && (nodo.IsInGroup("Jugador") || nodo.Name.ToString().ToLower().Contains("player") || nodo.Name == "CharacterBody2D" || nodo.GetParent().IsInGroup("Jugador")))
		{
			yaColisiono = true; // Marcamos que ya pegó

			// Le restamos una vida al jugador
			if (GameManager.instance != null)
			{
				GameManager.instance.perderVida();
			}
			
			// Destruimos el láser enemigo
			QueueFree(); 
		}
	}
}
