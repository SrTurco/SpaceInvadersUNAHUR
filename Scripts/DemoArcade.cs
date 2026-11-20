using Godot;
using System.Collections.Generic;

// Demostración visual aislada: usa la escena real del nivel 2 sin ejecutar su lógica.
public partial class DemoArcade : Node2D
{
    [Export] public PackedScene escenaNivel;
    private Node2D nivel;
    private AlienManager horda;
    private Player nave;
    private readonly List<Enemigo> enemigos = new();
    private readonly List<Vector2> disparosNave = new();
    private readonly List<Vector2> disparosEnemigos = new();
    private float tiempo;
    private float proximoDisparo;
    private float proximoDisparoEnemigo;
    private readonly RandomNumberGenerator azar = new();

    public override void _Ready()
    {
        azar.Randomize();
        nivel = escenaNivel.Instantiate<Node2D>();
        nivel.ProcessMode = ProcessModeEnum.Disabled;
        horda = nivel.GetNode<AlienManager>("AlienManager");
        horda.nivelForzado = 2;
        horda.completarAlEliminar = false;
        nivel.GetNode<CanvasLayer>("Interfaz").Visible = false;
        AddChild(nivel);
        nave = nivel.GetNode<Player>("CharacterBody2D");
        nave.RemoveFromGroup("Jugador");
        foreach (Node hijo in horda.GetChildren()) if (hijo is Enemigo alien) enemigos.Add(alien);
        Modulate = new Color(1, 1, 1, 0.46f);
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        tiempo += dt;
        horda.Position = new Vector2(200 + Mathf.Sin(tiempo * 0.55f) * 100, 85 + Mathf.Sin(tiempo * 0.2f) * 12);
        nave.Position = new Vector2(512 + Mathf.Sin(tiempo * 0.8f) * 310, 710);
        bool pose = ((int)(tiempo * 4) % 2) == 0;
        foreach (Enemigo alien in enemigos)
            foreach (Node hijo in alien.GetChildren())
                if (hijo is Sprite2D sprite && sprite.Visible)
                    sprite.Scale = pose ? new Vector2(1.08f, 0.92f) : new Vector2(0.92f, 1.08f);

        proximoDisparo -= dt;
        if (proximoDisparo <= 0)
        {
            disparosNave.Add(nave.GlobalPosition + new Vector2(0, -25));
            proximoDisparo = 0.7f;
        }
        proximoDisparoEnemigo -= dt;
        if (proximoDisparoEnemigo <= 0 && enemigos.Count > 0)
        {
            Enemigo elegido = enemigos[azar.RandiRange(0, enemigos.Count - 1)];
            if (elegido.Visible) disparosEnemigos.Add(elegido.GlobalPosition + new Vector2(0, 25));
            proximoDisparoEnemigo = 1.1f;
        }
        for (int i = disparosNave.Count - 1; i >= 0; i--)
        {
            Vector2 pos = disparosNave[i] + Vector2.Up * 400 * dt;
            bool impacto = false;
            foreach (Enemigo alien in enemigos)
                if (alien.Visible && Mathf.Abs(alien.GlobalPosition.X - pos.X) < 24 && Mathf.Abs(alien.GlobalPosition.Y - pos.Y) < 22)
                { alien.Visible = false; impacto = true; break; }
            if (impacto || pos.Y < -30) disparosNave.RemoveAt(i);
            else disparosNave[i] = pos;
        }
        for (int i = disparosEnemigos.Count - 1; i >= 0; i--)
        {
            Vector2 pos = disparosEnemigos[i] + Vector2.Down * 250 * dt;
            if (pos.Y > GetViewportRect().Size.Y + 30) disparosEnemigos.RemoveAt(i);
            else disparosEnemigos[i] = pos;
        }
        // La demostración se repite sin modificar puntos, vidas ni escenas reales.
        if (tiempo > 22)
        {
            foreach (Enemigo alien in enemigos) alien.Visible = true;
            tiempo = 0;
        }
        QueueRedraw();
    }

    public override void _Draw()
    {
        foreach (Vector2 pos in disparosNave) DrawRect(new Rect2(ToLocal(pos) - new Vector2(2, 10), new Vector2(4, 20)), new Color("73dcff"));
        foreach (Vector2 pos in disparosEnemigos) DrawRect(new Rect2(ToLocal(pos) - new Vector2(2, 8), new Vector2(4, 16)), new Color("ff7290"));
    }
}
