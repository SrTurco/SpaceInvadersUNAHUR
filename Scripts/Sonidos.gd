extends Node
# Sonidos y música del juego, cargados desde archivos de audio (.wav / .ogg)
# que se asignan en el Inspector de Sonidos.tscn.
# Efectos: Sonidos.reproducir("explosion")
# Música: cambia sola según la escena (ver _process).

# ----- Música (un reproductor propio, aparte de los efectos) -----
@export_group("Musica")
@export var musica_menu: AudioStream
@export var musica_partida: AudioStream
@export var musica_jefe: AudioStream
@export var musica_victoria: AudioStream
@export var musica_derrota: AudioStream
@export var volumen_musica_db: float = -10.0

# ----- Efectos de sonido -----
@export_group("Efectos")
@export var hover: AudioStream
@export var disparo: AudioStream
@export var disparo_enemigo: AudioStream
@export var explosion: AudioStream
@export var danio: AudioStream
@export var jefe: AudioStream
@export var carga: AudioStream
@export var rayo: AudioStream

var sonidos: Dictionary = {}
var reproductores: Array[AudioStreamPlayer] = []
var reproductor_musica: AudioStreamPlayer
var escena_anterior: String = ""

func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS

	# Arma el diccionario nombre -> archivo de audio
	sonidos = {
		"hover": hover,
		"disparo": disparo,
		"disparo_enemigo": disparo_enemigo,
		"explosion": explosion,
		"danio": danio,
		"jefe": jefe,
		"carga": carga,
		"rayo": rayo,
	}

	# Reproductor exclusivo de la música
	reproductor_musica = AudioStreamPlayer.new()
	reproductor_musica.volume_db = volumen_musica_db
	add_child(reproductor_musica)

	# 8 reproductores de efectos que se reutilizan, así pueden sonar varios a la vez
	for i in range(8):
		var player := AudioStreamPlayer.new()
		player.volume_db = -8.0
		add_child(player)
		reproductores.append(player)

	# Conecta también botones creados por código, como los de pausa y tutorial.
	get_tree().node_added.connect(_registrar_boton)

# Cada frame mira si cambiamos de escena. Si cambió, elige la música que corresponde.
# Si ya está sonando esa misma canción, la deja seguir (por eso no se reinicia entre niveles).
func _process(_delta: float) -> void:
	var escena := get_tree().current_scene
	if escena == null:
		return

	var ruta: String = escena.scene_file_path
	if ruta == escena_anterior:
		return
	escena_anterior = ruta

	match ruta.get_file().get_basename():
		"MainMenu":
			reproducir_musica(musica_menu)
		"Lvl1", "Lvl2", "Lvl3", "NivelCompletado":
			reproducir_musica(musica_partida)
		"Boss":
			reproducir_musica(musica_jefe)
		"Win":
			reproducir_musica(musica_victoria)
		"GameOverScreen":
			reproducir_musica(musica_derrota)
		# Cualquier otra escena (Tutorial...) deja sonando lo que ya suena

func reproducir_musica(musica: AudioStream) -> void:
	# Si no hay archivo asignado, no cambia nada
	if musica == null:
		return
	# Si esa canción ya está sonando, la dejo seguir
	if reproductor_musica.stream == musica and reproductor_musica.playing:
		return
	reproductor_musica.stream = musica
	reproductor_musica.play()

func detener_musica() -> void:
	reproductor_musica.stop()

# Permite registrar de forma explícita una escena ya cargada sin duplicar conexiones.
func registrar_botones(raiz: Node) -> void:
	_registrar_boton(raiz)
	for hijo in raiz.get_children():
		registrar_botones(hijo)

func _registrar_boton(nodo: Node) -> void:
	if nodo is BaseButton:
		var boton := nodo as BaseButton
		var llamada := _al_pasar_mouse.bind(boton)
		if not boton.mouse_entered.is_connected(llamada):
			boton.mouse_entered.connect(llamada)

func _al_pasar_mouse(boton: BaseButton) -> void:
	if is_instance_valid(boton) and boton.is_visible_in_tree() and not boton.disabled:
		reproducir("hover")

func reproducir(nombre: String) -> void:
	# Si el nombre no existe o todavía no le asignaste un archivo, no hace nada
	if not sonidos.has(nombre) or sonidos[nombre] == null:
		return
	for player in reproductores:
		if not player.playing:
			player.volume_db = -1.0 if nombre == "hover" else -8.0
			player.stream = sonidos[nombre]
			player.play()
			return
