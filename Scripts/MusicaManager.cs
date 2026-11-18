using Godot;

// Reproduce la música y los efectos de sonido de todo el juego desde un solo lugar.
// Es un Autoload: sobrevive a los cambios de escena, así la canción no se reinicia entre niveles
// y los efectos no se cortan cuando el objeto que los generó desaparece.
public partial class MusicaManager : Node
{
	public static MusicaManager instance;

	// ----- Música (se asignan arrastrando los archivos en el Inspector) -----
	[ExportGroup("Musica")]
	[Export] public AudioStream musicaMenu;
	[Export] public AudioStream musicaPartida;
	[Export] public AudioStream musicaJefe;
	[Export] public AudioStream musicaVictoria;
	[Export] public AudioStream musicaDerrota;

	// ----- Efectos de sonido -----
	[ExportGroup("Efectos")]
	[Export] public AudioStream sonidoDestruccion;
	[Export] public AudioStream sonidoDisparoJugador;
	[Export] public AudioStream sonidoDisparoEnemigo;
	[Export] public AudioStream sonidoPowerUpTriple;
	[Export] public AudioStream sonidoPowerUpRapido;
	[Export] public AudioStream sonidoPowerUpEscudo;
	[Export] public float volumenEfectosDb = -5.0f;

	private AudioStreamPlayer reproductor;
	private string escenaAnterior = "";

	public override void _Ready()
	{
		instance = this;
		reproductor = GetNode<AudioStreamPlayer>("Reproductor");
	}

	public override void _Process(double delta)
	{
		// Cada frame miro si cambiamos de pantalla. Si cambió, elijo la música.
		Node escena = GetTree().CurrentScene;
		if (escena == null) return;

		string ruta = escena.SceneFilePath;
		if (ruta == escenaAnterior) return;
		escenaAnterior = ruta;

		switch (ruta.GetFile().GetBaseName())
		{
			case "MainMenu":
				Reproducir(musicaMenu);
				break;

			case "Lvl1":
			case "Lvl2":
			case "Lvl3":
			case "NivelCompletado":
				Reproducir(musicaPartida);
				break;

			case "Win":
				Reproducir(musicaVictoria);
				break;

			case "GameOverScreen":
				Reproducir(musicaDerrota);
				break;

			// Cualquier otra pantalla (Tutorial, PauseMenu...) deja sonando lo que ya suena
		}
	}

	// ---------------- Música ----------------

	public void Reproducir(AudioStream musica)
	{
		if (musica == null) return;

		// Si ya está sonando esa canción, la dejo seguir
		if (reproductor.Stream == musica && reproductor.Playing) return;

		reproductor.Stream = musica;
		reproductor.Play();
	}

	// Para llamarlo desde otros scripts (por ejemplo el Boss)
	public void ReproducirJefe()
	{
		Reproducir(musicaJefe);
	}

	public void Detener()
	{
		reproductor.Stop();
	}

	// ---------------- Efectos ----------------

	// Crea un reproductor nuevo para cada efecto, así pueden sonar varios a la vez
	// (por ejemplo, dos naves destruidas casi juntas) y se borra solo al terminar.
	public void ReproducirEfecto(AudioStream sonido)
	{
		if (sonido == null) return;

		AudioStreamPlayer efecto = new AudioStreamPlayer();
		efecto.Stream = sonido;
		efecto.VolumeDb = volumenEfectosDb;
		AddChild(efecto);
		efecto.Finished += efecto.QueueFree;
		efecto.Play();
	}

	public void SonidoDestruccion()
	{
		ReproducirEfecto(sonidoDestruccion);
	}

	public void SonidoDisparoJugador()
	{
		ReproducirEfecto(sonidoDisparoJugador);
	}

	public void SonidoDisparoEnemigo()
	{
		ReproducirEfecto(sonidoDisparoEnemigo);
	}

	public void SonidoPowerUpTriple()
	{
		ReproducirEfecto(sonidoPowerUpTriple);
	}

	public void SonidoPowerUpRapido()
	{
		ReproducirEfecto(sonidoPowerUpRapido);
	}

	public void SonidoPowerUpEscudo()
	{
		ReproducirEfecto(sonidoPowerUpEscudo);
	}
}
