using Godot;

public partial class BloqueRefugio : Area2D
{
    private static readonly Color ColorBloque = new Color("ef3e54");

    public override void _Draw()
    {
        DrawRect(new Rect2(-4, -4, 8, 8), ColorBloque);
    }

    public void RecibirImpacto()
    {
        if (IsQueuedForDeletion()) return;
        QueueFree();
    }
}
