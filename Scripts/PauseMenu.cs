using Godot;

public partial class PauseMenu : CanvasLayer
{
	// Panel principal que contiene todo el menú de pausa
	private Panel panelFondo;
	private bool pausaActiva = false;

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

			TogglePausa();
		}
	}

	private void TogglePausa()
	{
		pausaActiva = !pausaActiva;
		panelFondo.Visible = pausaActiva;
		GetTree().Paused = pausaActiva;
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
		// --- Panel de fondo semitransparente que cubre toda la pantalla ---
		panelFondo = new Panel();
		panelFondo.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

		// Estilo del panel: fondo oscuro semitransparente
		StyleBoxFlat estiloPanelFondo = new StyleBoxFlat();
		estiloPanelFondo.BgColor = new Color(0, 0, 0, 0.7f);
		panelFondo.AddThemeStyleboxOverride("panel", estiloPanelFondo);
		AddChild(panelFondo);

		// --- Contenedor vertical centrado para organizar todo ---
		VBoxContainer contenedor = new VBoxContainer();
		contenedor.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
		contenedor.GrowHorizontal = Control.GrowDirection.Both;
		contenedor.GrowVertical = Control.GrowDirection.Both;
		contenedor.CustomMinimumSize = new Vector2(400, 350);
		contenedor.OffsetLeft = -200;
		contenedor.OffsetTop = -175;
		contenedor.OffsetRight = 200;
		contenedor.OffsetBottom = 175;
		contenedor.Alignment = BoxContainer.AlignmentMode.Center;

		// Separación entre los elementos del menú
		contenedor.AddThemeConstantOverride("separation", 30);
		panelFondo.AddChild(contenedor);

		// --- Título "PAUSA" ---
		Label tituloPausa = new Label();
		tituloPausa.Text = "PAUSA";
		tituloPausa.HorizontalAlignment = HorizontalAlignment.Center;

		LabelSettings estiloTitulo = new LabelSettings();
		estiloTitulo.FontSize = 64;
		estiloTitulo.FontColor = Colors.White;
		tituloPausa.LabelSettings = estiloTitulo;
		contenedor.AddChild(tituloPausa);

		// --- Botón: Regresar Al Juego ---
		Button botonRegresar = CrearBoton("Regresar Al Juego");
		botonRegresar.Pressed += AlPresionarRegresar;
		contenedor.AddChild(botonRegresar);

		// --- Botón: Volver Al Menú ---
		Button botonMenu = CrearBoton("Volver Al Menu");
		botonMenu.Pressed += AlPresionarVolverAlMenu;
		contenedor.AddChild(botonMenu);

		// --- Botón: Salir del Juego ---
		Button botonSalir = CrearBoton("Salir del Juego");
		botonSalir.Pressed += AlPresionarSalir;
		contenedor.AddChild(botonSalir);
	}

	private Button CrearBoton(string texto)
	{
		Button boton = new Button();
		boton.Text = texto;
		boton.CustomMinimumSize = new Vector2(300, 50);

		// Estilo normal del botón
		StyleBoxFlat estiloNormal = new StyleBoxFlat();
		estiloNormal.BgColor = new Color(0.15f, 0.15f, 0.3f, 0.9f);
		estiloNormal.SetCornerRadiusAll(8);
		estiloNormal.SetContentMarginAll(10);
		boton.AddThemeStyleboxOverride("normal", estiloNormal);

		// Estilo cuando se pasa el mouse por encima
		StyleBoxFlat estiloHover = new StyleBoxFlat();
		estiloHover.BgColor = new Color(0.25f, 0.25f, 0.5f, 0.9f);
		estiloHover.SetCornerRadiusAll(8);
		estiloHover.SetContentMarginAll(10);
		boton.AddThemeStyleboxOverride("hover", estiloHover);

		// Estilo cuando se presiona
		StyleBoxFlat estiloPresionado = new StyleBoxFlat();
		estiloPresionado.BgColor = new Color(0.1f, 0.1f, 0.2f, 0.9f);
		estiloPresionado.SetCornerRadiusAll(8);
		estiloPresionado.SetContentMarginAll(10);
		boton.AddThemeStyleboxOverride("pressed", estiloPresionado);

		// Tamaño de fuente para los botones
		boton.AddThemeFontSizeOverride("font_size", 24);

		return boton;
	}

	// --- Acciones de los botones ---

	private void AlPresionarRegresar()
	{
		// Despausa el juego y oculta el menú
		TogglePausa();
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
