using Godot;

public partial class MusicaManager : Node
{
	public static MusicaManager instance;

	[Export] public AudioStream musicaMenu;
	[Export] public AudioStream musicaPartida;
	[Export] public AudioStream musicaJefe;
	[Export] public AudioStream musicaVictoria;
	[Export] public AudioStream musicaDerrota;

	private AudioStreamPlayer reproductor;
	private string escenaAnterior = "";

	public override void _Ready()
	{
		instance = this;
		reproductor = GetNode<AudioStreamPlayer>("Reproductor");
	}

	public override void _Process(double delta)
	{
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
			case "Tutorial":
				Reproducir(musicaPartida);
				break;

			case "Win":
				Reproducir(musicaVictoria);
				break;

			case "GameOverScreen":
				Reproducir(musicaDerrota);
				break;

		}
	}

	public void Reproducir(AudioStream musica)
	{
		if (musica == null) return;

		if (reproductor.Stream == musica && reproductor.Playing) return;

		reproductor.Stream = musica;
		reproductor.Play();
	}

	public void ReproducirJefe()
	{
		Reproducir(musicaJefe);
	}

	public void Detener()
	{
		reproductor.Stop();
	}
}
