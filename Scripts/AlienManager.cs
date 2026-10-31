using Godot;

public partial class AlienManager : Node2D
{
	// Datos configurables desde el editor para armar los enemigos
	[Export] public PackedScene alienEscena;
	[Export] public int filas = 3;
	[Export] public int columnas = 8;
	[Export] public float espacioH = 60.0f;
	[Export] public float espacioV = 50.0f;
	[Export] public float velocidad = 80.0f;
	[Export] public PackedScene laserEnemigoEscena;

	// Dirección en la que se mueve la horda (1 es derecha, -1 es izquierda)
	private int direccion = 1;

	public override void _Ready()
	{
		// Apenas arranca el nivel, armamos la formación de aliens
		CrearAliens();
	}

	public override void _Process(double delta)
	{
		// 1. Movemos a los aliens de un lado a otro horizontalmente
		Position += new Vector2(velocidad * direccion * (float)delta, 0);

		// 2. Si la horda toca el borde derecho, baja un escalón y va hacia la izquierda
		if (Position.X + (columnas * espacioH) > 750 && direccion == 1)
		{
			BajarYCambiar();
		}
		// 3. Si toca el borde izquierdo, baja un escalón y va hacia la derecha
		else if (Position.X < 50 && direccion == -1)
		{
			BajarYCambiar();
		}

		// 4. Si los aliens bajan demasiado y llegan a la zona del jugador, este pierde vida
		if (Position.Y > 400)
		{
			if (GameManager.instance != null)
			{
				GameManager.instance.perderVida();
			}
			
			// Devolvemos la horda arriba para reiniciar el ciclo
			Position = new Vector2(100, 100); 
		}

		// 5. Si ya no quedan aliens vivos, ganamos el nivel
		if (GetChildCount() == 0)
		{
			if (GameManager.instance != null)
			{
				GameManager.instance.pasarANivelCompletado();
			}
		}
	}

	// Función para dibujar la grilla de enemigos en filas y columnas
	private void CrearAliens()
	{
		for (int r = 0; r < filas; r++)
		{
			for (int c = 0; c < columnas; c++)
			{
				Node2D alien = (Node2D)alienEscena.Instantiate();
				
				float x = c * espacioH;
				float y = r * espacioV;
				
				alien.Position = new Vector2(x, y);
				AddChild(alien);
			}
		}
	}

	// Función para que la horda baje y cambie de rumbo
	private void BajarYCambiar()
	{
		direccion *= -1; // Invierte la dirección
		Position += new Vector2(0, 20); // Baja un poco en vertical
	}
	
	// Función que se ejecuta cada vez que el temporizador (Timer) se completa para disparar
	private void _on_timer_timeout()
	{
		int cantidadAliens = GetChildCount();
		
		if (cantidadAliens > 0 && laserEnemigoEscena != null)
		{
			// Elegimos un alien al azar de la horda
			int indiceAleatorio = (int)(GD.Randi() % (uint)cantidadAliens);
			Node hijo = GetChild(indiceAleatorio);

			if (hijo is Node2D alienAleatorio)
			{
				// Creamos el disparo enemigo y lo ubicamos abajo del alien elegido
				Node2D laserE = (Node2D)laserEnemigoEscena.Instantiate();
				laserE.GlobalPosition = alienAleatorio.GlobalPosition + new Vector2(0, 35);
				GetTree().CurrentScene.AddChild(laserE);
			}
		}
	}
}
