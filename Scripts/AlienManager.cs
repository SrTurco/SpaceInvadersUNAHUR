using Godot;
using System.Collections.Generic;

public partial class AlienManager : Node2D
{
	[Export] public PackedScene alienEscena;
	[Export] public int filas = 3;
	[Export] public int columnas = 8;
	[Export] public float espacioH = 60.0f;
	[Export] public float espacioV = 50.0f;
	[Export] public float velocidad = 80.0f;
	[Export] public PackedScene laserEnemigoEscena;

	// Margen superior para que no aparezcan pegados arriba
	[Export] public float offsetSuperior = 80.0f;

	private int direccion = 1;
	private Vector2 tamanoPantalla;
	private Vector2 posicionInicial;

	public override void _Ready()
	{
		// Obtenemos la resolución actual de la pantalla/viewport
		tamanoPantalla = GetViewportRect().Size;
		
		CentrarYCrearAliens();
	}

	public override void _Process(double delta)
	{
		// 1. Mover la horda
		Position += new Vector2(velocidad * direccion * (float)delta, 0);

		// Ancho total que ocupa la formación de aliens
		float anchoHorda = (columnas - 1) * espacioH;

		// 2. Colisión borde derecho
		if (Position.X + anchoHorda >= tamanoPantalla.X - 20 && direccion == 1)
		{
			BajarYCambiar();
		}
		// 3. Colisión borde izquierdo
		else if (Position.X <= 20 && direccion == -1)
		{
			BajarYCambiar();
		}

		// 4. Reinicio si bajan demasiado (adaptado al alto de pantalla)
		if (Position.Y > tamanoPantalla.Y - 100)
		{
			if (GameManager.instance != null)
			{
				GameManager.instance.perderVida();
			}
			
			// Restaurar a la posición inicial centrada
			Position = posicionInicial; 
		}

		// 5. Victoria si no quedan enemigos
		if (GetChildCount() == 0)
		{
			if (GameManager.instance != null)
			{
				GameManager.instance.pasarANivelCompletado();
			}
		}
	}

	private void CentrarYCrearAliens()
{
	float anchoHorda = (columnas - 1) * espacioH;
	float xInicial = (tamanoPantalla.X - anchoHorda) / 2.0f;
	float yInicial = offsetSuperior;

	posicionInicial = new Vector2(xInicial, yInicial);
	Position = posicionInicial;

	for (int r = 0; r < filas; r++)
	{
		for (int c = 0; c < columnas; c++)
		{
			Enemigo alien = (Enemigo)alienEscena.Instantiate();
			
			// 1. Asignamos el tipo de enemigo por fila
			if (r == 0)
			{
				alien.tipo = TipoEnemigo.Tirador;
			}
			else if (r == 1)
			{
				alien.tipo = TipoEnemigo.ZigZag;
			}
			else
			{
				alien.tipo = TipoEnemigo.Normal;
			}

			// 2. IMPORTANTE: Actualizamos la skin explícitamente
			alien.ConfigurarSkin();

			alien.Position = new Vector2(c * espacioH, r * espacioV);
			AddChild(alien);
		}
	}
}
	private void BajarYCambiar()
	{
		direccion *= -1;
		Position += new Vector2(0, 20);
	}

	private void _on_timer_timeout()
	{
		// Filtramos únicamente a los aliens que sean de tipo Tirador
		List<Enemigo> tiradores = new List<Enemigo>();

		foreach (Node hijo in GetChildren())
		{
			if (hijo is Enemigo alien && alien.tipo == TipoEnemigo.Tirador)
			{
				tiradores.Add(alien);
			}
		}

		// Si hay tiradores vivos, uno aleatorio dispara
		if (tiradores.Count > 0 && laserEnemigoEscena != null)
		{
			int indiceAleatorio = (int)(GD.Randi() % (uint)tiradores.Count);
			Enemigo tiradorElegido = tiradores[indiceAleatorio];

			Node2D laserE = (Node2D)laserEnemigoEscena.Instantiate();
			laserE.GlobalPosition = tiradorElegido.GlobalPosition + new Vector2(0, 35);
			GetTree().CurrentScene.AddChild(laserE);
		}
	}
}
