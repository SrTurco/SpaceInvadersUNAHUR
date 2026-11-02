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

	// Referencias a las skins dentro del nodo Enemigo (pueden ser Sprite2D)
	[Export] public Sprite2D skin1;
	[Export] public Sprite2D skin2;
	[Export] public Sprite2D skin3;

	private float tiempoZigZag = 0.0f;

	public override void _Ready()
	{
		ConfigurarSkin();
	}

	public void ConfigurarSkin()
	{
		// Ocultar todas las skins
		if (skin1 != null) skin1.Visible = false;
		if (skin2 != null) skin2.Visible = false;
		if (skin3 != null) skin3.Visible = false;

		// Mostrar la skin correcta según el tipo asignado
		switch (tipo)
		{
			case TipoEnemigo.Normal:
				if (skin1 != null) skin1.Visible = true;
				break;

			case TipoEnemigo.Tirador:
				if (skin2 != null) skin2.Visible = true;
				break;

			case TipoEnemigo.ZigZag:
				if (skin3 != null) skin3.Visible = true;
				break;
		}
	}

	public override void _Process(double delta)
	{
		// Si el alien es tipo ZigZag, oscila horizontalmente dentro de su celda
		if (tipo == TipoEnemigo.ZigZag)
		{
			tiempoZigZag += (float)delta * 6.0f; // Velocidad del zigzag

			if (skin3 != null)
			{
				skin3.Position = new Vector2(Mathf.Sin(tiempoZigZag) * 12.0f, skin3.Position.Y);
			}
		}
	}
}
