using Godot;

public partial class Player : CharacterBody2D
{
	[Export] public float velocidad = 350.0f;
	[Export] public float margenBorde = 30.0f;
	[Export] public PackedScene laserEscena;

	// Variable para guardar el láser que acabamos de disparar
	private Node2D laserActual = null;

	public override void _PhysicsProcess(double delta)
	{
		Vector2 vel = Velocity;
		Vector2 dir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		vel.X = dir.X * velocidad;

		Velocity = vel;
		MoveAndSlide();

		float anchoPantalla = GetViewportRect().Size.X;
		Vector2 pos = Position;
		pos.X = Mathf.Clamp(pos.X, margenBorde, anchoPantalla - margenBorde);
		Position = pos;

		if (Input.IsActionJustPressed("ui_accept"))
		{
			Disparar();
		}
	}

	private void Disparar()
	{
		// Si ya hay un láser creado y sigue existiendo en pantalla, cortamos acá para evitar el spam
		if (laserActual != null && GodotObject.IsInstanceValid(laserActual))
		{
			return; 
		}

		if (laserEscena != null)
		{
			// Instancio el nuevo láser y lo guardo en la variable
			laserActual = (Node2D)laserEscena.Instantiate();
			laserActual.GlobalPosition = GlobalPosition + new Vector2(0, -30);
			GetTree().CurrentScene.AddChild(laserActual);
		}
		else
		{
			GD.Print("¡Che, te olvidaste de asignar el láser en el Inspector!");
		}
	}
}
