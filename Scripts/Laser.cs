using Godot;

public partial class Laser : Area2D
{
	[Export] public float velocidad = 400.0f;
	private bool hasHit = false; // Sirve para que el láser impacte una sola vez y no haga doble daño

	public override void _Process(double delta)
	{
		// El láser sube por la pantalla
		Position -= new Vector2(0, velocidad * (float)delta);

		// Si sale de la pantalla por arriba, lo borramos para liberar memoria
		if (Position.Y < -50)
		{
			QueueFree();
		}
	}

	// Detecta cuando el láser toca a un alien
	private void _on_area_entered(Area2D area)
	{
		if (hasHit) return; 
		hasHit = true;
			
		// Sumamos puntos a través del administrador general
		if (GameManager.instance != null)
		{
			GameManager.instance.sumarPuntos(10);
		}

		// Borramos al alien al que le pegamos y también destruimos nuestro láser
		area.QueueFree(); 
		QueueFree();
	}
}
