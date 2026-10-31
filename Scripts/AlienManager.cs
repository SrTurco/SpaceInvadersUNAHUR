using Godot;

public partial class AlienManager : Node2D
{
	[Export] public PackedScene alienEscena;
	[Export] public int filas = 3;
	[Export] public int columnas = 8;
	[Export] public float espacioH = 60.0f;
	[Export] public float espacioV = 50.0f;
	[Export] public float velocidad = 80.0f;

	private int direccion = 1; // 1 para la derecha, -1 para la izquierda

	public override void _Ready()
	{
		crearAliens();
	}

	private void crearAliens()
	{
		for (int r = 0; r < filas; r++)
		{
			for (int c = 0; c < columnas; c++)
			{
				if (alienEscena != null)
				{
					Node2D alien = (Node2D)alienEscena.Instantiate();
					alien.Position = new Vector2(c * espacioH, r * espacioV);
					alien.AddToGroup("Aliens"); // por las dudas lo metemos al grupo
					AddChild(alien);
				}
			}
		}
	}

	public override void _Process(double delta)
	{
		// mover la horda lateralmente
		Position += new Vector2(velocidad * direccion * (float)delta, 0);

		// si llega al borde baja y cambia de direccion
		if (Position.X + (columnas * espacioH) > 750 && direccion == 1)
		{
			bajarYCambiar();
		}
		else if (Position.X < 50 && direccion == -1)
		{
			bajarYCambiar();
		}

		// si no quedan hijos (se murieron todos los aliens)
		if (GetChildCount() == 0)
		{
			GD.Print("ganaste el nivel!");
		}
	}

	private void bajarYCambiar()
	{
		direccion *= -1;
		Position += new Vector2(0, 20.0f);
		velocidad *= 1.05f; // aceleran un poquito
	}
}
