using Godot;

public partial class GameManager : Node
{
	// Instancia estática para poder consultar datos desde cualquier script fácilmente (como un GameManager global)
	public static GameManager instance;
	public bool escudoActivo = false;

	[Export] public int puntos = 0;
	[Export] public int vidas = 3;
	public int nivelActual = 1;
	public int puntajeFinalDerrota;
	private CanvasLayer capaEfectos;
	private Texture2D texturaParticula;

	public override void _Ready()
	{
		instance = this;
		capaEfectos = new CanvasLayer { Name = "Efectos", Layer = 10 };
		AddChild(capaEfectos);
		Image imagen = Image.CreateEmpty(4, 4, false, Image.Format.Rgba8);
		imagen.Fill(Colors.White);
		texturaParticula = ImageTexture.CreateFromImage(imagen);
	}

	// La capa global permite que la explosión siga visible al cambiar de nivel.
	public void crearExplosion(Vector2 posicion, Color color, bool grande = false)
	{
		var degradado = new Gradient
		{
			Colors = new[] { Colors.White, new Color(1, 1, 1, 0) },
			Offsets = new[] { 0.0f, 1.0f }
		};
		var particulas = new CpuParticles2D
		{
			Name = "Explosion",
			Texture = texturaParticula,
			Amount = grande ? 70 : 24,
			Lifetime = grande ? 1.1 : 0.65,
			OneShot = true,
			Explosiveness = 1.0f,
			Direction = Vector2.Up,
			Spread = 180.0f,
			InitialVelocityMin = grande ? 80.0f : 45.0f,
			InitialVelocityMax = grande ? 240.0f : 150.0f,
			Gravity = new Vector2(0, 70),
			ScaleAmountMin = grande ? 1.0f : 0.7f,
			ScaleAmountMax = grande ? 2.6f : 1.7f,
			Color = color,
			ColorRamp = degradado,
			Emitting = false
		};
		capaEfectos.AddChild(particulas);
		particulas.GlobalPosition = posicion;
		particulas.Finished += particulas.QueueFree;
		particulas.Emitting = true;
	}

	// Tripulantes decorativos: no detectan impactos ni cambian el puntaje.
	public void crearTripulantes(Vector2 posicion, Color color, bool nodriza = false)
	{
		int cantidad = nodriza ? 28 : 4;
		for (int i = 0; i < cantidad; i++)
		{
			var tripulante = new Tripulante
			{
				Name = "Tripulante",
				ColorPiel = color,
				Velocidad = new Vector2((float)GD.RandRange(-95.0, 95.0), (float)GD.RandRange(-105.0, -30.0)),
				Balanceo = (float)GD.RandRange(0.0, Mathf.Tau)
			};
			capaEfectos.AddChild(tripulante);
			tripulante.AddToGroup("TripulantesEfecto");
			float radio = nodriza ? 70.0f : 12.0f;
			tripulante.GlobalPosition = posicion + new Vector2((float)GD.RandRange(-radio, radio), (float)GD.RandRange(-10.0, 10.0));
		}
	}

	// Al escapar por abajo no se otorgan puntos: primero huyen, luego estalla la nave.
	public void crearHuidaYExplosion(Enemigo nave, Vector2 posicion, Color color)
	{
		for (int i = 0; i < 4; i++)
		{
			float sentido = i < 2 ? -1 : 1;
			var tripulante = new Tripulante
			{
				Name = "TripulanteHuyendo", ColorPiel = color, Huyendo = true,
				Velocidad = new Vector2(sentido * (60 + i * 12), 0)
			};
			capaEfectos.AddChild(tripulante);
			tripulante.AddToGroup("TripulantesEfecto");
			tripulante.GlobalPosition = posicion + new Vector2((i - 1.5f) * 5, 12);
		}
		GetTree().CreateTimer(0.55, false).Timeout += () =>
		{
			crearExplosion(posicion, color);
			reproducirSonido("explosion");
			if (GodotObject.IsInstanceValid(nave)) nave.QueueFree();
		};
	}

	// Texto flotante en la misma capa global que la explosión.
	public void mostrarPuntos(Vector2 posicion, int cantidad, Color color)
	{
		var texto = new Label
		{
			Name = "PuntosFlotantes",
			Text = "+" + cantidad,
			HorizontalAlignment = HorizontalAlignment.Center,
			MouseFilter = Control.MouseFilterEnum.Ignore,
			Size = new Vector2(120, 34)
		};
		texto.AddThemeFontSizeOverride("font_size", 24);
		texto.AddThemeColorOverride("font_color", color);
		texto.AddThemeColorOverride("font_outline_color", Colors.Black);
		texto.AddThemeConstantOverride("outline_size", 3);
		capaEfectos.AddChild(texto);
		Vector2 pantalla = GetViewport().GetVisibleRect().Size;
		texto.Position = new Vector2(
			Mathf.Clamp(posicion.X - 60, 10, Mathf.Max(10, pantalla.X - 130)),
			Mathf.Clamp(posicion.Y - 30, 40, Mathf.Max(40, pantalla.Y - 50)));
		Tween animacion = texto.CreateTween().SetParallel(true);
		animacion.TweenProperty(texto, "position:y", texto.Position.Y - 40, 0.9);
		animacion.TweenProperty(texto, "modulate:a", 0.0, 0.9).SetDelay(0.25);
		animacion.Chain().TweenCallback(Callable.From(texto.QueueFree));
	}

	public void reproducirSonido(string nombre)
	{
		GetNodeOrNull<Node>("/root/Sonidos")?.Call("reproducir", nombre);
	}

	// Sube el puntaje cuando matamos un alien
	public void sumarPuntos(int cantidad)
	{
		puntos += cantidad;
		GD.Print("Puntos actuales: " + puntos);
	}

	// Resta vida si nos pegan o si los aliens se pasan de la raya abajo
	public void perderVida()
	{
		if (escudoActivo)
		{
			escudoActivo = false; // El escudo absorbe el golpe y se rompe
			GD.Print("¡El escudo absorbió el impacto!");
			return;
		}
		if (vidas <= 0) return;
		vidas--;
		// Guardar el puntaje antes del último impacto para mostrarlo en Game Over.
		if (vidas <= 0) puntajeFinalDerrota = puntos;
		puntos = 0;
		reproducirSonido("danio");
		if (vidas <= 0)
		{
			// Si nos quedamos sin vidas, mandamos a la pantalla de Game Over
			GetTree().ChangeSceneToFile("res://Escenas/GameOverScreen.tscn");
		}
	}

	// Función para avanzar al siguiente nivel según el número actual
	public void siguienteNivel()
	{
		nivelActual++;
		
		if (nivelActual == 2)
		{
			GetTree().ChangeSceneToFile("res://Escenas/Lvl2.tscn");
		}
		else if (nivelActual == 3)
		{
			GetTree().ChangeSceneToFile("res://Escenas/Lvl3.tscn");
		}
		else
		{
			GetTree().ChangeSceneToFile("res://Escenas/Boss.tscn");
		}
	}
	
	// Pantalla intermedia cuando pasamos un nivel
	public void pasarANivelCompletado()
	{
		reproducirSonido("victoria");
		GetTree().ChangeSceneToFile("res://Escenas/NivelCompletado.tscn");
	}
}
