using Godot;

public partial class Laser : Area2D
{
	[Export] public float velocidad = 400.0f;
	private bool hasHit = false; // Bandera para que el láser pegue una sola vez

	public override void _Process(double delta)
	{
		// Mover el láser hacia arriba
		Position -= new Vector2(0, velocidad * (float)delta);

		// Si sale de la pantalla por arriba, lo borramos para liberar memoria
		if (Position.Y < -50)
		{
			QueueFree();
		}
	}

	// Función que se activa cuando el láser toca el área de un alien
	private void _on_area_entered(Area2D area)
	{
		if (hasHit) return; // Si ya pegó, no hacemos nada más

		hasHit = true;
			
		// Le sumamos puntos al GameManager
		if (GameManager.instance != null)
		{
			GameManager.instance.sumarPuntos(10);
		}

		// Destruimos únicamente al alien al que le pegó
		area.QueueFree(); 
			
		// Destruimos el láser
		QueueFree();
	}
}
