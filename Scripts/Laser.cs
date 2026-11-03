using Godot;

public partial class Laser : Area2D
{
	[Export] public float velocidad = 400.0f;
	private bool hasHit = false; // Bandera para que el láser impacte una sola vez y no haga doble daño

	public override void _Process(double delta)
	{
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
		hasHit = true;
			
		// Le aviso al GameManager que sume puntos
		if (GameManager.instance != null)
		{
			GameManager.instance.sumarPuntos(10);
		}

		// Borro al alien y también destruyo mi propio láser
		area.QueueFree(); 
		QueueFree();
	}
}
