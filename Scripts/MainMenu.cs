using Godot;

public partial class MainMenu : CanvasLayer
{
    private Control titulo;
    private Label insertarFicha;
    private Button jugar;
    private float tiempo;
    private bool cambiando;

    public override void _Ready()
    {
        titulo = GetNode<Control>("UI/Titulo");
        insertarFicha = GetNode<Label>("UI/InsertarFicha");
        jugar = GetNode<Button>("UI/Botones/Lista/Jugar");
        Button tutorial = GetNode<Button>("UI/Botones/Lista/Tutorial");
        Button salir = GetNode<Button>("UI/Botones/Lista/Salir");
        EstilizarBoton(jugar, new Color("6ceba5"));
        EstilizarBoton(tutorial, new Color("ffcf62"));
        EstilizarBoton(salir, new Color("ff7290"));
        jugar.Pressed += IniciarJuego;
        tutorial.Pressed += AbrirTutorial;
        salir.Pressed += () => GetTree().Quit();
        // Asegura la conexión de hover incluso si la escena ya estaba cargada.
        GetNodeOrNull<Node>("/root/Sonidos")?.Call("registrar_botones", this);
        jugar.GrabFocus();
        Tween entrada = CreateTween();
        entrada.TweenProperty(titulo, "position:y", 76.0f, 1.25).SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);
    }

    public override void _Process(double delta)
    {
        tiempo += (float)delta;
        insertarFicha.Visible = ((int)(tiempo / 0.65f) % 2) == 0;
    }

    public override void _UnhandledInput(InputEvent evento)
    {
        if (evento is InputEventKey tecla && tecla.Pressed && !tecla.Echo && tecla.Keycode == Key.T)
        {
            AbrirTutorial();
            GetViewport().SetInputAsHandled();
        }
        else if (evento.IsActionPressed("ui_accept") && GetViewport().GuiGetFocusOwner() == null)
        {
            IniciarJuego();
            GetViewport().SetInputAsHandled();
        }
    }

    private void IniciarJuego()
    {
        if (cambiando) return;
        cambiando = true;
        if (GameManager.instance != null)
        {
            GameManager.instance.vidas = 3;
            GameManager.instance.puntos = 0;
            GameManager.instance.nivelActual = 1;
        }
        GetTree().ChangeSceneToFile("res://Escenas/Lvl1.tscn");
    }

    private void AbrirTutorial()
    {
        if (cambiando) return;
        cambiando = true;
        GetTree().ChangeSceneToFile("res://Escenas/Tutorial.tscn");
    }

    private static void EstilizarBoton(Button boton, Color color)
    {
        boton.AddThemeFontSizeOverride("font_size", 23);
        boton.AddThemeColorOverride("font_color", color);
        boton.AddThemeColorOverride("font_hover_color", new Color("071719"));
        boton.AddThemeColorOverride("font_pressed_color", new Color("071719"));
        boton.AddThemeColorOverride("font_focus_color", color);
        var normal = new StyleBoxFlat { BgColor = new Color("071719"), BorderColor = color };
        normal.SetBorderWidthAll(2); normal.SetContentMarginAll(7);
        var hover = new StyleBoxFlat { BgColor = color, BorderColor = color };
        hover.SetBorderWidthAll(2); hover.SetContentMarginAll(7);
        var presionado = new StyleBoxFlat { BgColor = color.Darkened(0.25f), BorderColor = Colors.White };
        presionado.SetBorderWidthAll(2); presionado.SetContentMarginAll(7);
        var foco = new StyleBoxFlat { DrawCenter = false, BorderColor = Colors.White };
        foco.SetBorderWidthAll(2); foco.ExpandMarginLeft = 3; foco.ExpandMarginRight = 3; foco.ExpandMarginTop = 3; foco.ExpandMarginBottom = 3;
        boton.AddThemeStyleboxOverride("normal", normal);
        boton.AddThemeStyleboxOverride("hover", hover);
        boton.AddThemeStyleboxOverride("pressed", presionado);
        boton.AddThemeStyleboxOverride("focus", foco);
    }
}
