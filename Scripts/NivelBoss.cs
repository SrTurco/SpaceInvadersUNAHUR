using Godot;
using System.Collections.Generic;

// Encuentro de la nodriza: los huecos de la órbita se conservan al perder escoltas.
public partial class NivelBoss : Node2D
{
	private enum Fase { Introduccion, Orbita, Dispersion, Alinear, Cargar, Rayo, Regresar }
	[Export] public PackedScene alienEscena;
	[Export] public PackedScene laserEnemigoEscena;
	[Export] public int cantidadEscoltas = 10;
	[Export] public float duracionIntroduccion = 2.5f;
	[Export] public float velocidadOrbita = 2.1f;
	[Export] public float limiteDescenso = 430.0f;
	[Export] public int vueltasPorTanda = 3;
	[Export] public int tandasPorCiclo = 3;
	[Export] public float tiempoCarga = 2.0f;
	[Export] public float duracionRayo = 1.0f;
	[Export] public float intervaloRegeneracion = 15.0f;
	[Export] public float velocidadEntrada = 320.0f;

	private readonly List<Enemigo> escoltas = new();
	private readonly List<int> posicionesOrbita = new();
	private Boss jefe;
	private Player jugador;
	private Node2D formacion;
	private Label estado;
	private ProgressBar barraVida;
	private Fase fase = Fase.Introduccion;
	public bool IntroduccionActiva => fase == Fase.Introduccion;
	private float angulo;
	private float anguloTanda;
	private float tiempoFase;
	private float tiempoDisparo;
	private float tiempoMovimiento;
	private float tiempoHastaRegenerar;
	private float objetivoRayo;
	private int indiceDisparo;
	private int avisosIntroduccion;
	private int tandas;
	private bool rayoImpacto;
	private bool terminado;
	private Vector2 pantalla;
	private Vector2 centroJefe;
	private readonly RandomNumberGenerator azar = new();

	public override void _Ready()
	{
		pantalla = GetViewportRect().Size;
		centroJefe = new Vector2(pantalla.X / 2.0f, 170.0f);
		limiteDescenso = Mathf.Min(limiteDescenso, pantalla.Y - 200.0f);
		jefe = GetNode<Boss>("Jefe");
		jugador = GetNode<Player>("Player");
		formacion = GetNode<Node2D>("Formacion");
		jefe.Position = new Vector2(centroJefe.X, -100.0f);
		jugador.SetPhysicsProcess(false);
		azar.Randomize();
		tiempoHastaRegenerar = intervaloRegeneracion;
		CrearIndicadores();
		estado.Text = "ALERTA  //  NODRIZA APROXIMANDOSE";
		estado.Position = new Vector2(pantalla.X / 2 - 190, 300);
		barraVida.Visible = false;
		GameManager.instance?.reproducirSonido("aproximacion");
		avisosIntroduccion = 1;
	}

	private void CrearEscoltas()
	{
		for (int i = 0; i < cantidadEscoltas; i++)
		{
			posicionesOrbita.Add(i);
			escoltas.Add(CrearEscolta(i));
		}
	}

	private Enemigo CrearEscolta(int indice)
	{
		Enemigo alien = alienEscena.Instantiate<Enemigo>();
		alien.tipo = indice % 3 == 0 ? TipoEnemigo.Tirador : indice % 3 == 1 ? TipoEnemigo.ZigZag : TipoEnemigo.Normal;
		int posicion = posicionesOrbita[indice];
		float faseInicial = angulo + Mathf.Tau * posicion / Mathf.Max(1, cantidadEscoltas);
		Vector2 destino = jefe.Position + new Vector2(Mathf.Cos(faseInicial) * 180, Mathf.Sin(faseInicial) * 105);
		destino.X = Mathf.Clamp(destino.X, 30, pantalla.X - 30);
		destino.Y = Mathf.Clamp(destino.Y, 75, limiteDescenso);
		// Entra desde el lateral más cercano, completamente fuera de la cámara.
		Vector2 entrada = new Vector2(destino.X < pantalla.X / 2 ? -48 : pantalla.X + 48, destino.Y);
		alien.Position = formacion.ToLocal(entrada);
		formacion.AddChild(alien);
		alien.SetProcess(false); // El encuentro dirige sus movimientos, no la horda normal.
		return alien;
	}

	private void RegenerarEscoltas()
	{
		bool huboRegeneracion = false;
		for (int i = 0; i < escoltas.Count; i++)
		{
			if (Vivo(escoltas[i])) continue;
			escoltas[i] = CrearEscolta(i);
			huboRegeneracion = true;
		}
		if (huboRegeneracion) GameManager.instance?.reproducirSonido("regenerar");
	}

	private void CrearIndicadores()
	{
		var capa = new CanvasLayer { Layer = 2 };
		AddChild(capa);
		estado = new Label { Name = "EstadoNodriza", HorizontalAlignment = HorizontalAlignment.Center,
			Position = new Vector2(pantalla.X / 2 - 190, 20), Size = new Vector2(380, 28),
			MouseFilter = Control.MouseFilterEnum.Ignore };
		estado.AddThemeFontSizeOverride("font_size", 18);
		estado.AddThemeColorOverride("font_color", new Color("ffcf62"));
		capa.AddChild(estado);
		barraVida = new ProgressBar { Name = "VidaNodriza", Position = new Vector2(pantalla.X / 2 - 160, 48),
			Size = new Vector2(320, 12), MaxValue = jefe.vidaMaxima, Value = jefe.VidaActual, ShowPercentage = false,
			MouseFilter = Control.MouseFilterEnum.Ignore };
		var fondo = new StyleBoxFlat { BgColor = new Color("20212a") };
		var relleno = new StyleBoxFlat { BgColor = new Color("ff7290") };
		barraVida.AddThemeStyleboxOverride("background", fondo);
		barraVida.AddThemeStyleboxOverride("fill", relleno);
		capa.AddChild(barraVida);
	}

	public override void _Process(double delta)
	{
		if (terminado) return;
		if (!GodotObject.IsInstanceValid(jefe) || jefe.IsQueuedForDeletion())
		{
			terminado = true;
			GetTree().ChangeSceneToFile("res://Escenas/Win.tscn");
			return;
		}
		if (!GodotObject.IsInstanceValid(jugador) || jugador.IsQueuedForDeletion()) return;
		float dt = (float)delta;
		tiempoFase += dt;
		tiempoMovimiento += dt;
		if (fase == Fase.Introduccion)
		{
			ActualizarIntroduccion();
			QueueRedraw();
			return;
		}
		if (intervaloRegeneracion > 0)
		{
			tiempoHastaRegenerar -= dt;
			// No reaparecen durante los ataques por turnos ni encima del rayo.
			if (tiempoHastaRegenerar <= 0 && fase == Fase.Orbita)
			{
				RegenerarEscoltas();
				tiempoHastaRegenerar = intervaloRegeneracion;
			}
		}
		barraVida.Value = jefe.VidaActual;
		switch (fase)
		{
			case Fase.Orbita: ActualizarOrbita(dt); break;
			case Fase.Dispersion: ActualizarDispersion(dt); break;
			case Fase.Alinear: ActualizarAlineacion(dt); break;
			case Fase.Cargar:
				MoverEscoltasALosLados(dt);
				estado.Text = "¡RAYO MAESTRO!  PREPARANDO";
				if (tiempoFase >= tiempoCarga) CambiarFase(Fase.Rayo);
				break;
			case Fase.Rayo:
				MoverEscoltasALosLados(dt);
				estado.Text = "¡RAYO MAESTRO!";
				if (!rayoImpacto && Mathf.Abs(jugador.GlobalPosition.X - objetivoRayo) < 45.0f)
				{
					rayoImpacto = true;
					GameManager.instance?.crearExplosion(jugador.GlobalPosition, new Color("73dcff"));
					GameManager.instance?.perderVida();
				}
				if (tiempoFase >= duracionRayo) CambiarFase(Fase.Regresar);
				break;
			case Fase.Regresar:
				MoverEscoltasALosLados(dt);
				estado.Text = "NODRIZA  //  REAGRUPANDO";
				jefe.Position = jefe.Position.MoveToward(centroJefe, 300.0f * dt);
				if (jefe.Position.DistanceTo(centroJefe) < 2.0f)
				{
					tandas = 0;
					anguloTanda = 0;
					CambiarFase(Fase.Orbita);
				}
				break;
		}
		QueueRedraw();
	}

	private void ActualizarIntroduccion()
	{
		float duracion = Mathf.Max(0.01f, duracionIntroduccion);
		float progreso = Mathf.Clamp(tiempoFase / duracion, 0, 1);
		float suave = Mathf.SmoothStep(0, 1, progreso);
		jefe.Position = new Vector2(centroJefe.X, Mathf.Lerp(-100.0f, centroJefe.Y, suave));
		while (avisosIntroduccion < 3 && tiempoFase >= duracion * avisosIntroduccion / 3.0f)
		{
			GameManager.instance?.reproducirSonido("aproximacion");
			avisosIntroduccion++;
		}
		if (progreso >= 1)
		{
			jefe.Position = centroJefe;
			estado.Position = new Vector2(pantalla.X / 2 - 190, 20);
			barraVida.Visible = true;
			CrearEscoltas();
			jugador.SetPhysicsProcess(true);
			tiempoMovimiento = 0;
			CambiarFase(Fase.Orbita);
		}
	}

	private bool Vivo(Enemigo alien) => GodotObject.IsInstanceValid(alien) && !alien.IsQueuedForDeletion();
	private int EscoltasVivos()
	{
		int vivos = 0;
		foreach (var alien in escoltas) if (Vivo(alien)) vivos++;
		return vivos;
	}

	private void ActualizarOrbita(float dt)
	{
		estado.Text = "NODRIZA  //  ESCOLTA " + (tandas + 1) + "/" + tandasPorCiclo;
		// Si no quedan escoltas, no se obliga a esperar tres órbitas vacías.
		if (EscoltasVivos() == 0) { CambiarFase(Fase.Alinear); return; }
		float paso = velocidadOrbita * dt;
		angulo += paso;
		anguloTanda += paso;
		float velocidadJefe = 0.5f + jefe.FraccionDanio * 0.8f;
		Vector2 objetivo = new Vector2(centroJefe.X + Mathf.Sin(tiempoMovimiento * velocidadJefe) * 80, centroJefe.Y);
		// No reemplazar la posición: al entrar en esta fase el objetivo puede estar lejos.
		float velocidadDeslizamiento = 160.0f + jefe.FraccionDanio * 160.0f;
		jefe.Position = jefe.Position.MoveToward(objetivo, velocidadDeslizamiento * dt);
		for (int i = 0; i < escoltas.Count; i++)
		{
			var alien = escoltas[i];
			if (!Vivo(alien)) continue;
			float faseAlien = angulo + Mathf.Tau * posicionesOrbita[i] / escoltas.Count;
			Vector2 destino = jefe.Position + new Vector2(Mathf.Cos(faseAlien) * 180, Mathf.Sin(faseAlien) * 105);
			MoverAlien(alien, destino, dt);
		}
		tiempoDisparo -= dt;
		if (tiempoDisparo <= 0)
		{
			var candidatos = new List<Enemigo>();
			foreach (var alien in escoltas)
				if (Vivo(alien) && EstaEnPantalla(alien) && alien.GlobalPosition.Y > jefe.GlobalPosition.Y + 25) candidatos.Add(alien);
			if (candidatos.Count > 0) Disparar(candidatos[azar.RandiRange(0, candidatos.Count - 1)]);
			tiempoDisparo = Mathf.Lerp(1.8f, 1.0f, jefe.FraccionDanio);
		}
		if (anguloTanda >= Mathf.Tau * vueltasPorTanda)
		{
			indiceDisparo = 0;
			CambiarFase(Fase.Dispersion);
		}
	}

	private void ActualizarDispersion(float dt)
	{
		estado.Text = "ESCOLTA  //  ATAQUE POR TURNOS";
		jefe.Position = jefe.Position.MoveToward(centroJefe, 180 * dt);
		for (int i = 0; i < escoltas.Count; i++)
		{
			if (!Vivo(escoltas[i])) continue;
			float x = Mathf.Clamp(jugador.GlobalPosition.X + (i - (escoltas.Count - 1) / 2.0f) * 65, 40, pantalla.X - 40);
			float y = Mathf.Min(centroJefe.Y + 140 + (i % 2) * 30, limiteDescenso);
			MoverAlien(escoltas[i], new Vector2(x, y), dt);
		}
		// Se deja tiempo para alcanzar la nueva posición antes de la primera bala.
		if (tiempoFase < 1.2f) return;
		tiempoDisparo -= dt;
		if (tiempoDisparo > 0) return;
		while (indiceDisparo < escoltas.Count && !Vivo(escoltas[indiceDisparo])) indiceDisparo++;
		if (indiceDisparo < escoltas.Count)
		{
			Disparar(escoltas[indiceDisparo++]);
			tiempoDisparo = 0.42f;
		}
		else
		{
			tandas++;
			anguloTanda = 0;
			if (tandas >= tandasPorCiclo || EscoltasVivos() == 0) CambiarFase(Fase.Alinear);
			else
			{
				// Se reordenan los lugares; la regeneración tiene su propio temporizador.
				for (int i = posicionesOrbita.Count - 1; i > 0; i--)
				{
					int j = azar.RandiRange(0, i);
					(posicionesOrbita[i], posicionesOrbita[j]) = (posicionesOrbita[j], posicionesOrbita[i]);
				}
				angulo += azar.RandfRange(0, Mathf.Tau);
				CambiarFase(Fase.Orbita);
			}
		}
	}

	private void ActualizarAlineacion(float dt)
	{
		estado.Text = "NODRIZA  //  FIJANDO OBJETIVO";
		MoverEscoltasALosLados(dt);
		jefe.Position = jefe.Position.MoveToward(new Vector2(objetivoRayo, centroJefe.Y), 300 * dt);
		if (Mathf.Abs(jefe.Position.X - objetivoRayo) < 4)
		{
			objetivoRayo = jefe.GlobalPosition.X; // Se fija durante la carga para permitir esquivarlo.
			CambiarFase(Fase.Cargar);
		}
	}

	private void MoverEscoltasALosLados(float dt)
	{
		float xRayo = fase == Fase.Alinear ? jefe.GlobalPosition.X : objetivoRayo;
		for (int i = 0; i < escoltas.Count; i++)
		{
			if (!Vivo(escoltas[i])) continue;
			float lado = i % 2 == 0 ? -1 : 1;
			float x = Mathf.Clamp(xRayo + lado * (140 + (i / 2) * 45), 30, pantalla.X - 30);
			MoverAlien(escoltas[i], new Vector2(x, Mathf.Min(centroJefe.Y + 145 + (i % 3) * 22, limiteDescenso)), dt);
		}
	}

	private bool EstaEnPantalla(Enemigo alien)
	{
		return alien.GlobalPosition.X >= 25 && alien.GlobalPosition.X <= pantalla.X - 25;
	}

	private void MoverAlien(Enemigo alien, Vector2 destino, float dt)
	{
		destino.X = Mathf.Clamp(destino.X, 30, pantalla.X - 30);
		destino.Y = Mathf.Clamp(destino.Y, 75, limiteDescenso);
		float velocidad = EstaEnPantalla(alien) ? 500.0f : velocidadEntrada;
		alien.GlobalPosition = alien.GlobalPosition.MoveToward(destino, velocidad * dt);
		// El límite también se cumple durante las transiciones entre formaciones.
		var pos = alien.GlobalPosition;
		pos.Y = Mathf.Min(pos.Y, limiteDescenso);
		alien.GlobalPosition = pos;
		foreach (Node hijo in alien.GetChildren())
			if (hijo is Sprite2D sprite && sprite.Visible)
			{
				bool pose = ((int)(tiempoMovimiento * 4) % 2) == 0;
				sprite.Scale = pose ? new Vector2(1.08f, 0.92f) : new Vector2(0.92f, 1.08f);
				sprite.Position = new Vector2(0, pose ? -2 : 0);
			}
	}

	private void Disparar(Enemigo alien)
	{
		if (laserEnemigoEscena == null || !Vivo(alien) || !EstaEnPantalla(alien)) return;
		var laser = laserEnemigoEscena.Instantiate<LaserEnemigo>();
		GetTree().CurrentScene.AddChild(laser);
		laser.GlobalPosition = alien.GlobalPosition + new Vector2(0, 30);
		laser.ApuntarA(jugador.GlobalPosition);
		GameManager.instance?.reproducirSonido("disparo_enemigo");
	}

	private void CambiarFase(Fase nueva)
	{
		fase = nueva;
		tiempoFase = 0;
		tiempoDisparo = nueva == Fase.Dispersion ? 0 : 1.0f;
		if (nueva == Fase.Alinear) objetivoRayo = Mathf.Clamp(jugador.GlobalPosition.X, 110, pantalla.X - 110);
		if (nueva == Fase.Cargar) GameManager.instance?.reproducirSonido("carga");
		if (nueva == Fase.Rayo) { rayoImpacto = false; GameManager.instance?.reproducirSonido("rayo"); }
	}

	public override void _Draw()
	{
		if (fase == Fase.Introduccion) return;
		// Línea de seguridad: los escoltas nunca cruzan hacia la zona de la nave.
		for (float x = 24; x < pantalla.X - 24; x += 24)
			DrawLine(new Vector2(x, limiteDescenso + 22), new Vector2(x + 10, limiteDescenso + 22), new Color(0.2f, 0.6f, 0.55f, 0.32f), 2);
		// Cada escolta aporta su propio campo; al morir deja un hueco visible.
		if (fase == Fase.Orbita)
			foreach (var alien in escoltas)
				if (Vivo(alien))
					DrawArc(ToLocal(alien.GlobalPosition), 27, 0, Mathf.Tau, 20, new Color(0.20f, 0.90f, 0.80f, 0.5f), 2);
		if (fase != Fase.Cargar && fase != Fase.Rayo) return;
		float inicio = jefe.GlobalPosition.Y + 50;
		float alto = pantalla.Y - inicio;
		if (fase == Fase.Cargar)
		{
			float pulso = 0.15f + 0.15f * Mathf.Abs(Mathf.Sin(tiempoFase * 8));
			DrawRect(new Rect2(objetivoRayo - 45, inicio, 90, alto), new Color(1, 0.3f, 0.35f, pulso));
			DrawLine(new Vector2(objetivoRayo - 45, inicio), new Vector2(objetivoRayo - 45, pantalla.Y), new Color("ff7290"), 2);
			DrawLine(new Vector2(objetivoRayo + 45, inicio), new Vector2(objetivoRayo + 45, pantalla.Y), new Color("ff7290"), 2);
		}
		else
		{
			DrawRect(new Rect2(objetivoRayo - 45, inicio, 90, alto), new Color(1, 0.25f, 0.3f, 0.8f));
			DrawRect(new Rect2(objetivoRayo - 20, inicio, 40, alto), new Color("fff3bd"));
			DrawRect(new Rect2(objetivoRayo - 5, inicio, 10, alto), Colors.White);
		}
	}
}
