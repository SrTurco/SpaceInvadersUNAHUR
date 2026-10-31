using Godot;

public partial class GameManager : Node
{
	public static GameManager instance; // para llamarlo desde cualquier lado

	[Export] public int puntos = 0;
	[Export] public int vidas = 3;
	private int nivelActual = 1;

	public override void _Ready()
	{
		instance = this;
	}

	public void sumarPuntos(int cantidad)
	{
		puntos += cantidad;
		GD.Print("puntos actuales: " + puntos);
	}

	public void perderVida()
	{
		vidas--;
		if (vidas <= 0)
		{
			GetTree().ChangeSceneToFile("res://Scenes/GameOverScreen.tscn");
		}
	}
}
