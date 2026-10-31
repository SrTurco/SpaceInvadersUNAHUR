using Godot;

public partial class Player : CharacterBody2D
{
	// Velocidad a la que se mueve el jugador en la pantalla
	[Export] public float velocidad = 350.0f;
	
	// Guardamos la escena del láser para poder clonarla cuando disparamos
	[Export] public PackedScene laserEscena;

	public override void _PhysicsProcess(double delta)
	{
		// Obtenemos la velocidad actual del personaje
		Vector2 vel = Velocity;

		// Leemos si el usuario presiona las teclas de movimiento (flechas o A/D)
		Vector2 dir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		vel.X = dir.X * velocidad;

		// Aplicamos el movimiento
		Velocity = vel;
		MoveAndSlide();

		// Evitamos que el jugador se salga de los límites de la pantalla
		Vector2 pos = Position;
		pos.X = Mathf.Clamp(pos.X, 40f, 760f);
		Position = pos;

		// Si presionamos la barra espaciadora, disparamos
		if (Input.IsActionJustPressed("ui_accept"))
		{
			Disparar();
		}
	}

	// Función para crear el disparo del jugador
	private void Disparar()
	{
		// Nos aseguramos de haber asignado la escena del láser en el editor
		if (laserEscena != null)
		{
			// Creamos una copia del láser
			Node2D laser = (Node2D)laserEscena.Instantiate();
			
			// Lo ubicamos justo arriba de la nave del jugador
			laser.GlobalPosition = GlobalPosition + new Vector2(0, -30);
			
			// Lo agregamos a la escena principal para que aparezca en pantalla
			GetTree().CurrentScene.AddChild(laser);
		}
		else
		{
			GD.Print("¡Te olvidaste de asignar el láser en el Inspector!");
		}
	}
}
