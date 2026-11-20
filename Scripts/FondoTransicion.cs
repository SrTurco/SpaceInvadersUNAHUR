using Godot;
using System.Collections.Generic;

// Campo de estrellas decorativo: no participa en la partida.
public partial class FondoTransicion : Node2D
{
    private readonly List<Vector2> estrellas = new();
    private float tiempo;

    public override void _Ready()
    {
        var azar = new RandomNumberGenerator { Seed = 2709 };
        Vector2 pantalla = GetViewportRect().Size;
        for (int i = 0; i < 60; i++)
            estrellas.Add(new Vector2(azar.RandfRange(20, pantalla.X - 20), azar.RandfRange(20, pantalla.Y - 20)));
    }

    public override void _Process(double delta)
    {
        tiempo += (float)delta;
        QueueRedraw();
    }

    public override void _Draw()
    {
        for (int i = 0; i < estrellas.Count; i++)
        {
            float brillo = 0.18f + 0.20f * (0.5f + 0.5f * Mathf.Sin(tiempo * 1.8f + i));
            float tamano = i % 7 == 0 ? 3.0f : 2.0f;
            DrawRect(new Rect2(estrellas[i], new Vector2(tamano, tamano)), new Color(0.35f, 0.9f, 0.82f, brillo));
        }
    }
}
