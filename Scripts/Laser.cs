using Godot;

public partial class Laser : Area2D
{
	[Export] public float velocidad = 600.0f;
	private bool yaPego = false; // para que no atraviese a todos

	public override void _Process(double delta)
	{
		Position += new Vector2(0, -velocidad * (float)delta);

		// si se va arriba de todo lo borramos para no gastar memoria
		if (Position.Y < -50)
		{
			QueueFree();
		}
	}

	private void _on_area_entered(Area2D area)
	{
		if (yaPego) return;

		// si le pegamos a un alien
		if (area.IsInGroup("Aliens"))
		{
			yaPego = true;

			if (GameManager.instance != null)
			{
				GameManager.instance.sumarPuntos(10);
			}

			area.QueueFree(); // vuela el alien
			QueueFree();      // vuela el laser
		}
	}
}
