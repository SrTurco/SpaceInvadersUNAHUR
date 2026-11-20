using Godot;

public partial class LaserEnemigo : Area2D
{
	[Export] public float velocidad = 300.0f;
	private bool yaColisiono = false; // Bandera para que el láser impacte una sola vez

	private Sprite2D sprite;
	private float tiempoAnimacion;
	private Vector2 direccion = Vector2.Down;

	// Se fija una sola vez al disparar: no sigue al jugador después.
	public void ApuntarA(Vector2 objetivo)
	{
		Vector2 desplazamiento = objetivo - GlobalPosition;
		if (desplazamiento.LengthSquared() > 0.01f)
			direccion = desplazamiento.Normalized();
		Rotation = direccion.Angle() - Mathf.Pi / 2.0f;
	}

	public override void _Ready()
	{
		sprite = GetNode<Sprite2D>("Sprite2D");
	}

	public override void _Process(double delta)
	{
		tiempoAnimacion += (float)delta;
		sprite.Scale = new Vector2(0.85f + 0.15f * Mathf.Sin(tiempoAnimacion * 22.0f), 1.0f);
		// El láser del enemigo baja hacia la nave del jugador
		Position += direccion * velocidad * (float)delta;

		// Si se va por abajo de la pantalla, lo borramos para liberar memoria
		if (GlobalPosition.Y > GetViewportRect().Size.Y + 50 || GlobalPosition.Y < -50 ||
			GlobalPosition.X < -50 || GlobalPosition.X > GetViewportRect().Size.X + 50)
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

		if (nodo is BloqueRefugio bloque && !bloque.IsQueuedForDeletion())
		{
			yaColisiono = true;
			bloque.RecibirImpacto();
			QueueFree();
			return;
		}

		bool esJugador = nodo is Player;

		if (esJugador)
		{
			yaColisiono = true; // Marcamos que ya hizo efecto
			GameManager.instance?.crearExplosion(GlobalPosition, new Color("73dcff"));

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
