using Godot;

// Ambas pantallas muestran la ilustración y luego el puntaje durante cinco segundos.
public partial class SecuenciaFinal : CanvasLayer
{
    protected virtual bool EsVictoria => false;
    private const float SegundosDeImagen = 2.0f;
    private const float SegundosDePuntaje = 5.0f;
    private Label textoPuntaje;
    private Label textoRegreso;
    private float tiempo;
    private bool puntajeMostrado;

    public override void _Ready()
    {
        textoPuntaje = GetNode<Label>("Puntaje");
        textoRegreso = GetNode<Label>("Regreso");
        textoPuntaje.Visible = false;
        textoRegreso.Visible = false;
    }

    public override void _Process(double delta)
    {
        tiempo += (float)delta;
        if (tiempo < SegundosDeImagen) return;

        if (!puntajeMostrado)
        {
            puntajeMostrado = true;
            int puntos = GameManager.instance == null ? 0 :
                EsVictoria ? GameManager.instance.puntos : GameManager.instance.puntajeFinalDerrota;
            textoPuntaje.Text = "PUNTAJE FINAL: " + puntos.ToString("D6");
            textoPuntaje.Visible = true;
            textoRegreso.Visible = true;
        }

        float restante = SegundosDeImagen + SegundosDePuntaje - tiempo;
        textoRegreso.Text = "VOLVIENDO AL MENU EN " + Mathf.Max(0, Mathf.CeilToInt(restante));
        if (restante <= 0)
        {
            SetProcess(false);
            if (GameManager.instance != null)
            {
                GameManager.instance.vidas = 3;
                GameManager.instance.puntos = 0;
                GameManager.instance.nivelActual = 1;
                GameManager.instance.puntajeFinalDerrota = 0;
            }
            GetTree().ChangeSceneToFile("res://Escenas/MainMenu.tscn");
        }
    }
}
