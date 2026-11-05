using Godot;

public partial class LaserEnemigo : Area2D
{
	[Export] public float velocidad = 300.0f;
	private bool yaColisiono = false; // Bandera para que el láser impacte una sola vez

	public override void _Process(double delta)
	{
		// El láser del enemigo baja hacia la nave del jugador
		Position += new Vector2(0, velocidad * (float)delta);

		// Si se va por abajo de la pantalla, lo borramos para liberar memoria
		if (Position.Y > 800)
		{
			QueueFree();
		}
	}

	// Detecta cuando entra en contacto con otra área
	private void _on_area_entered(Area2D area)
	{
		VerificarImpacto(area);
	}

	// Detecta cuando entra en contacto con un cuerpo físico
	private void _on_body_entered(Node2D body)
	{
		VerificarImpacto(body);
	}

	// Función para verificar contra qué chocamos
	private void VerificarImpacto(Node nodo)
	{
		if (yaColisiono) return;
		if (nodo == null) return;

		// NUEVO: Si lo que tocamos es un Enemigo (cualquiera, sea Normal, Tirador o ZigZag), lo ignoramos por completo
		if (nodo is Enemigo || nodo.Name.ToString().ToLower().Contains("enemigo") || (nodo.GetParent() != null && nodo.GetParent() is Enemigo))
		{
			return; 
		}

		// Nos fijamos si lo que tocamos es el jugador
		bool esJugador = nodo.IsInGroup("Jugador") || 
						 nodo.Name.ToString().ToLower().Contains("player") || 
						 nodo.Name == "CharacterBody2D" || 
						 (nodo.GetParent() != null && nodo.GetParent().IsInGroup("Jugador"));

		if (esJugador)
		{
			yaColisiono = true; // Marcamos que ya hizo efecto

			// Le restamos una vida al jugador mediante el GameManager
			if (GameManager.instance != null)
			{
				GameManager.instance.perderVida();
			}
			
			// Destruimos el láser enemigo inmediatamente al impactar al jugador
			QueueFree(); 
		}
	}
}
