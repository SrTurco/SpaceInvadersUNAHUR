using Godot;

public partial class GameManager : Node
{
	// Instancia estática para poder consultar datos desde cualquier script fácilmente (como un GameManager global)
	public static GameManager instance;

	[Export] public int puntos = 0;
	[Export] public int vidas = 3;
	public int nivelActual = 1; 

	public override void _Ready()
	{
		instance = this;
	}

	// Sube el puntaje cuando matamos un alien
	public void sumarPuntos(int cantidad)
	{
		puntos += cantidad;
		GD.Print("Puntos actuales: " + puntos);
	}

	// Resta vida si nos pegan o si los aliens se pasan de la raya abajo
	public void perderVida()
	{
		vidas--;
		if (vidas <= 0)
		{
			// Si nos quedamos sin vidas, mandamos a la pantalla de Game Over
			GetTree().ChangeSceneToFile("res://Escenas/GameOverScreen.tscn");
		}
	}

	// Función para avanzar al siguiente nivel según el número actual
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
	
	// Pantalla intermedia cuando pasamos un nivel
	public void pasarANivelCompletado()
	{
		GetTree().ChangeSceneToFile("res://Escenas/NivelCompletado.tscn");
	}
}
