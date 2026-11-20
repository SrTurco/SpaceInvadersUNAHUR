using Godot;

public enum TipoEnemigo
{
	Normal,
	Tirador,
	ZigZag
}

public partial class Enemigo : Area2D
{
	[Export] public TipoEnemigo tipo = TipoEnemigo.Normal;
	public bool tienePowerUp = false;

	private Sprite2D skin1Node;
	private Sprite2D skin2Node;
	private Sprite2D skin3Node;

	private float tiempo = 0.0f;
	private Vector2 posicionBaseGlobal;
	private Vector2 tamanoPantalla;
	private bool esZigZagInvertido = false;
	private Sprite2D skinActual;
	private bool alcanzandoFondo;

	public override void _Ready()
	{
		skin1Node = GetNodeOrNull<Sprite2D>("Skin1");
		skin2Node = GetNodeOrNull<Sprite2D>("Skin2");
		skin3Node = GetNodeOrNull<Sprite2D>("Skin3");

		tamanoPantalla = GetViewportRect().Size;
		posicionBaseGlobal = GlobalPosition;

		if (posicionBaseGlobal.X > tamanoPantalla.X / 2.0f)
		{
			esZigZagInvertido = true;
		}

		ConfigurarSkin();
	}

	public void ConfigurarSkin()
	{
		if (skin1Node != null) skin1Node.Visible = false;
		if (skin2Node != null) skin2Node.Visible = false;
		if (skin3Node != null) skin3Node.Visible = false;

		switch (tipo)
		{
			case TipoEnemigo.Normal:
				skinActual = skin1Node;
				if (skin1Node != null) skin1Node.Visible = true;
				break;

			case TipoEnemigo.ZigZag:
				skinActual = skin2Node;
				if (skin2Node != null) skin2Node.Visible = true;
				break;

			case TipoEnemigo.Tirador:
				skinActual = skin3Node;
				if (skin3Node != null) skin3Node.Visible = true;
				break;
		}
	}

	public void Explotar()
	{
		if (IsQueuedForDeletion() || alcanzandoFondo) return;
		Color color = tipo switch
		{
			TipoEnemigo.ZigZag => new Color("ffcf62"),
			TipoEnemigo.Tirador => new Color("ff7290"),
			_ => new Color("6ceba5")
		};
		int puntos = tipo switch
		{
			TipoEnemigo.ZigZag => 200,
			TipoEnemigo.Tirador => 300,
			_ => 100
		};
		GameManager.instance?.sumarPuntos(puntos);
		GameManager.instance?.mostrarPuntos(GlobalPosition, puntos, color);
		GameManager.instance?.crearExplosion(GlobalPosition, color);
		GameManager.instance?.crearTripulantes(GlobalPosition, color);
		GameManager.instance?.reproducirSonido("explosion");
		QueueFree();
	}

	public override void _Process(double delta)
	{
		tiempo += (float)delta;
		if (skinActual != null)
		{
			// Dos poses alternadas, como los sprites clásicos del arcade.
			bool pose = ((int)(tiempo * 4.0f) % 2) == 0;
			skinActual.FlipH = pose;
			skinActual.Scale = pose ? new Vector2(1.08f, 0.92f) : new Vector2(0.92f, 1.08f);
			skinActual.Position = new Vector2(0, pose ? -2 : 0);
		}

		// 1. Movimiento en zigzag independiente para los laterales
		if (tipo == TipoEnemigo.ZigZag)
		{
			posicionBaseGlobal.Y += 55.0f * (float)delta;
			if (posicionBaseGlobal.Y >= tamanoPantalla.Y - 50)
			{
				alcanzandoFondo = true;
				SetProcess(false);
				SetDeferred("monitorable", false);
				GameManager.instance?.crearHuidaYExplosion(this, GlobalPosition, new Color("ffcf62"));
				GameManager.instance?.perderVida();
				return;
			}

			float amplitud = Mathf.Min(100.0f, Mathf.Min(posicionBaseGlobal.X - 30.0f, tamanoPantalla.X - posicionBaseGlobal.X - 30.0f));
			float offsetX = Mathf.Sin(tiempo * 2.0f) * Mathf.Max(0, amplitud) * (esZigZagInvertido ? -1 : 1);
			GlobalPosition = new Vector2(posicionBaseGlobal.X + offsetX, posicionBaseGlobal.Y);
		}

		// 2. Tirador estático arriba
		else if (tipo == TipoEnemigo.Tirador)
		{
			Node padre = GetParent();
			if (padre is AlienManager alienManager)
			{
				Position -= new Vector2(alienManager.velocidad * alienManager.direccion * (float)delta, 0);
			}
		}
	}

	public void MarcarComoPortador()
	{
		tienePowerUp = true;
		Modulate = new Color(1.0f, 0.85f, 0.2f);
	}
}
