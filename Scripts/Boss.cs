using Godot;

public partial class Boss : Area2D
{
    [Export] public int vidaMaxima = 15;
    public float FraccionDanio => 1.0f - (float)vida / Mathf.Max(1, vidaMaxima);
    public int VidaActual => vida;
    private int vida;
    private Sprite2D sprite;
    private Vector2 escalaSprite;
    private float tiempoAnimacion;
    private float destello;

    public override void _Ready()
    {
        sprite = GetNode<Sprite2D>("Sprite2D");
        escalaSprite = sprite.Scale;
        vida = Mathf.Max(1, vidaMaxima);
    }

    public override void _Process(double delta)
    {
        tiempoAnimacion += (float)delta;
        destello = Mathf.Max(0, destello - (float)delta);
        bool pose = ((int)(tiempoAnimacion * 3.0f) % 2) == 0;
        sprite.Scale = escalaSprite * (pose ? new Vector2(1.04f, 0.96f) : new Vector2(0.96f, 1.04f));
        sprite.FlipH = pose;
        sprite.Modulate = destello > 0 ? new Color(1.0f, 0.35f, 0.35f) : Colors.White;

    }

    public void RecibirGolpe()
    {
        if (vida <= 0) return;
        vida--;
        destello = 0.15f;
        GameManager.instance?.reproducirSonido("jefe");
        if (vida == 0)
        {
            GameManager.instance?.sumarPuntos(1000);
            GameManager.instance?.mostrarPuntos(GlobalPosition, 1000, Colors.White);
            GameManager.instance?.crearExplosion(GlobalPosition, Colors.White, true);
            GameManager.instance?.crearTripulantes(GlobalPosition, new Color("6ceba5"), true);
            GameManager.instance?.reproducirSonido("explosion");
            QueueFree();
        }
    }
}
