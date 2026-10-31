using Godot;

public partial class AlienManager : Node2D
{
	// Variables exportadas para poder configurar la horda fácil desde el editor de Godot
	[Export] public PackedScene alienEscena;
	[Export] public int filas = 3;
	[Export] public int columnas = 8;
	[Export] public float espacioH = 60.0f;
	[Export] public float espacioV = 50.0f;
	[Export] public float velocidad = 80.0f;

	// Variable para saber si se mueven a la derecha (1) o a la izquierda (-1)
	private int direccion = 1;

	public override void _Ready()
	{
		// Apenas arranca la escena, llamamos a la función que arma la grilla de enemigos
		crearAliens();
	}

	public override void _Process(double delta)
	{
		// 1. Mover la horda de un lado a otro horizontalmente
		Position += new Vector2(velocidad * direccion * (float)delta, 0);

		// 2. Si la horda toca el borde derecho, baja y cambia de dirección a la izquierda
		if (Position.X + (columnas * espacioH) > 750 && direccion == 1)
		{
			bajarYCambiar();
		}
		// 3. Si toca el borde izquierdo, baja y cambia de dirección a la derecha
		else if (Position.X < 50 && direccion == -1)
		{
			bajarYCambiar();
		}

		// 4. Si la horda baja demasiado y llega a la zona baja (contacto con el jugador)
		if (Position.Y > 400)
		{
			if (GameManager.instance != null)
			{
				GameManager.instance.perderVida();
			}
			
			// Reiniciamos la posición de la horda arriba para que vuelva a empezar
			Position = new Vector2(100, 100); 
		}

		// 5. Si ya no quedan aliens vivos (hijos), pasamos a la pantalla de nivel completado
		if (GetChildCount() == 0)
		{
			if (GameManager.instance != null)
			{
				GameManager.instance.pasarANivelCompletado();
			}
		}
	}

	// Función para instanciar y ubicar a cada alien en forma de grilla
	private void crearAliens()
	{
		for (int r = 0; r < filas; r++)
		{
			for (int c = 0; c < columnas; c++)
			{
				// Creamos una copia del alien
				Node2D alien = (Node2D)alienEscena.Instantiate();
				
				// Calculamos su posición según la columna y la fila
				float x = c * espacioH;
				float y = r * espacioV;
				
				alien.Position = new Vector2(x, y);
				
				// Lo sumamos como hijo del AlienManager
				AddChild(alien);
			}
		}
	}

	// Función para hacer que la horda baje un escalón e invierta su rumbo
	private void bajarYCambiar()
	{
		direccion *= -1; // Invierte el valor (si era 1 pasa a -1, y viceversa)
		Position += new Vector2(0, 20); // Baja un poco en el eje Y
	}
}
