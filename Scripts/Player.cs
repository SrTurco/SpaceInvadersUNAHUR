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

	private float tiempoTriple = 0.0f;
	private float tiempoRapido = 0.0f;
	private float tiempoEscudo = 0.0f;
	private float cooldownRapido = 0.0f;
	public override void _PhysicsProcess(double delta)
	{
		float t = (float)delta;
		ActualizarPowerUps(t);
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
		//Lógica de disparo rápido vs normal
		if (tiempoRapido > 0)
		{
			if (Input.IsActionPressed("ui_accept") && cooldownRapido <= 0)
			{
				Disparar();
				cooldownRapido = 0.15f;
			}
		}
		else if (Input.IsActionJustPressed("ui_accept"))
		{
			Disparar();
		}
	}

	// Método para instanciar el láser del jugador
	private void Disparar()
	{
		// Si ya hay un láser creado y sigue existiendo en pantalla, cortamos acá para evitar el spam
		if (tiempoRapido <= 0 && laserActual != null && GodotObject.IsInstanceValid(laserActual))
		{
			return; 
		}

		if (laserEscena == null)
		{
			GD.Print("¡Che, te olvidaste de asignar el láser en el Inspector!");
			return; 
		}
		if (tiempoTriple > 0)
		{
			float[] offsets = { -25.0f, 0.0f, 25.0f };
			foreach (float offsetX in offsets)
			{
				Node2D laser = (Node2D)laserEscena.Instantiate();
				laser.GlobalPosition = GlobalPosition + new Vector2(offsetX, -30);
				GetTree().CurrentScene.AddChild(laser);
				laserActual = laser;
			}
		}
		else
		{
			// Instancio el nuevo láser y lo guardo en la variable
			laserActual = (Node2D)laserEscena.Instantiate();
			laserActual.GlobalPosition = GlobalPosition + new Vector2(0, -30);
			GetTree().CurrentScene.AddChild(laserActual);
		}
	}


	private void ActualizarPowerUps(float t)
	{
		if (tiempoTriple > 0) tiempoTriple -= t;
		if (tiempoRapido > 0) tiempoRapido -= t;
		if (cooldownRapido > 0) cooldownRapido -= t;

		if (tiempoEscudo > 0)
		{
			tiempoEscudo -= t;
			// Si el escudo se gastó por un golpe o se acabó el tiempo, apagamos el efecto visual
			if (tiempoEscudo <= 0 || (GameManager.instance != null && !GameManager.instance.escudoActivo))
			{
				tiempoEscudo = 0;
				if (GameManager.instance != null) GameManager.instance.escudoActivo = false;
				Modulate = Colors.White; // Devuelve el color normal a la nave
			}
		}
	}
	public void ActivarPowerUp(TipoPowerUp tipo)
	{
		switch (tipo)
		{
			case TipoPowerUp.TripleTiro:
				tiempoTriple = 5.0f;
				if (MusicaManager.instance != null) MusicaManager.instance.SonidoPowerUpTriple();
				break;

			case TipoPowerUp.DisparoRapido:
				tiempoRapido = 5.0f;
				if (MusicaManager.instance != null) MusicaManager.instance.SonidoPowerUpRapido();
				break;

			case TipoPowerUp.Escudo:
				tiempoEscudo = 5.0f;
				if (GameManager.instance != null) GameManager.instance.escudoActivo = true;
				Modulate = new Color(0.4f, 0.9f, 1.0f);
				if (MusicaManager.instance != null) MusicaManager.instance.SonidoPowerUpEscudo();
				break;
		}
	}
}
