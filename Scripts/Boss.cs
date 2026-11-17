using Godot;
using System.Collections.Generic;

public partial class Boss : Area2D
{
	[ExportGroup("Vida")]
	[Export] public int vidaMaxima = 30;
	[Export] public int puntosAlMorir = 500;

	[ExportGroup("Movimiento")]
	[Export] public float velocidad = 120.0f;
	[Export] public float velocidadEntrada = 100.0f;
	[Export] public float alturaCombate = 110.0f;   // altura donde se queda peleando
	[Export] public float margenLateral = 130.0f;   // distancia mínima a los bordes

	[ExportGroup("Esbirros")]
	[Export] public PackedScene esbirroEscena;      // Enemigo.tscn
	[Export] public float intervaloEsbirros = 6.0f; // segundos entre oleadas
	[Export] public int esbirrosPorOleada = 2;
	[Export] public int maxEsbirros = 6;            // tope vivos al mismo tiempo
	[Export] public float velocidadCaidaEsbirro = 70.0f;
	[Export] public float probabilidadPowerUp = 0.3f; // chance (0 a 1) de que un esbirro suelte power-up al morir

	[ExportGroup("Disparo")]
	[Export] public PackedScene laserEnemigoEscena; // LaserEnemigo.tscn
	[Export] public float intervaloDisparoBoss = 1.4f;
	[Export] public float intervaloDisparoEsbirros = 3.0f;

	private enum Estado { Entrando, Peleando, Muriendo }
	private Estado estado = Estado.Entrando;

	// Datos de cada esbirro (el boss los mueve, porque Enemigo.cs solo sabe moverse dentro de la horda)
	private class DatosEsbirro
	{
		public Enemigo nodo;
		public float xBase;
		public float y;
		public float t;
		public float fase;
	}

	private readonly List<DatosEsbirro> esbirros = new List<DatosEsbirro>();

	private Vector2 tamanoPantalla;
	private int vida;
	private int direccion = 1;
	private bool fase2 = false;

	private float tiempoEsbirros = 0.0f;
	private float tiempoDisparoBoss = 1.0f;
	private float tiempoDisparoEsbirros = 3.0f;

	private ProgressBar barraVida;

	public override void _Ready()
	{
		tamanoPantalla = GetViewportRect().Size;
		vida = vidaMaxima;
		direccion = (GD.Randi() % 2 == 0) ? 1 : -1;
		CrearBarraDeVida();
	}

	public override void _Process(double delta)
	{
		float dt = (float)delta;

		switch (estado)
		{
			case Estado.Entrando:
				// Baja hasta la altura de combate. Mientras tanto no recibe daño.
				Position += new Vector2(0, velocidadEntrada * dt);
				if (Position.Y >= alturaCombate)
				{
					Position = new Vector2(Position.X, alturaCombate);
					estado = Estado.Peleando;
					tiempoEsbirros = intervaloEsbirros * 0.5f; // primera oleada más rápido
				}
				break;

			case Estado.Peleando:
				MoverBoss(dt);
				ActualizarDisparos(dt);
				break;
		}

		MoverEsbirros(dt);
	}

	private void MoverBoss(float dt)
	{
		float vel = velocidad * (fase2 ? 1.5f : 1.0f);
		Position += new Vector2(vel * direccion * dt, 0);

		float minX = margenLateral;
		float maxX = tamanoPantalla.X - margenLateral;

		if (Position.X >= maxX)
		{
			Position = new Vector2(maxX, Position.Y);
			direccion = -1;
		}
		else if (Position.X <= minX)
		{
			Position = new Vector2(minX, Position.Y);
			direccion = 1;
		}
	}

	private void ActualizarDisparos(float dt)
	{
		tiempoDisparoBoss -= dt;
		if (tiempoDisparoBoss <= 0)
		{
			DispararBoss();
			tiempoDisparoBoss = intervaloDisparoBoss * (fase2 ? 0.6f : 1.0f);
		}

		tiempoDisparoEsbirros -= dt;
		if (tiempoDisparoEsbirros <= 0)
		{
			DispararDesdeEsbirro();
			tiempoDisparoEsbirros = intervaloDisparoEsbirros * (fase2 ? 0.6f : 1.0f);
		}

		tiempoEsbirros -= dt;
		if (tiempoEsbirros <= 0)
		{
			InvocarEsbirros();
			tiempoEsbirros = intervaloEsbirros * (fase2 ? 0.7f : 1.0f);
		}
	}

	private void DispararBoss()
	{
		// Fase 1: dos cañones. Fase 2: tres.
		float[] offsets = fase2 ? new float[] { -70.0f, 0.0f, 70.0f } : new float[] { -40.0f, 40.0f };
		foreach (float offsetX in offsets)
		{
			CrearLaser(GlobalPosition + new Vector2(offsetX, 70));
		}
	}

	private void DispararDesdeEsbirro()
	{
		// Solo disparan los que todavía están lejos del fondo de la pantalla
		List<DatosEsbirro> candidatos = new List<DatosEsbirro>();
		foreach (DatosEsbirro e in esbirros)
		{
			if (EsbirroVivo(e) && e.y < tamanoPantalla.Y - 200.0f)
			{
				candidatos.Add(e);
			}
		}

		if (candidatos.Count == 0) return;

		DatosEsbirro elegido = candidatos[(int)(GD.Randi() % (uint)candidatos.Count)];
		CrearLaser(elegido.nodo.GlobalPosition + new Vector2(0, 35));
	}

	private void CrearLaser(Vector2 posicion)
	{
		if (laserEnemigoEscena == null) return;

		Node2D laser = (Node2D)laserEnemigoEscena.Instantiate();
		laser.GlobalPosition = posicion;
		GetTree().CurrentScene.AddChild(laser);
	}

	private void InvocarEsbirros()
	{
		if (esbirroEscena == null) return;

		// Primero limpio la lista de los que ya murieron para contar bien
		esbirros.RemoveAll(e => !EsbirroVivo(e));

		int cantidad = Mathf.Min(esbirrosPorOleada + (fase2 ? 1 : 0), maxEsbirros - esbirros.Count);

		for (int i = 0; i < cantidad; i++)
		{
			Enemigo alien = (Enemigo)esbirroEscena.Instantiate();
			alien.tipo = TipoEnemigo.Tirador; // skin de "tirador", así se nota que dispara

			// Algunos esbirros salen dorados: al destruirlos sueltan un power-up
			// (lo suelta Laser.cs, igual que con los enemigos de la horda)
			if (GD.Randf() < probabilidadPowerUp)
			{
				alien.MarcarComoPortador();
			}

			float offsetX = (i - (cantidad - 1) / 2.0f) * 90.0f;
			Vector2 posInicial = GlobalPosition + new Vector2(offsetX, 100);
			alien.Position = posInicial;

			GetTree().CurrentScene.AddChild(alien);

			esbirros.Add(new DatosEsbirro
			{
				nodo = alien,
				xBase = posInicial.X,
				y = posInicial.Y,
				t = 0.0f,
				fase = GD.Randf() * Mathf.Pi * 2.0f
			});
		}
	}

	private void MoverEsbirros(float dt)
	{
		for (int i = esbirros.Count - 1; i >= 0; i--)
		{
			DatosEsbirro e = esbirros[i];

			if (!EsbirroVivo(e))
			{
				esbirros.RemoveAt(i);
				continue;
			}

			e.t += dt;
			e.y += velocidadCaidaEsbirro * dt;

			float x = e.xBase + Mathf.Sin(e.t * 2.0f + e.fase) * 60.0f;
			x = Mathf.Clamp(x, 30.0f, tamanoPantalla.X - 30.0f);
			e.nodo.GlobalPosition = new Vector2(x, e.y);

			// Si el esbirro choca con el jugador: le saca una vida y el esbirro desaparece
			if (TocaAlJugador(e.nodo))
			{
				if (GameManager.instance != null)
				{
					GameManager.instance.perderVida();
				}
				e.nodo.QueueFree();
				esbirros.RemoveAt(i);
				continue;
			}

			// Si se va por abajo de la pantalla, lo sacamos
			if (e.y > tamanoPantalla.Y + 40.0f)
			{
				e.nodo.QueueFree();
				esbirros.RemoveAt(i);
			}
		}
	}

	private bool TocaAlJugador(Enemigo alien)
	{
		// Mira qué cuerpos están pisando el área del esbirro y busca al jugador
		foreach (Node2D cuerpo in alien.GetOverlappingBodies())
		{
			if (cuerpo is Player) return true;
		}
		return false;
	}

	private bool EsbirroVivo(DatosEsbirro e)
	{
		return e.nodo != null && IsInstanceValid(e.nodo) && !e.nodo.IsQueuedForDeletion();
	}

	// Lo llama Laser.cs cuando un láser del jugador toca al boss
	public void RecibirDanio(int cantidad = 1)
	{
		if (estado != Estado.Peleando) return;

		vida = Mathf.Max(0, vida - cantidad);
		if (barraVida != null) barraVida.Value = vida;

		Parpadear();

		if (vida <= 0)
		{
			Morir();
			return;
		}

		if (!fase2 && vida <= vidaMaxima / 2)
		{
			EntrarEnFase2();
		}
	}

	private void EntrarEnFase2()
	{
		fase2 = true;
		tiempoEsbirros = 0.0f; // suelta una oleada enseguida
	}

	private void Parpadear()
	{
		Tween tween = CreateTween();
		tween.TweenProperty(this, "modulate", new Color(1.0f, 0.3f, 0.3f), 0.05f);
		tween.TweenProperty(this, "modulate", Colors.White, 0.1f);
	}

	private void Morir()
	{
		estado = Estado.Muriendo;

		// Se van con él todos los esbirros que quedaban
		foreach (DatosEsbirro e in esbirros)
		{
			if (EsbirroVivo(e)) e.nodo.QueueFree();
		}
		esbirros.Clear();

		// Que no siga recibiendo colisiones mientras se desvanece
		SetDeferred("monitoring", false);
		SetDeferred("monitorable", false);

		if (GameManager.instance != null)
		{
			GameManager.instance.sumarPuntos(puntosAlMorir);
		}

		// Animación simple: se agranda y se desvanece. Después, nivel completado.
		Tween tween = CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(this, "modulate", new Color(1, 1, 1, 0), 1.5f);
		tween.TweenProperty(this, "scale", Scale * 1.3f, 1.5f);
		tween.Chain().TweenCallback(Callable.From(TerminarNivel));
	}

	private void TerminarNivel()
	{
		if (GameManager.instance != null)
		{
			GameManager.instance.pasarANivelCompletado();
		}
		else
		{
			GetTree().ChangeSceneToFile("res://Escenas/Win.tscn");
		}
	}

	private void CrearBarraDeVida()
	{
		// Va en su propio CanvasLayer para que no dependa de Interfaz.tscn ni se mueva con el boss
		CanvasLayer capa = new CanvasLayer();
		AddChild(capa);

		float ancho = 300.0f;

		barraVida = new ProgressBar
		{
			MinValue = 0,
			MaxValue = vidaMaxima,
			Value = vidaMaxima,
			ShowPercentage = false,
			CustomMinimumSize = new Vector2(ancho, 16)
		};
		barraVida.Position = new Vector2((tamanoPantalla.X - ancho) / 2.0f, 10);
		barraVida.AddThemeStyleboxOverride("fill", new StyleBoxFlat { BgColor = new Color(0.9f, 0.15f, 0.15f) });

		capa.AddChild(barraVida);
	}
}
