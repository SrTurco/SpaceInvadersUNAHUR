using Godot;

public partial class NivelCompletado : CanvasLayer
{
    private Label continuar;
    private float tiempo;
    private bool cambiando;

    public override void _Ready()
    {
        int nivel = GameManager.instance?.nivelActual ?? 1;
        int puntos = GameManager.instance?.puntos ?? 0;
        int vidas = GameManager.instance?.vidas ?? 3;
        GetNode<Label>("UI/Panel/Sector").Text = "SECTOR // " + nivel.ToString("D2");
        GetNode<Label>("UI/Panel/PuntosValor").Text = puntos.ToString("D6");
        GetNode<Label>("UI/Panel/VidasValor").Text = vidas.ToString("D2");
        GetNode<Label>("UI/Panel/SiguienteValor").Text = nivel >= 3 ? "NAVE NODRIZA" : "NIVEL " + (nivel + 1).ToString("D2");
        continuar = GetNode<Label>("UI/Panel/Continuar");

        var progreso = GetNode<HBoxContainer>("UI/Panel/Progreso");
        for (int i = 0; i < 4; i++)
        {
            var paso = progreso.GetChild<Label>(i);
            Color color = i < nivel ? new Color("6ceba5") : new Color("365c5c");
            if (i == 3 && nivel >= 3) color = new Color("ff7290");
            paso.AddThemeColorOverride("font_color", color);
        }
    }

    public override void _Process(double delta)
    {
        tiempo += (float)delta;
        continuar.Visible = ((int)(tiempo / 0.65f) % 2) == 0;
    }

    public override void _Input(InputEvent evento)
    {
        if (cambiando || !evento.IsActionPressed("ui_accept")) return;
        cambiando = true;
        GetViewport().SetInputAsHandled();
        if (GameManager.instance != null) GameManager.instance.siguienteNivel();
        else GetTree().ChangeSceneToFile("res://Escenas/Lvl2.tscn");
    }
}
