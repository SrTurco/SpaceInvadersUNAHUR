using Godot;

// Cuatro refugios formados por bloques independientes que absorben los disparos.
public partial class Refugios : Node2D
{
    private const int Cantidad = 4;
    private const int Columnas = 12;
    private const int Filas = 7;
    private const float Tamano = 8.0f;

    public override void _Ready()
    {
        float ancho = GetViewportRect().Size.X;
        float alto = GetViewportRect().Size.Y;
        var forma = new RectangleShape2D { Size = new Vector2(Tamano, Tamano) };
        for (int refugio = 0; refugio < Cantidad; refugio++)
        {
            float centroX = ancho * (refugio + 0.5f) / Cantidad;
            for (int fila = 0; fila < Filas; fila++)
            {
                for (int columna = 0; columna < Columnas; columna++)
                {
                    // Esquinas superiores diagonales y arco inferior, como en el original.
                    if (fila == 0 && (columna < 2 || columna > 9)) continue;
                    if (fila == 1 && (columna == 0 || columna == 11)) continue;
                    if (fila >= 5 && columna >= 4 && columna <= 7) continue;
                    if (fila == 4 && columna >= 5 && columna <= 6) continue;
                    var bloque = new BloqueRefugio
                    {
                        Position = new Vector2(centroX + (columna - 5.5f) * Tamano,
                            alto - 150.0f + fila * Tamano),
                        Monitoring = false
                    };
                    bloque.AddChild(new CollisionShape2D { Shape = forma });
                    AddChild(bloque);
                }
            }
        }
    }
}
