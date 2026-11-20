using Godot;

// Decoración sin colisiones: pequeño alien pixelado que sale despedido y cae.
public partial class Tripulante : Node2D
{
    public Color ColorPiel = new Color("6ceba5");
    public Vector2 Velocidad;
    public float Balanceo;
    public bool Huyendo;
    private float tiempo;
    private const float Duracion = 2.2f;

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        tiempo += dt;
        if (!Huyendo) Velocidad += new Vector2(0, 240.0f * dt);
        Position += Velocidad * dt;
        Rotation = Huyendo ? 0 : Mathf.Sin(tiempo * 5.0f + Balanceo) * 0.22f;
        if (tiempo > Duracion - 0.5f)
            Modulate = new Color(1, 1, 1, Mathf.Clamp((Duracion - tiempo) / 0.5f, 0, 1));
        if (tiempo >= Duracion) QueueFree();
        else QueueRedraw();
    }

    public override void _Draw()
    {
        // 12 x 15 píxeles: cabeza ancha, ojos oscuros, cuerpo, brazos y piernas.
        Color sombra = ColorPiel.Darkened(0.25f);
        DrawRect(new Rect2(-4, -7, 8, 5), ColorPiel);
        DrawRect(new Rect2(-3, -8, 6, 1), ColorPiel);
        DrawRect(new Rect2(-3, -5, 2, 2), new Color("071719"));
        DrawRect(new Rect2(1, -5, 2, 2), new Color("071719"));
        DrawRect(new Rect2(-2, -2, 4, 5), ColorPiel);
        bool pose = ((int)(tiempo * 8) % 2) == 0;
        DrawRect(new Rect2(-5, pose ? -1 : 0, 3, 2), ColorPiel);
        DrawRect(new Rect2(2, pose ? 0 : -1, 3, 2), ColorPiel);
        DrawRect(new Rect2(-2, 3, 2, pose ? 4 : 3), sombra);
        DrawRect(new Rect2(1, 3, 2, pose ? 3 : 4), sombra);
    }
}
