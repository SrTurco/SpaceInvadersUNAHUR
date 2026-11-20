using Godot;

public partial class PauseMenu : CanvasLayer
{
	// Panel principal que contiene todo el menú de pausa
	private Panel panelFondo;
	private bool pausaActiva = false;
	private Button botonContinuar;

	// Nombres de escenas donde NO queremos que aparezca el menú de pausa
	private readonly string[] escenasExcluidas = { "MainMenu", "Tutorial" };

	public override void _Ready()
	{
		// Nos ponemos en una capa alta para que siempre se vea encima de todo
		Layer = 100;

		// Creamos toda la interfaz por código para no depender de nodos manuales
		CrearInterfaz();

		// Arrancamos con el menú oculto
		panelFondo.Visible = false;

		// No procesamos la pausa cuando el juego esté pausado (queremos seguir escuchando input)
		ProcessMode = ProcessModeEnum.Always;
	}

	public override void _Input(InputEvent @event)
	{
		// Detectamos la tecla ESC
		if (@event is InputEventKey eventKey && eventKey.Pressed && !eventKey.Echo && eventKey.Keycode == Key.Escape)
		{
			// Verificamos que no estemos en una escena excluida (menú o tutorial)
			if (EstaEnEscenaExcluida())
				return;
			if (GetTree().CurrentScene is NivelBoss nivel && nivel.IntroduccionActiva)
				return;

			TogglePausa();
		}
	}

	private void TogglePausa()
	{
		pausaActiva = !pausaActiva;
		panelFondo.Visible = pausaActiva;
		GetTree().Paused = pausaActiva;
		if (pausaActiva) botonContinuar.GrabFocus();
	}

	private bool EstaEnEscenaExcluida()
	{
		// Obtenemos el nombre del nodo raíz de la escena actual
		Node escenaActual = GetTree().CurrentScene;
		if (escenaActual == null)
			return true;

		string nombreEscena = escenaActual.Name;

		foreach (string excluida in escenasExcluidas)
		{
			if (nombreEscena.Contains(excluida))
				return true;
		}

		return false;
	}

	private void CrearInterfaz()
	{
		// El velo deja ver la partida sin competir con los controles del panel.
		panelFondo = new Panel { Name = "Fondo" };
		panelFondo.AddThemeStyleboxOverride("panel", new StyleBoxFlat
		{
			BgColor = new Color(0.0f, 0.0f, 0.0f, 0.82f)
		});
		AddChild(panelFondo);
		panelFondo.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

		var centro = new CenterContainer { Name = "Centro" };
		panelFondo.AddChild(centro);
		centro.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

		var tarjeta = new PanelContainer { Name = "PanelArcade" };
		tarjeta.CustomMinimumSize = new Vector2(440, 0);
		var marco = new StyleBoxFlat { BgColor = new Color("071719"), BorderColor = new Color("28e2d4") };
		marco.SetBorderWidthAll(3);
		marco.SetContentMarginAll(28);
		tarjeta.AddThemeStyleboxOverride("panel", marco);
		centro.AddChild(tarjeta);

		var contenido = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
		contenido.AddThemeConstantOverride("separation", 14);
		tarjeta.AddChild(contenido);

		var etiqueta = CrearEtiqueta("SPACE INVADERS  //  PAUSA", 16, new Color("78c9bd"));
		contenido.AddChild(etiqueta);
		var titulo = CrearEtiqueta("JUEGO EN PAUSA", 38, new Color("d5fff1"));
		contenido.AddChild(titulo);
		var separador = new HSeparator();
		contenido.AddChild(separador);

		botonContinuar = CrearBoton("CONTINUAR", new Color("6ceba5"));
		botonContinuar.Name = "Continuar";
		botonContinuar.Pressed += AlPresionarRegresar;
		contenido.AddChild(botonContinuar);

		var botonJefe = CrearBoton("PROBAR JEFE FINAL", new Color("8bd8ff"));
		botonJefe.Name = "ProbarJefe";
		botonJefe.Pressed += AlPresionarProbarJefe;
		contenido.AddChild(botonJefe);

		var botonMenu = CrearBoton("VOLVER AL MENU", new Color("ffcf62"));
		botonMenu.Name = "VolverAlMenu";
		botonMenu.Pressed += AlPresionarVolverAlMenu;
		contenido.AddChild(botonMenu);

		var botonSalir = CrearBoton("SALIR DEL JUEGO", new Color("ff7290"));
		botonSalir.Name = "Salir";
		botonSalir.Pressed += AlPresionarSalir;
		contenido.AddChild(botonSalir);

		var ayuda = CrearEtiqueta("ESC PARA CONTINUAR", 16, new Color("78c9bd"));
		contenido.AddChild(ayuda);
	}

	private static Label CrearEtiqueta(string texto, int tamano, Color color)
	{
		var etiqueta = new Label { Text = texto, HorizontalAlignment = HorizontalAlignment.Center };
		etiqueta.AddThemeFontSizeOverride("font_size", tamano);
		etiqueta.AddThemeColorOverride("font_color", color);
		return etiqueta;
	}

	private static Button CrearBoton(string texto, Color acento)
	{
		var boton = new Button { Text = texto, CustomMinimumSize = new Vector2(350, 52) };
		boton.AddThemeFontSizeOverride("font_size", 24);
		boton.AddThemeColorOverride("font_color", acento);
		boton.AddThemeColorOverride("font_hover_color", new Color("071719"));
		boton.AddThemeColorOverride("font_pressed_color", new Color("071719"));
		boton.AddThemeColorOverride("font_focus_color", acento);

		var normal = new StyleBoxFlat { BgColor = new Color("102526"), BorderColor = acento };
		normal.SetBorderWidthAll(2);
		normal.SetContentMarginAll(8);
		boton.AddThemeStyleboxOverride("normal", normal);

		var hover = new StyleBoxFlat { BgColor = acento, BorderColor = acento };
		hover.SetBorderWidthAll(2);
		hover.SetContentMarginAll(8);
		boton.AddThemeStyleboxOverride("hover", hover);

		var presionado = new StyleBoxFlat { BgColor = acento.Darkened(0.22f), BorderColor = Colors.White };
		presionado.SetBorderWidthAll(2);
		presionado.SetContentMarginAll(8);
		boton.AddThemeStyleboxOverride("pressed", presionado);

		// El marco exterior señala el botón seleccionado con teclado o mando.
		var foco = new StyleBoxFlat { DrawCenter = false, BorderColor = Colors.White };
		foco.SetBorderWidthAll(2);
		foco.ExpandMarginLeft = 4;
		foco.ExpandMarginRight = 4;
		foco.ExpandMarginTop = 4;
		foco.ExpandMarginBottom = 4;
		boton.AddThemeStyleboxOverride("focus", foco);
		return boton;
	}

	// --- Acciones de los botones ---

	private void AlPresionarRegresar()
	{
		// Despausa el juego y oculta el menú
		TogglePausa();
	}

	private void AlPresionarProbarJefe()
	{
		// Inicia el combate con un estado limpio para poder probarlo varias veces.
		pausaActiva = false;
		panelFondo.Visible = false;
		GetTree().Paused = false;
		if (GameManager.instance != null)
		{
			GameManager.instance.vidas = 3;
			GameManager.instance.puntos = 0;
			GameManager.instance.nivelActual = 4;
		}
		GetTree().ChangeSceneToFile("res://Escenas/Boss.tscn");
	}

	private void AlPresionarVolverAlMenu()
	{
		// Despausa el juego antes de cambiar de escena
		pausaActiva = false;
		panelFondo.Visible = false;
		GetTree().Paused = false;

		// Reiniciamos los datos del GameManager
		if (GameManager.instance != null)
		{
			GameManager.instance.vidas = 3;
			GameManager.instance.puntos = 0;
			GameManager.instance.nivelActual = 1;
		}

		GetTree().ChangeSceneToFile("res://Escenas/MainMenu.tscn");
	}

	private void AlPresionarSalir()
	{
		// Cierra la aplicación
		GetTree().Quit();
	}
}
