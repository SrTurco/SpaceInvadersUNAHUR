using Godot;

public partial class Tutorial : CanvasLayer
{
	private int paginaActual = 0;
	private const int TOTAL_PAGINAS = 2;

	// Contenedores de cada página
	private Control pagina1;
	private Control pagina2;

	// Botones de navegación
	private Button botonSiguiente;
	private Button botonAnterior;
	private Button botonVolver;

	// Label del indicador de página
	private Label indicadorPagina;

	public override void _Ready()
	{
		CrearInterfaz();
		MostrarPagina(0);
	}

	private void CrearInterfaz()
	{
		// --- Fondo general ---
		Panel fondoGeneral = new Panel();
		fondoGeneral.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

		StyleBoxFlat estiloFondo = new StyleBoxFlat();
		estiloFondo.BgColor = new Color(0.05f, 0.05f, 0.12f, 1.0f);
		fondoGeneral.AddThemeStyleboxOverride("panel", estiloFondo);
		AddChild(fondoGeneral);

		// === TÍTULO "TUTORIAL" en la parte superior ===
		Label titulo = new Label();
		titulo.Text = "TUTORIAL";
		titulo.HorizontalAlignment = HorizontalAlignment.Center;
		titulo.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopWide);
		titulo.OffsetTop = 30;
		titulo.OffsetBottom = 100;

		LabelSettings estiloTitulo = new LabelSettings();
		estiloTitulo.FontSize = 52;
		estiloTitulo.FontColor = Colors.White;
		titulo.LabelSettings = estiloTitulo;
		fondoGeneral.AddChild(titulo);

		// === PÁGINA 1: Controles ===
		pagina1 = CrearPagina1();
		fondoGeneral.AddChild(pagina1);

		// === PÁGINA 2: Tipos de enemigos ===
		pagina2 = CrearPagina2();
		fondoGeneral.AddChild(pagina2);

		// === BARRA INFERIOR CON NAVEGACIÓN ===
		HBoxContainer barraInferior = new HBoxContainer();
		barraInferior.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomWide);
		barraInferior.OffsetTop = -80;
		barraInferior.OffsetLeft = 50;
		barraInferior.OffsetRight = -50;
		barraInferior.Alignment = BoxContainer.AlignmentMode.Center;
		barraInferior.AddThemeConstantOverride("separation", 30);
		fondoGeneral.AddChild(barraInferior);

		// Botón Anterior
		botonAnterior = CrearBotonNav("< Anterior");
		botonAnterior.Pressed += () => CambiarPagina(-1);
		barraInferior.AddChild(botonAnterior);

		// Indicador de página
		indicadorPagina = new Label();
		indicadorPagina.HorizontalAlignment = HorizontalAlignment.Center;
		indicadorPagina.CustomMinimumSize = new Vector2(150, 50);
		indicadorPagina.VerticalAlignment = VerticalAlignment.Center;

		LabelSettings estiloIndicador = new LabelSettings();
		estiloIndicador.FontSize = 22;
		estiloIndicador.FontColor = new Color(0.7f, 0.7f, 0.7f);
		indicadorPagina.LabelSettings = estiloIndicador;
		barraInferior.AddChild(indicadorPagina);

		// Botón Siguiente
		botonSiguiente = CrearBotonNav("Siguiente >");
		botonSiguiente.Pressed += () => CambiarPagina(1);
		barraInferior.AddChild(botonSiguiente);

		// Botón Volver al Menú (separado, a la derecha)
		Control espaciador = new Control();
		espaciador.CustomMinimumSize = new Vector2(80, 0);
		barraInferior.AddChild(espaciador);

		botonVolver = CrearBotonNav("Volver al Menu");
		botonVolver.Pressed += VolverAlMenu;
		barraInferior.AddChild(botonVolver);
	}

	// ============================================================
	//  PÁGINA 1 — CONTROLES
	// ============================================================
	private Control CrearPagina1()
	{
		VBoxContainer contenedor = CrearContenedorPagina();

		// Subtítulo
		Label subtitulo = CrearSubtitulo("CONTROLES");
		contenedor.AddChild(subtitulo);

		// Separador
		contenedor.AddChild(CrearSeparador());

		// === MOVIMIENTO ===
		VBoxContainer bloqueMovimiento = new VBoxContainer();
		bloqueMovimiento.AddThemeConstantOverride("separation", 10);
		bloqueMovimiento.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		bloqueMovimiento.CustomMinimumSize = new Vector2(450, 0);
		contenedor.AddChild(bloqueMovimiento);

		// Teclas de flechas centradas
		HBoxContainer flechasContainer = new HBoxContainer();
		flechasContainer.AddThemeConstantOverride("separation", 15);
		flechasContainer.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		bloqueMovimiento.AddChild(flechasContainer);

		Panel teclaIzq = CrearTeclaVisual("<--");
		flechasContainer.AddChild(teclaIzq);

		Panel teclaDer = CrearTeclaVisual("-->");
		flechasContainer.AddChild(teclaDer);

		// Título y descripción centrados
		Label tituloMov = CrearTextoDestacado("Flechas Izquierda / Derecha");
		tituloMov.HorizontalAlignment = HorizontalAlignment.Center;
		bloqueMovimiento.AddChild(tituloMov);

		Label descMov = CrearTextoNormal("Mueven al jugador a los costados de la pantalla.");
		descMov.HorizontalAlignment = HorizontalAlignment.Center;
		bloqueMovimiento.AddChild(descMov);

		// Espacio entre secciones
		contenedor.AddChild(CrearEspaciador(30));

		// === DISPARO ===
		VBoxContainer bloqueDisparo = new VBoxContainer();
		bloqueDisparo.AddThemeConstantOverride("separation", 10);
		bloqueDisparo.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		bloqueDisparo.CustomMinimumSize = new Vector2(450, 0);
		contenedor.AddChild(bloqueDisparo);

		// Tecla espacio centrada
		Panel teclaEspacio = CrearTeclaVisual("ESPACIO");
		teclaEspacio.CustomMinimumSize = new Vector2(90, 30);
		teclaEspacio.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		bloqueDisparo.AddChild(teclaEspacio);

		// Título y descripción centrados
		Label tituloDisp = CrearTextoDestacado("Disparar");
		tituloDisp.HorizontalAlignment = HorizontalAlignment.Center;
		bloqueDisparo.AddChild(tituloDisp);

		Label descDisp = CrearTextoNormal("Dispara un láser hacia arriba para eliminar aliens.");
		descDisp.HorizontalAlignment = HorizontalAlignment.Center;
		bloqueDisparo.AddChild(descDisp);

		return contenedor;
	}

	// ============================================================
	//  PÁGINA 2 — TIPOS DE ENEMIGOS
	// ============================================================
	private Control CrearPagina2()
	{
		VBoxContainer contenedor = CrearContenedorPagina();

		// Subtítulo
		Label subtitulo = CrearSubtitulo("TIPOS DE ENEMIGOS");
		contenedor.AddChild(subtitulo);

		contenedor.AddChild(CrearSeparador());

		// --- Enemigo 1: Normal ---
		contenedor.AddChild(
			CrearFilaEnemigo(
				"res://Assets/Space-invaders-top-alien.png",
				"Enemigo Normal",
				"Va de izquierda a derecha bajando poco a poco.",
				Colors.Green
			)
		);

		contenedor.AddChild(CrearEspaciador(15));

		// --- Enemigo 2: ZigZag ---
		contenedor.AddChild(
			CrearFilaEnemigo(
				"res://Assets/images.png",
				"Enemigo ZigZag",
				"Va haciendo zigzag y baja más rápido que los demás.",
				Colors.Orange
			)
		);

		contenedor.AddChild(CrearEspaciador(15));

		// --- Enemigo 3: Tirador ---
		contenedor.AddChild(
			CrearFilaEnemigo(
				"res://Assets/Space-Invaders-Alien-PNG-Image-with-Transparent-Background.png",
				"Enemigo Tirador",
				"Este alien queda estático pero ojo, ¡este dispara!",
				Colors.Red
			)
		);

		return contenedor;
	}

	// ============================================================
	//  HELPERS PARA CONSTRUIR LA UI
	// ============================================================

	private HBoxContainer CrearFilaEnemigo(string rutaImagen, string nombre, string descripcion, Color colorNombre)
	{
		HBoxContainer fila = new HBoxContainer();
		fila.Alignment = BoxContainer.AlignmentMode.Center;
		fila.AddThemeConstantOverride("separation", 25);

		// Panel contenedor de la imagen con borde
		Panel marcoImagen = new Panel();
		marcoImagen.CustomMinimumSize = new Vector2(100, 100);

		StyleBoxFlat estiloMarco = new StyleBoxFlat();
		estiloMarco.BgColor = new Color(0.1f, 0.1f, 0.18f, 1.0f);
		estiloMarco.SetCornerRadiusAll(10);
		estiloMarco.SetBorderWidthAll(2);
		estiloMarco.BorderColor = colorNombre;
		marcoImagen.AddThemeStyleboxOverride("panel", estiloMarco);

		// Imagen del enemigo
		TextureRect imagenEnemigo = new TextureRect();
		imagenEnemigo.Texture = GD.Load<Texture2D>(rutaImagen);
		imagenEnemigo.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
		imagenEnemigo.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
		imagenEnemigo.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		imagenEnemigo.OffsetLeft = 10;
		imagenEnemigo.OffsetTop = 10;
		imagenEnemigo.OffsetRight = -10;
		imagenEnemigo.OffsetBottom = -10;
		marcoImagen.AddChild(imagenEnemigo);

		fila.AddChild(marcoImagen);

		// Textos del enemigo
		VBoxContainer textos = new VBoxContainer();
		textos.CustomMinimumSize = new Vector2(400, 0);

		Label labelNombre = new Label();
		labelNombre.Text = nombre;
		LabelSettings estiloNombre = new LabelSettings();
		estiloNombre.FontSize = 28;
		estiloNombre.FontColor = colorNombre;
		labelNombre.LabelSettings = estiloNombre;
		textos.AddChild(labelNombre);

		Label labelDesc = CrearTextoNormal(descripcion);
		textos.AddChild(labelDesc);

		fila.AddChild(textos);

		return fila;
	}

	private VBoxContainer CrearContenedorPagina()
	{
		VBoxContainer contenedor = new VBoxContainer();
		contenedor.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		contenedor.OffsetTop = 100;
		contenedor.OffsetBottom = -100;
		contenedor.OffsetLeft = 80;
		contenedor.OffsetRight = -80;
		contenedor.Alignment = BoxContainer.AlignmentMode.Center;
		contenedor.AddThemeConstantOverride("separation", 10);
		return contenedor;
	}

	private Label CrearSubtitulo(string texto)
	{
		Label label = new Label();
		label.Text = texto;
		label.HorizontalAlignment = HorizontalAlignment.Center;

		LabelSettings estilo = new LabelSettings();
		estilo.FontSize = 32;
		estilo.FontColor = new Color(0.4f, 0.7f, 1.0f);
		label.LabelSettings = estilo;

		return label;
	}

	private Label CrearTextoDestacado(string texto)
	{
		Label label = new Label();
		label.Text = texto;

		LabelSettings estilo = new LabelSettings();
		estilo.FontSize = 26;
		estilo.FontColor = Colors.Yellow;
		label.LabelSettings = estilo;

		return label;
	}

	private Label CrearTextoNormal(string texto)
	{
		Label label = new Label();
		label.Text = texto;
		label.AutowrapMode = TextServer.AutowrapMode.WordSmart;

		LabelSettings estilo = new LabelSettings();
		estilo.FontSize = 20;
		estilo.FontColor = new Color(0.85f, 0.85f, 0.85f);
		label.LabelSettings = estilo;

		return label;
	}

	private HSeparator CrearSeparador()
	{
		HSeparator sep = new HSeparator();
		sep.AddThemeConstantOverride("separation", 20);
		return sep;
	}

	private Control CrearEspaciador(float alto)
	{
		Control espaciador = new Control();
		espaciador.CustomMinimumSize = new Vector2(0, alto);
		return espaciador;
	}

	private Panel CrearTeclaVisual(string texto)
	{
		Panel tecla = new Panel();
		tecla.CustomMinimumSize = new Vector2(55, 30);

		StyleBoxFlat estilo = new StyleBoxFlat();
		estilo.BgColor = new Color(0.25f, 0.25f, 0.35f, 1.0f);
		estilo.SetCornerRadiusAll(6);
		estilo.SetBorderWidthAll(2);
		estilo.BorderColor = new Color(0.5f, 0.5f, 0.6f);
		tecla.AddThemeStyleboxOverride("panel", estilo);

		Label label = new Label();
		label.Text = texto;
		label.HorizontalAlignment = HorizontalAlignment.Center;
		label.VerticalAlignment = VerticalAlignment.Center;
		label.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

		LabelSettings estiloLabel = new LabelSettings();
		estiloLabel.FontSize = 14;
		estiloLabel.FontColor = Colors.White;
		label.LabelSettings = estiloLabel;
		tecla.AddChild(label);

		return tecla;
	}

	private Button CrearBotonNav(string texto)
	{
		Button boton = new Button();
		boton.Text = texto;
		boton.CustomMinimumSize = new Vector2(180, 45);

		StyleBoxFlat estiloNormal = new StyleBoxFlat();
		estiloNormal.BgColor = new Color(0.15f, 0.15f, 0.3f, 0.9f);
		estiloNormal.SetCornerRadiusAll(8);
		estiloNormal.SetContentMarginAll(8);
		boton.AddThemeStyleboxOverride("normal", estiloNormal);

		StyleBoxFlat estiloHover = new StyleBoxFlat();
		estiloHover.BgColor = new Color(0.25f, 0.25f, 0.5f, 0.9f);
		estiloHover.SetCornerRadiusAll(8);
		estiloHover.SetContentMarginAll(8);
		boton.AddThemeStyleboxOverride("hover", estiloHover);

		StyleBoxFlat estiloPresionado = new StyleBoxFlat();
		estiloPresionado.BgColor = new Color(0.1f, 0.1f, 0.2f, 0.9f);
		estiloPresionado.SetCornerRadiusAll(8);
		estiloPresionado.SetContentMarginAll(8);
		boton.AddThemeStyleboxOverride("pressed", estiloPresionado);

		boton.AddThemeFontSizeOverride("font_size", 20);

		return boton;
	}

	// ============================================================
	//  NAVEGACIÓN ENTRE PÁGINAS
	// ============================================================

	private void CambiarPagina(int direccion)
	{
		int nuevaPagina = paginaActual + direccion;
		if (nuevaPagina >= 0 && nuevaPagina < TOTAL_PAGINAS)
		{
			MostrarPagina(nuevaPagina);
		}
	}

	private void MostrarPagina(int pagina)
	{
		paginaActual = pagina;

		pagina1.Visible = (paginaActual == 0);
		pagina2.Visible = (paginaActual == 1);

		// Actualizar botones de navegación
		botonAnterior.Disabled = (paginaActual == 0);
		botonSiguiente.Disabled = (paginaActual == TOTAL_PAGINAS - 1);

		// Actualizar indicador
		indicadorPagina.Text = $"Página {paginaActual + 1} / {TOTAL_PAGINAS}";
	}

	private void VolverAlMenu()
	{
		GetTree().ChangeSceneToFile("res://Escenas/MainMenu.tscn");
	}
}
