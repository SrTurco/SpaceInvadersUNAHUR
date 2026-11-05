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

	[Export] public float offsetSuperior = 80.0f;

	public int direccion = 1; 
	private Vector2 tamanoPantalla;
	private Vector2 posicionInicial;

	public override void _Ready()
	{
		tamanoPantalla = GetViewportRect().Size;
		CentrarYCrearAliens();
	}

	public override void _Process(double delta)
	{
		// 1. Muevo a toda la horda para los costados
		Position += new Vector2(velocidad * direccion * (float)delta, 0);

		float anchoHorda = (columnas - 1) * espacioH;

		// 2. Si tocamos los bordes de la pantalla, bajamos un toque y cambiamos de dirección
		if (Position.X + anchoHorda >= tamanoPantalla.X - 20 && direccion == 1)
		{
			BajarYCambiar();
		}
		else if (Position.X <= 20 && direccion == -1)
		{
			BajarYCambiar();
		}

		// 3. Control de altura: Si la horda baja demasiado y llega cerca de la base, perdemos vida y vuelve arriba
		if (Position.Y >= 600.0f)
		{
			if (GameManager.instance != null)
			{
				GameManager.instance.perderVida();
			}
			Position = posicionInicial; 
		}

		// 4. Si matamos a todos los bichos, ganamos el nivel
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

		int nivelActual = (GameManager.instance != null) ? GameManager.instance.nivelActual : 1;

		for (int r = 0; r < filas; r++)
		{
			for (int c = 0; c < columnas; c++)
			{
				Enemigo alien = (Enemigo)alienEscena.Instantiate();
				
				// REGLAS SEGÚN EL NIVEL:
				if (nivelActual == 1)
				{
					// LVL 1: Todos normales (Skin 1), sin zigzag
					alien.tipo = TipoEnemigo.Normal;
				}
				else
				{
					// LVL 2 y LVL 3: Los laterales hacen zigzag y el resto normal/tirador
					if (c == 0 || c == columnas - 1)
					{
						alien.tipo = TipoEnemigo.ZigZag;
					}
					else if (r == 0 && nivelActual == 3)
					{
						alien.tipo = TipoEnemigo.Tirador;
					}
					else
					{
						alien.tipo = TipoEnemigo.Normal;
					}
				}

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
		int nivelActual = (GameManager.instance != null) ? GameManager.instance.nivelActual : 1;
		if (nivelActual < 3) return; // Solo disparamos en el nivel 3

		List<Enemigo> tiradores = new List<Enemigo>();

		foreach (Node hijo in GetChildren())
		{
			if (hijo is Enemigo alien && alien.tipo == TipoEnemigo.Tirador)
			{
				tiradores.Add(alien);
			}
		}

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
