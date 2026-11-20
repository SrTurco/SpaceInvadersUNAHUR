using Godot;

public partial class Laser : Area2D
{
	[Export] public float velocidad = 400.0f;
	[Export] public PackedScene powerUpEscena;
	private bool hasHit = false; // Bandera para que el láser impacte una sola vez y no haga doble daño

	private Sprite2D sprite;
	private float tiempoAnimacion;

	public override void _Ready()
	{
		sprite = GetNode<Sprite2D>("Sprite2D");
	}

	public override void _Process(double delta)
	{
		tiempoAnimacion += (float)delta;
		sprite.Scale = new Vector2(0.85f + 0.15f * Mathf.Sin(tiempoAnimacion * 22.0f), 1.0f);
		// El láser sube por la pantalla
		Position -= new Vector2(0, velocidad * (float)delta);

		// Si se sale por arriba de la pantalla, lo borramos de memoria para no gastar recursos
		if (Position.Y < -50)
		{
			QueueFree();
		}
	}

	// Detecta cuando el láser toca a un alien
	private void _on_area_entered(Area2D area)
	{
		if (hasHit) return;

		// Si choca contra un disparo enemigo, desaparecen ambos
		if (area is LaserEnemigo laserEnemigo)
		{
			hasHit = true;
			laserEnemigo.QueueFree();
			QueueFree();
			return;
		}

		if (area is BloqueRefugio bloque && !bloque.IsQueuedForDeletion())
		{
			hasHit = true;
			bloque.RecibirImpacto();
			QueueFree();
		}
		else if (area is Boss boss)
		{
			hasHit = true;
			boss.RecibirGolpe();
			QueueFree();
		}
		else if (area is Enemigo enemigo && !area.IsQueuedForDeletion())
		{
			hasHit = true;

			// Si el enemigo es portador de un PowerUp, lo soltamos
			if (enemigo.tienePowerUp && powerUpEscena != null)
			{
				PowerUp item = (PowerUp)powerUpEscena.Instantiate();
				int randomTipo = (int)(GD.Randi() % 3);
				item.tipo = (TipoPowerUp)randomTipo;
				item.GlobalPosition = enemigo.GlobalPosition;
				GetTree().CurrentScene.CallDeferred("add_child", item);
			}

			enemigo.Explotar();
			QueueFree();
		}
	}
}
