using Godot;

public partial class Player : CharacterBody2D
{
	[Export] public float velocidad = 350.0f;
	[Export] public PackedScene laserEscena; // arrastramos el laser aca

	public override void _PhysicsProcess(double delta)
	{
		Vector2 vel = Velocity;

		// nos movemos a los lados con las flechas o a/d
		Vector2 dir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		vel.X = dir.X * velocidad;

		Velocity = vel;
		MoveAndSlide();

		// que no se salga de la pantalla
		Vector2 pos = Position;
		pos.X = Mathf.Clamp(pos.X, 40f, 760f);
		Position = pos;

		// disparar con espacio
		if (Input.IsActionJustPressed("ui_accept"))
		{
			disparar();
		}
	}

	private void disparar()
	{
		if (laserEscena != null)
		{
			Node2D laser = (Node2D)laserEscena.Instantiate();
			laser.GlobalPosition = GlobalPosition + new Vector2(0, -30);
			GetTree().CurrentScene.AddChild(laser);
		}
		else
		{
			GD.Print("che, te olvidaste de asignar el laser en el inspector!");
		}
	}
}
