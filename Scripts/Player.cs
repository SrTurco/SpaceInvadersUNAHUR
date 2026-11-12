using Godot;

public partial class Player : CharacterBody2D
{
	// A qué velocidad se mueve la nave por la pantalla
	[Export] public float velocidad = 350.0f;
	// Para que no se me salga de los bordes de la pantalla (en píxeles)
	[Export] public float margenBorde = 30.0f;
	// Acá arrastro la escena del láser desde el editor para poder clonarla
	[Export] public PackedScene laserEscena;

	// Variable para guardar el láser que acabamos de disparar
	private Node2D laserActual = null;

	public override void _PhysicsProcess(double delta)
	{
		// Agarro la velocidad actual de movimiento
		Vector2 vel = Velocity;
		// Leo las teclas (flechas o A/D para movernos a los costados)
		Vector2 dir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		vel.X = dir.X * velocidad;

		// Le aplico la física de movimiento de Godot
		Velocity = vel;
		MoveAndSlide();

		// Averiguo el ancho de la pantalla para calcular los límites
		float anchoPantalla = GetViewportRect().Size.X;
		
		// Limito la posición para que la nave no atraviese las paredes invisibles
		Vector2 pos = Position;
		pos.X = Mathf.Clamp(pos.X, margenBorde, anchoPantalla - margenBorde);
		Position = pos;

		// Si apreto la barra espaciadora, disparamos
		if (Input.IsActionJustPressed("ui_accept"))
		{
			Disparar();
		}
	}

	// Método para instanciar el láser del jugador
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
