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

	private Sprite2D skin1Node;
	private Sprite2D skin2Node;
	private Sprite2D skin3Node;

	private float tiempo = 0.0f;
	private Vector2 posicionBaseGlobal;
	private Vector2 tamanoPantalla;
	private bool esZigZagInvertido = false; 

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
				if (skin1Node != null) skin1Node.Visible = true;
				break;

			case TipoEnemigo.ZigZag:
				if (skin2Node != null) skin2Node.Visible = true;
				break;

			case TipoEnemigo.Tirador:
				if (skin3Node != null) skin3Node.Visible = true;
				break;
		}
	}

	public override void _Process(double delta)
	{
		tiempo += (float)delta;

		// 1. Movimiento en zigzag independiente para los laterales
		if (tipo == TipoEnemigo.ZigZag)
		{
			posicionBaseGlobal.Y += 30.0f * (float)delta; 
			if (posicionBaseGlobal.Y > tamanoPantalla.Y + 50) 
			{
				posicionBaseGlobal.Y = -50; 
			}

			float anchoUtil = tamanoPantalla.X - 100;
			float centroX = tamanoPantalla.X / 2.0f;
			float offsetX = Mathf.Sin(tiempo * 2.0f + (esZigZagInvertido ? Mathf.Pi : 0.0f)) * (anchoUtil / 2.0f);

			GlobalPosition = new Vector2(centroX + offsetX, posicionBaseGlobal.Y);
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
}
