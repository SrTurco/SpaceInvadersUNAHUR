using Godot;

public partial class GameManager : Node
{
	// Instancia global para acceder a los datos desde cualquier script fácilmente
	public static GameManager instance;

	[Export] public int puntos = 0;
	[Export] public int vidas = 3;
	public int nivelActual = 1; 

	public override void _Ready()
	{
		instance = this;
	}

	// Función para sumar puntos cuando destruimos un alien
	public void sumarPuntos(int cantidad)
	{
		puntos += cantidad;
		GD.Print("Puntos actuales: " + puntos);
	}

	// Función para restar vidas cuando nos pega un enemigo o bajan demasiado
	public void perderVida()
	{
		vidas--;
		if (vidas <= 0)
		{
			// Si nos quedamos sin vidas, vamos a la pantalla de Game Over
			GetTree().ChangeSceneToFile("res://Escenas/GameOverScreen.tscn");
		}
	}

	// Función para avanzar de nivel
	public void siguienteNivel()
	{
		nivelActual++;
		
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
	
	// Pantalla intermedia cuando completamos un nivel
	public void pasarANivelCompletado()
	{
		GetTree().ChangeSceneToFile("res://Escenas/NivelCompletado.tscn");
	}
	}
