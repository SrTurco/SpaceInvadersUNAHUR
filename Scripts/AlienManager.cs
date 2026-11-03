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

	// Lo puse public para que los enemigos tiradores estáticos puedan leer la dirección de la horda
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
		// 1. Muevo toda la horda horizontalmente
		Position += new Vector2(velocidad * direccion * (float)delta, 0);

		float anchoHorda = (columnas - 1) * espacioH;

		// 2. Si tocamos el borde derecho o izquierdo, bajamos y cambiamos de rumbo
		if (Position.X + anchoHorda >= tamanoPantalla.X - 20 && direccion == 1)
		{
			BajarYCambiar();
		}
		else if (Position.X <= 20 && direccion == -1)
		{
			BajarYCambiar();
		}

		// 3. Si los aliens bajan demasiado, perdemos vida y reseteamos posición
		if (Position.Y > tamanoPantalla.Y - 100)
		{
			if (GameManager.instance != null)
			{
				GameManager.instance.perderVida();
			}
			Position = posicionInicial; 
		}

		// 4. Si ya no quedan hijos (matamos a todos), pasamos al cartel de nivel completado
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
				
				// Acá defino qué tipo de bicho va en cada lugar:
				// - Las columnas de las puntas (laterales) son los que hacen zigzag independiente
				if (c == 0 || c == columnas - 1)
				{
					alien.tipo = TipoEnemigo.ZigZag;
				}
				// - En el nivel 3, los de arriba del todo (salvo las puntas) son los tiradores estáticos
				else if (r == 0 && nivelActual == 3)
				{
					alien.tipo = TipoEnemigo.Tirador;
				}
				// - El resto son los normales de la horda común
				else
				{
					alien.tipo = TipoEnemigo.Normal;
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
		if (nivelActual < 3) return; // Solo disparamos si estamos en el nivel 3

		List<Enemigo> tiradores = new List<Enemigo>();

		// Busco cuáles son los enemigos tiradores que siguen vivos
		foreach (Node hijo in GetChildren())
		{
			if (hijo is Enemigo alien && alien.tipo == TipoEnemigo.Tirador)
			{
				tiradores.Add(alien);
			}
		}

		// Si hay tiradores, elijo uno al azar para que dispare su rayito
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
