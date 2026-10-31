using Godot;

public partial class GameManager : Node
{
	public static GameManager instance;

	[Export] public int puntos = 0;
	[Export] public int vidas = 3;
	public int nivelActual = 1; // Para saber qué nivel estamos jugando

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
			GetTree().ChangeSceneToFile("res://Escenas/GameOverScreen.tscn");
		}
	}

	public void siguienteNivel()
	{
		nivelActual++;
		
		// Cambiamos de escena según el número de nivel
		if (nivelActual == 2)
		{
			GetTree().ChangeSceneToFile("res://Escenas/Lvl2.tscn");
		}
		else if (nivelActual == 3)
		{
			GetTree().ChangeSceneToFile("res://Escenas/Lvl3.tscn");
		}
		else
		{
			GetTree().ChangeSceneToFile("res://Escenas/WinScreen.tscn");
		}
	}
	
	public void pasarANivelCompletado()
	{
		GetTree().ChangeSceneToFile("res://Escenas/NivelCompletado.tscn");
	}
}
