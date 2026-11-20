using Godot;
using System.Collections.Generic;

public partial class AlienManager : Node2D
{
	[Export] public PackedScene alienEscena;
	[Export] public int filas = 3;
	[Export] public int columnas = 8;
	[Export] public float espacioH = 60.0f;
	[Export] public float espacioV = 50.0f;
	[Export] public float velocidad = 80.0f;
	[Export] public PackedScene laserEnemigoEscena;

	[Export] public float offsetSuperior = 80.0f;
	[Export] public bool completarAlEliminar = true;
	[Export] public bool soloNormales = false;
	[Export] public int nivelForzado = 0;
	[Export] public int cantidadPortadoresPowerUp = 2;

	public int direccion = 1; 
	private Vector2 tamanoPantalla;
	private Vector2 posicionInicial;

	public override void _Ready()
	{
		tamanoPantalla = GetViewportRect().Size;
		CentrarYCrearAliens();
	}

	public override void _Process(double delta)
	{
		// En el nivel del jefe la horda puede acabarse antes que el jefe.
		if (!QuedanEnemigos())
		{
			if (completarAlEliminar) GameManager.instance?.pasarANivelCompletado();
			SetProcess(false);
			return;
		}

		// 1. Muevo a toda la horda para los costados
		Position += new Vector2(velocidad * direccion * (float)delta, 0);

		float anchoHorda = (columnas - 1) * espacioH;

		// 2. Si tocamos los bordes de la pantalla, bajamos un toque y cambiamos de dirección
		if (Position.X + anchoHorda >= tamanoPantalla.X - 20 && direccion == 1)
		{
			BajarYCambiar();
		}
		else if (Position.X <= 20 && direccion == -1)
		{
			BajarYCambiar();
		}

		// 3. Control de altura: Si la horda baja demasiado y llega cerca de la base, perdemos vida y vuelve arriba
		if (Position.Y >= 600.0f)
		{
			if (GameManager.instance != null)
			{
				GameManager.instance.perderVida();
				if (GameManager.instance.vidas <= 0) return;
			}
			Position = posicionInicial; 
		}

	}

	public bool QuedanEnemigos()
	{
		foreach (Node hijo in GetChildren())
			if (hijo is Enemigo && !hijo.IsQueuedForDeletion()) return true;
		return false;
	}

	private void CentrarYCrearAliens()
	{
		float anchoHorda = (columnas - 1) * espacioH;
		float xInicial = (tamanoPantalla.X - anchoHorda) / 2.0f;
		float yInicial = offsetSuperior;

		posicionInicial = new Vector2(xInicial, yInicial);
		Position = posicionInicial;

		int nivelActual = nivelForzado > 0 ? nivelForzado : (GameManager.instance != null ? GameManager.instance.nivelActual : 1);

		// Ubicaciones sin repetición: amarillos y tiradores aparecen en columnas variables.
		var columnasMezcladas = new List<int>();
		for (int i = 0; i < columnas; i++) columnasMezcladas.Add(i);
		for (int i = columnasMezcladas.Count - 1; i > 0; i--)
		{
			int j = (int)(GD.Randi() % (uint)(i + 1));
			(columnasMezcladas[i], columnasMezcladas[j]) = (columnasMezcladas[j], columnasMezcladas[i]);
		}
		int cantidadAmarillos = Mathf.Min(2, columnas);
		int cantidadTiradores = nivelActual >= 3 ? columnas - cantidadAmarillos : Mathf.Min(2, columnas - cantidadAmarillos);

		for (int r = 0; r < filas; r++)
		{
			for (int c = 0; c < columnas; c++)
			{
				Enemigo alien = (Enemigo)alienEscena.Instantiate();
				
				int lugarAleatorio = columnasMezcladas.IndexOf(c);
				if (soloNormales || r != 0)
					alien.tipo = TipoEnemigo.Normal;
				else if (lugarAleatorio < cantidadAmarillos)
					alien.tipo = TipoEnemigo.ZigZag;
				else if (lugarAleatorio < cantidadAmarillos + cantidadTiradores)
					alien.tipo = TipoEnemigo.Tirador;
				else
					alien.tipo = TipoEnemigo.Normal;

				alien.Position = new Vector2(c * espacioH, r * espacioV);
				AddChild(alien);
			}
		}
		AsignarPortadoresPowerUp(cantidadPortadoresPowerUp);
	}

	private void BajarYCambiar()
	{
		direccion *= -1;
		Position += new Vector2(0, 20);
	}

	private void _on_timer_timeout()
	{

		List<Enemigo> tiradores = new List<Enemigo>();

		foreach (Node hijo in GetChildren())
		{
			if (hijo is Enemigo alien && !alien.IsQueuedForDeletion() && alien.tipo == TipoEnemigo.Tirador)
			{
				tiradores.Add(alien);
			}
		}

		if (tiradores.Count > 0 && laserEnemigoEscena != null)
		{
			int indiceAleatorio = (int)(GD.Randi() % (uint)tiradores.Count);
			Enemigo tiradorElegido = tiradores[indiceAleatorio];

			LaserEnemigo laserE = laserEnemigoEscena.Instantiate<LaserEnemigo>();
			GetTree().CurrentScene.AddChild(laserE);
			laserE.GlobalPosition = tiradorElegido.GlobalPosition + new Vector2(0, 35);
			if (GameManager.instance?.nivelActual == 3 && GetTree().GetFirstNodeInGroup("Jugador") is Player jugador)
				laserE.ApuntarA(jugador.GlobalPosition);
			GameManager.instance?.reproducirSonido("disparo_enemigo");
		}
	}

	private void AsignarPortadoresPowerUp(int cantidad)
	{
		List<Enemigo> todosLosAliens = new List<Enemigo>();

		foreach (Node hijo in GetChildren())
		{
			if (hijo is Enemigo alien)
			{
				todosLosAliens.Add(alien);
			}
		}

		// Elegimos 'cantidad' enemigos distintos al azar
		int asignados = 0;
		while (asignados < cantidad && todosLosAliens.Count > 0)
		{
			int indice = (int)(GD.Randi() % (uint)todosLosAliens.Count);
			Enemigo elegido = todosLosAliens[indice];

			elegido.MarcarComoPortador();
			todosLosAliens.RemoveAt(indice); // Lo sacamos de la lista para no elegir el mismo dos veces
			asignados++;
		}
	}
}
