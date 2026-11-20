# Guía del proyecto Space Invaders UNAHUR

Esta guía describe el estado actual del proyecto y dónde modificar cada sistema. El juego usa escenas de Godot, scripts C# y un script GDScript para generar los sonidos. La implementación es una reinterpretación del arcade original, no una copia exacta de sus reglas o gráficos.

## Cómo usar esta guía

Si abrís el proyecto por primera vez, empezá por **1. Abrir y ejecutar**. Si ya lo hiciste, la tabla siguiente te dice dónde buscar un cambio; las secciones posteriores explican cómo funciona cada sistema. Al final hay recetas paso a paso, una explicación de las conexiones entre scripts y una tabla de problemas frecuentes.

| Quiero cambiar… | Abrí | Dónde se modifica |
|---|---|---|
| Velocidad del jugador | `Escenas/player.tscn` | Inspector del nodo raíz → `velocidad` de `Player.cs`. |
| Cantidad, separación o velocidad de enemigos | `Escenas/Lvl1.tscn`, `Lvl2.tscn` o `Lvl3.tscn` | Nodo `AlienManager` → propiedades exportadas. |
| Frecuencia de disparo de un nivel | La escena del nivel | Nodo `AlienManager/Timer` → **Wait Time**. |
| Puntos de un enemigo | `Scripts/Enemigo.cs` | `switch` de `puntos` dentro de `Explotar()`. |
| Vida, escoltas o fases del jefe | `Escenas/Boss.tscn` | Inspector de la raíz `NivelBoss` y del nodo `Jefe`. |
| Refugios | `Scripts/Refugios.cs` y `Scripts/BloqueRefugio.cs` | Constantes, forma de la cuadrícula y dibujo de bloques. |
| Texto o botones del menú | `Escenas/MainMenu.tscn` y `Scripts/MainMenu.cs` | Etiquetas en la escena; acciones, animación y estilos en el script. |
| Imágenes finales y tiempos | `Escenas/Win.tscn`, `Escenas/GameOverScreen.tscn` y `Scripts/SecuenciaFinal.cs` | Texturas en las escenas; constantes de tiempo en el script. |
| Sonido | `Scripts/Sonidos.gd` | Llamadas `_crear(...)` y volumen de los reproductores. |
| Filtro CRT | `Escenas/EfectoCRT.tscn` → `Pantalla` | Material → **Shader Parameters**. |

**Cómo editar una propiedad exportada:** abrí la escena, seleccioná el nodo en el árbol y buscá la propiedad en el Inspector. Guardá la escena y probá el resultado. Si modificás C#, guardá y recompilá. Un valor guardado en una instancia de escena puede sobrescribir el valor inicial del script: si un cambio en C# no se refleja, revisá el Inspector de esa instancia.

## 1. Abrir y ejecutar

- Usar **Godot 4.7.2 .NET** y el **SDK de .NET 8** (o uno compatible). La edición común de Godot no compila C#.
- Importar `project.godot` desde el gestor de proyectos. Esperar la importación de recursos.
- Compilar desde el editor de Godot y ejecutar con **F5**. F6 ejecuta solo la escena abierta; para probar la progresión completa usar F5.
- Controles: **Enter** inicia desde el menú, **T** abre el tutorial, **flechas** mueven la nave, **Espacio** dispara o continúa y **Esc** pausa. En victoria y derrota, primero aparece la ilustración durante dos segundos; luego se muestra el puntaje final durante cinco segundos y se vuelve automáticamente al menú.
- Para probar rápidamente la pelea final: entrar a una partida, presionar **Esc** y seleccionar **PROBAR JEFE FINAL**. El botón carga la nodriza con tres vidas y cero puntos; también permite reiniciar la pelea desde su propio menú de pausa.

Si se ejecuta directamente un nivel desde el editor, el `GameManager` puede seguir con `nivelActual = 1`, por lo que los tipos y la progresión pueden no coincidir con una partida normal. Probá la progresión comenzando en el menú.

## Icono y exportación para Windows

`Assets/icono_nodriza.svg` es el icono del proyecto y de la ventana, configurado en `application/config/icon`. La configuración `Windows Desktop` de `export_presets.cfg` usa el mismo archivo para el ejecutable. Para generar el juego en Windows, instalar las plantillas **4.7.2 .NET** desde el gestor de plantillas de Godot y exportar usando ese preset. Godot convierte el SVG a ICO al exportar; el aspecto final del ejecutable debe comprobarse después de generar el archivo. [Referencia de Godot](https://docs.godotengine.org/en/stable/tutorials/export/changing_application_icon_for_windows.html).

## 2. Mapa del proyecto

| Ubicación | Función |
|---|---|
| `project.godot` | Configuración, escena inicial y autoloads. |
| `Escenas/` | Niveles, jugador, enemigos, balas, interfaces, jefe y efecto CRT. |
| `Scripts/` | Comportamientos en C# y controlador de sonido en GDScript. |
| `Assets/` | Imágenes y sprites; la nave y los enemigos comunes usan SVG pixelados. |
| `Shaders/crt.gdshader` | Shader que dibuja el efecto de pantalla arcade. |
| `addons/gdfxr/` | Complemento externo para generar sonidos sfxr, con licencia MIT incluida. |

Autoloads registrados en el proyecto:

- **Sonidos** (`Scripts/Sonidos.gd`): genera y reproduce efectos retro.
- **GameManager** (`Scripts/GameManager.cs`): conserva vidas, puntos y nivel; también crea partículas.
- **PauseMenu** (`Escenas/PauseMenu.tscn` y `Scripts/PauseMenu.cs`): pausa y opciones para volver o salir. Su panel arcade y sus botones se crean en `CrearInterfaz()` y `CrearBoton()`: usan Pixelify Sans como fuente global y colores de la nave y los enemigos. Los estilos normal, hover, presionado y foco se pueden ajustar allí.
- **EfectoCRT** (`Escenas/EfectoCRT.tscn`): capa visual para toda la pantalla.

Los autoloads permanecen al cambiar de escena. En particular, los puntos y vidas continúan entre niveles y las partículas pueden terminar aunque cambie la escena.

## Menú arcade y finales

`Escenas/MainMenu.tscn` es la pantalla de inicio. `Scripts/MainMenu.cs` anima el título desde arriba, hace parpadear «INSERTE FICHA» y conecta los botones JUGAR, TUTORIAL y SALIR. Se puede navegar con flechas y Enter; T sigue abriendo el tutorial. Los estilos de los botones se definen en `EstilizarBoton()` y usan la misma paleta que la pausa. La duración de entrada y la velocidad de parpadeo se ajustan en `MainMenu.cs`.

El fondo es una demostración visual automática creada por `Scripts/DemoArcade.cs` a partir de `Lvl2.tscn`. La lógica de esa instancia está desactivada: el script anima nave, formación y disparos visuales sin colisiones, sin sonido y sin modificar vidas, puntos ni progreso. La demostración se repite. `nivelForzado = 2` permite que la formación del fondo use la distribución del nivel 2 sin tocar el estado de `GameManager`.

El título usa `Assets/Fonts/SpaceInvaders.ttf`, incluido localmente; las demás etiquetas siguen usando Pixelify Sans. Es la fuente Space Invaders Regular de kylemaoin (2010), descargada del [sitio solicitado](https://www.dafontfree.co/space-invaders-font/). Sus metadatos indican CC BY-SA 3.0, mientras que la página indica uso personal; `Assets/Fonts/SpaceInvaders-fuente.txt` conserva la atribución y los enlaces. Confirmar las condiciones con el autor antes de distribuir o utilizar comercialmente.

Entre oleadas, `Escenas/NivelCompletado.tscn` muestra el sector terminado, el progreso por los tres niveles, el puntaje y las vidas actuales y el próximo destino. Después del nivel 3 anuncia «NAVE NODRIZA». `Scripts/NivelCompletado.cs` completa esos datos desde `GameManager`, hace parpadear la instrucción y avanza al presionar Espacio; no modifica el puntaje ni las vidas. `Scripts/FondoTransicion.cs` dibuja las estrellas decorativas. Los textos, colores y la disposición del panel se editan en la escena; el ritmo del parpadeo y la lógica del destino se ajustan en el script.

`Win.tscn` muestra `Assets/tierra_salvada.png`, con la Tierra protegida y las naves huyendo. `GameOverScreen.tscn` muestra `Assets/tierra_invasion.png`, una versión de la misma composición bajo ataque. Las imágenes fueron generadas para el proyecto en pixel art; los textos se dibujan por separado, y el CRT se aplica también a ambas pantallas. `Scripts/SecuenciaFinal.cs` controla ambas: deja dos segundos para la ilustración, muestra el puntaje durante cinco segundos y vuelve al menú. En derrota, `GameManager.puntajeFinalDerrota` conserva el valor anterior al último impacto, mientras el puntaje normal sigue reiniciándose al perder cada vida. Los tiempos se ajustan con `SegundosDeImagen` y `SegundosDePuntaje` en ese script.

## 3. Recorrido y estado

`MainMenu → Lvl1 → NivelCompletado → Lvl2 → NivelCompletado → Lvl3 → NivelCompletado → Boss → Win`.

Se comienza con tres vidas. Los enemigos normales valen 100 puntos, los ZigZag 200, los tiradores 300 y el jefe 1.000. Cada vez que se pierde una vida el puntaje vuelve a cero; perder todas las vidas lleva a Game Over. Al iniciar una partida desde el menú o reiniciarla se restauran vidas, puntos y nivel.

`AlienManager.cs` crea y mueve la formación. `QuedanEnemigos()` cuenta únicamente enemigos vivos, no el temporizador. Cuando no queda ninguno, muestra la pantalla de nivel completado. En el nivel del jefe, `NivelBoss.cs` muestra `Win.tscn` en cuanto se destruye la nodriza, aunque sigan vivas sus escoltas.

## 4. Enemigos y animaciones

`Enemigo.cs` contiene tres tipos (`TipoEnemigo`), con su puntaje definido en `Explotar()`:

- **Normal**: sprite verde; se desplaza con la formación. Da 100 puntos.
- **ZigZag**: sprite amarillo; baja y oscila horizontalmente. Da 200 puntos si se destruye. Si llega al fondo, se pierde una vida, sus tripulantes huyen y su nave explota sin dar puntos.
- **Tirador**: sprite rojo; mantiene su posición horizontal y dispara. Da 300 puntos. En el nivel 3 apunta a la posición del jugador en el instante del disparo.

`AlienManager.cs`, en `CentrarYCrearAliens()`, decide los tipos y sus posiciones. La primera fila incluye especiales desde el nivel 1; sus columnas se mezclan al comenzar cada nivel, sin superponerse, y en el nivel 3 hay más tiradores. El temporizador del administrador controla la frecuencia de disparo: 3 segundos en `Lvl1.tscn`, 2,2 en `Lvl2.tscn` y 1,5 en `Lvl3.tscn`. Para cambiarlo, seleccionar `AlienManager/Timer` en cada escena y editar **Wait Time**.

Las poses alternadas están en `Enemigo._Process()`. La nave se inclina al desplazarse y pulsa ligeramente en `Player.cs`; los láseres pulsan en `Laser.cs` y `LaserEnemigo.cs`. Estos efectos transforman el sprite, no cambian su área de colisión.

Los gráficos del jugador y los enemigos están en `Assets/nave.svg`, `Assets/alien_normal.svg`, `Assets/alien_zigzag.svg` y `Assets/alien_tirador.svg`. Para reemplazarlos, conservar proporciones y revisar la escala y el `CollisionShape2D` en sus escenas. El tutorial usa esos mismos archivos.

## 5. Jefe final: la nodriza

La escena del combate es `Escenas/Boss.tscn`. `Scripts/NivelBoss.cs` dirige las fases y las escoltas; `Scripts/Boss.cs` administra los 15 impactos, el destello y la recompensa de 1.000 puntos. Una barra superior muestra la vida restante. La victoria depende de destruir la nodriza, no de eliminar a todas sus escoltas.

El ciclo funciona así:

1. **Entrada:** la nodriza baja desde fuera de la pantalla durante 2,5 segundos con sonidos de aproximación. Durante la entrada se bloquean movimiento, disparo y pausa. Al terminar aparecen diez escoltas desde los laterales.
2. **Órbita:** diez escoltas giran en sentido horario alrededor de la nodriza. Las regeneradas también entran desde fuera y no disparan hasta estar en pantalla. Solo las que están por debajo de ella disparan y lo hacen de una en una, apuntando hacia la posición del jugador al disparar y con una pausa entre disparos. Al perder vida la nodriza, se mueve más rápido y el intervalo entre disparos de órbita baja gradualmente de 1,8 a 1 segundo. Su posición avanza gradualmente hacia el objetivo de movimiento, incluso al cambiar de fase, para evitar saltos.
3. **Escudo con huecos:** cada escolta intercepta físicamente los disparos del jugador. Una escolta destruida deja libre su posición y las demás conservan sus lugares. Cada 15 segundos se regeneran los espacios vacíos durante la fase de órbita; si el temporizador vence durante otra fase, se espera al regreso a la órbita. La nodriza puede recibir daño si un disparo atraviesa el hueco: no tiene invulnerabilidad artificial.
4. **Dispersión:** después de tres vueltas completas, las escoltas se desplazan hacia posiciones alrededor de la línea del jugador y disparan por turnos. Terminado el turno de cada escolta viva, vuelven a orbitar con una orientación y posiciones aleatorias; los huecos permanecen hasta que vence el temporizador de regeneración. Esta secuencia de tres vueltas y dispersión se realiza tres veces.
5. **Rayo maestro:** las escoltas supervivientes se apartan de la trayectoria. La nodriza se alinea con la posición del jugador al comenzar esta fase y fija el objetivo; una franja roja intermitente y un sonido avisan durante dos segundos. Después dispara un rayo que atraviesa los refugios y puede quitar **una sola vida por disparo**, no una vida por fotograma. Tras disparar regresa al centro y empieza otro ciclo con las escoltas que queden.
6. Si se eliminan todas las escoltas antes, se omite la espera de órbitas vacías y se pasa al ataque del rayo; las escoltas podrán regenerarse al volver a la órbita.

Las escoltas no bajan más allá de `limiteDescenso`: la línea punteada representa el límite inferior aproximado de sus sprites, no una barrera para las balas. Sus posiciones se limitan también durante las transiciones. Los valores por defecto se pueden modificar seleccionando el nodo raíz `NivelBoss` en el inspector:

| Propiedad | Valor | Función |
|---|---:|---|
| `cantidadEscoltas` | 10 | Número de enemigos de la formación inicial. |
| `duracionIntroduccion` | 2,5 s | Duración de la bajada inicial y bloqueo de controles. |
| `velocidadOrbita` | 2,1 | Velocidad angular de la órbita, en radianes por segundo. |
| `limiteDescenso` | 430 | Altura máxima del centro de las escoltas; se ajusta si la ventana es pequeña. |
| `vueltasPorTanda` / `tandasPorCiclo` | 3 / 3 | Órbitas antes de dispersarse y dispersiones antes del rayo. |
| `tiempoCarga` / `duracionRayo` | 2 / 1 s | Tiempo de aviso y duración del disparo maestro. |
| `intervaloRegeneracion` | 15 s | Tiempo entre regeneraciones; 0 desactiva la regeneración. |
| `velocidadEntrada` | 320 | Velocidad de las escoltas mientras entran desde fuera de pantalla. |
| `alienEscena` / `laserEnemigoEscena` | escenas existentes | Recursos usados para escoltas y proyectiles. |

Seleccionar el nodo `Jefe` para ajustar `vidaMaxima` (15 por defecto). `Shaders/nodriza.gdshader` recorta visualmente el fondo blanco del JPEG original y colorea la silueta sin cambiar el archivo fuente. El sonido de carga y el disparo del rayo se generan en `Sonidos.gd`.

## 6. Refugios destructibles

`Scripts/Refugios.cs` crea cuatro refugios en cada nivel y en el jefe. Cada uno está formado por bloques `BloqueRefugio` (`Scripts/BloqueRefugio.cs`) con su propia colisión. Cuando un proyectil del jugador o enemigo impacta, el bloque y el proyectil se eliminan; los huecos son locales y no se regenera un bloque durante el mismo nivel. Se crean refugios nuevos al comenzar otro nivel.

Para ajustar cantidad, ancho, altura o tamaño de bloque, editar las constantes `Cantidad`, `Columnas`, `Filas` y `Tamano` en `Refugios.cs`. La forma del arco está definida por las condiciones dentro de los bucles, y el color en `BloqueRefugio.cs`. Si se modifican dimensiones, ajustar también los índices de esas condiciones y comprobar la alineación y las colisiones. `Laser.cs` y `LaserEnemigo.cs` manejan los impactos. Los disparos dirigidos conservan su dirección inicial y no persiguen al jugador; se eliminan al salir por cualquier borde.

## 7. Partículas

`GameManager.crearExplosion(posicion, color, grande)` crea un `CpuParticles2D` de una sola emisión con textura cuadrada. El método conecta `Finished` a `QueueFree` para liberar el efecto. Las explosiones se dibujan en una `CanvasLayer` global, así pueden seguir visibles durante un cambio de escena. `GameManager.mostrarPuntos()` crea un texto flotante en esa misma capa, con el color y el valor del enemigo; una animación lo desplaza, desvanece y elimina.

- `Enemigo.Explotar()` selecciona verde, amarillo o rosa según el tipo.
- `Boss.RecibirGolpe()` genera una explosión blanca más grande al morir.
- `LaserEnemigo.cs` genera partículas celestes al golpear a la nave.

En `crearExplosion` pueden ajustarse cantidad, duración, velocidad, gravedad, tamaño y desvanecimiento. Las partículas no tienen colisión ni modifican el daño.

Al destruir una nave común, `Enemigo.Explotar()` también llama a `GameManager.crearTripulantes()`: aparecen cuatro extraterrestres diminutos del color del enemigo, con cabeza, ojos, brazos y piernas. Al destruir la nodriza aparecen 28. Cuando un amarillo alcanza el fondo, cuatro tripulantes corren hacia los lados y la nave explota 0,55 segundos después. `Scripts/Tripulante.cs` dibuja sus siluetas pixeladas, anima los brazos, aplica gravedad a los que caen, los desvanece y libera cada nodo. Son decorativos, no tienen colisiones ni dan puntos. Las cantidades se ajustan en `crearTripulantes()` y la duración, el dibujo y la gravedad en `Tripulante.cs`.

## 8. Sonido procedural

El proyecto incluye [gdfxr](https://github.com/timothyqiu/gdfxr), de Haoyu Qiu, una adaptación de sfxr para Godot. Su licencia MIT está en `addons/gdfxr/LICENSE`. El plugin del editor está habilitado y permite crear efectos desde el panel gdfxr.

Para los efectos que ya usa el juego, `Sonidos.gd` crea muestras en memoria **una vez al iniciar** con `SFXRGenerator` y mantiene ocho reproductores para sonidos simultáneos. Hay sonidos para pasar el cursor sobre botones, disparos, explosiones, daño, impactos al jefe, aproximación de la nodriza, regeneración de escoltas, carga y rayo maestro, y victoria. Las llamadas desde C# pasan por `GameManager.reproducirSonido(nombre)`.

El sonido `hover` se conecta automáticamente a los botones mediante la señal `node_added`; también funciona con botones creados por código, como los de pausa y tutorial. El menú principal llama además a `registrar_botones()` para asegurar la conexión sin duplicarla. Solo suena en botones visibles y habilitados, y no depende del foco de teclado. Sus parámetros se editan en `_crear("hover", ...)`; su reproductor usa -1 dB y el resto de efectos -8 dB.

En `Sonidos.gd`, `_crear` recibe nombre, forma de onda, frecuencia inicial, pendiente, duración de sustain, decay y un volumen de síntesis opcional. El volumen de reproducción se asigna en `reproducir()`. Los sonidos pueden ajustarse allí o reemplazarse por otros creados con el plugin. El juego todavía no tiene música de fondo.

## 9. Efecto CRT

`Escenas/EfectoCRT.tscn` es una `CanvasLayer` global con un `ColorRect` que cubre la ventana. Está en la capa 110, por encima de las partículas (10) y de la pausa (100), y tiene **Mouse Filter = Ignore** para no bloquear botones. El shader `Shaders/crt.gdshader` lee la pantalla ya dibujada y aplica curvatura, líneas horizontales, oscurecimiento de bordes y una pequeña separación de colores. No afecta las colisiones.

Para ajustarlo: abrir `EfectoCRT.tscn`, seleccionar `Pantalla`, abrir su **Material → Shader Parameters** y modificar:

| Parámetro | Inicial | Efecto |
|---|---:|---|
| `curvatura` | 0,025 | Deforma los bordes; 0 la elimina. |
| `intensidad_lineas` | 0,16 | Intensidad de las líneas horizontales. |
| `intensidad_vineta` | 0,20 | Oscurecimiento hacia las esquinas. |
| `separacion_rgb` | 0,5 | Desplazamiento de colores en píxeles. |

Los valores predeterminados están en el shader; cambiar un parámetro en el material permite guardar una variación en la escena. Para apagar todo el efecto, desactivar `EfectoCRT` en **Proyecto → Configuración del proyecto → Autoload**. Volver a activarlo para recuperarlo. Como es un efecto de pantalla completa, la curvatura también deforma el HUD y los menús; una intensidad alta puede afectar la legibilidad.

Referencia técnica: [shaders que leen la pantalla, documentación de Godot](https://docs.godotengine.org/en/stable/tutorials/shaders/screen-reading_shaders.html).

## 10. Puntaje, fondo y tipografía

El fondo negro se configura en `project.godot`, en `rendering/environment/defaults/default_clear_color`. El marcador está anclado arriba a la derecha en `Escenas/Interfaz.tscn`; `Scripts/Interfaz.cs` lo actualiza junto con las vidas a partir de `GameManager`. El valor se guarda en `GameManager.puntos`, se incrementa con `sumarPuntos()` y se reinicia en `perderVida()` cada vez que se pierde una vida, incluso si quedan vidas disponibles.

El valor de cada enemigo se define en `Enemigo.Explotar()`, que también crea las partículas y el número flotante. El jefe concede 1.000 puntos en `Boss.RecibirGolpe()` al morir. Un láser solo activa una baja una vez. Si se quieren otros valores, modificar esos métodos y mantener este documento actualizado.

La tipografía Pixelify Sans se incluye localmente en `Assets/Fonts/PixelifySans.ttf` y se usa como fuente predeterminada del proyecto mediante `gui/theme/custom_font`; no necesita conexión al jugar. Proviene de [Google Fonts](https://github.com/google/fonts/tree/main/ofl/pixelifysans), y su licencia SIL Open Font License está en `Assets/Fonts/PixelifySans-OFL.txt`.

## 11. Pruebas recomendadas

1. Iniciar con F5, revisar el título animado, el parpadeo y la demo silenciosa; entrar al tutorial y comprobar sus botones con el mouse.
2. Disparar contra los refugios desde ambos lados y confirmar que cada impacto quite solo una parte. Comprobar que se renuevan en el siguiente nivel.
3. Verificar los tres tipos de enemigos y sus disparos en cada nivel.
4. Avanzar hasta el jefe, comprobar la órbita, los huecos, las tres dispersiones, la regeneración, los disparos apuntados, el aviso del rayo y que la victoria se produzca al destruir la nodriza, incluso con escoltas vivas.
5. Pausar, reanudar, perder todas las vidas y verificar el puntaje final y el regreso automático al menú.
6. Confirmar que cada tipo muestra su valor flotante y que el marcador se reinicia al perder una vida.
7. Escuchar los efectos y revisar intensidad del CRT, legibilidad y rendimiento en la PC de destino.

Las pruebas automáticas realizadas durante los cambios comprueban compilación, escenas, progresión, colisiones de refugios y generación de partículas/sonidos; no sustituyen una partida completa ni una revisión auditiva y visual.

## 12. Conceptos de Godot para leer el proyecto

- **Escena (`.tscn`)**: archivo que guarda un árbol de nodos y sus propiedades. Por ejemplo, `player.tscn` reúne la nave, su sprite y su colisión.
- **Nodo**: elemento del árbol. `Node2D` tiene posición 2D; `Area2D` detecta áreas; `CharacterBody2D` se usa para la nave; `CanvasLayer` dibuja interfaz en una capa.
- **Script**: código asociado a un nodo. `_Ready()` se ejecuta al entrar al árbol; `_Process(delta)` actualiza por fotograma y `_PhysicsProcess(delta)` se usa para el movimiento físico. `delta` es tiempo transcurrido, por eso se multiplica por la velocidad.
- **Señal**: aviso de un nodo, como `Timer.timeout` o `Area2D.area_entered`. En las escenas de los láseres y niveles hay conexiones a los métodos que responden a esas señales.
- **Autoload**: nodo global que Godot crea al iniciar y mantiene entre escenas. Aquí son `Sonidos`, `GameManager`, `PauseMenu` y `EfectoCRT`.
- **`[Export]`**: expone una propiedad C# en el Inspector. **`PackedScene`** es una escena que se puede instanciar desde código.
- **`QueueFree()`**: solicita que Godot elimine un nodo al final del fotograma. `IsQueuedForDeletion()` ayuda a evitar procesar dos veces un objeto ya marcado para borrar.

Árbol simplificado de los niveles normales:

```text
Lvl1 / Lvl2 / Lvl3
├── CharacterBody2D    instancia de player.tscn y su laserEscena
├── AlienManager      crea y mueve los enemigos
│   └── Timer         emite timeout para los disparos enemigos
├── Refugios          genera bloques destructibles
└── Interfaz          muestra puntos y vidas
```

`Boss.tscn` usa otro árbol: la raíz `NivelBoss` dirige `Player`, `Formacion`, `Jefe`, `Refugios` e `Interfaz`. No usa la formación normal para controlar a las escoltas.

**Si renombrás un nodo o movés un archivo**, revisá las rutas usadas en `GetNode(...)`, las referencias de recursos, las conexiones de señales y las llamadas `ChangeSceneToFile(...)`. Una escena puede abrir en el editor y aun así fallar en ejecución si una ruta dejó de coincidir.

### Valores iniciales de los niveles

| Escena | Filas × columnas | Separación H/V | Velocidad de formación | Timer de disparo |
|---|---:|---:|---:|---:|
| `Lvl1.tscn` | 3 × 8 | 80 / 80 | 80 | 3 s |
| `Lvl2.tscn` | 4 × 8 | 80 / 80 | 110 | 2,2 s |
| `Lvl3.tscn` | 5 × 8 | 80 / 80 | 80 | 1,5 s |

Los valores no escritos explícitamente en la escena provienen de las propiedades exportadas de `AlienManager.cs`. Al crear la primera fila se eligen dos posiciones para amarillos; en niveles 1 y 2 se eligen otras dos para tiradores, mientras que en el nivel 3 las demás posiciones de esa fila son tiradores. La selección de columnas es aleatoria y no se superpone. Las demás filas son normales.

## 13. Seguir una acción por el código

### Del disparo del jugador a los puntos

1. `Player._PhysicsProcess()` detecta `ui_accept` y llama a `Disparar()`. El jugador conserva la referencia `laserActual` y no dispara otro mientras el anterior siga existiendo.
2. `Player.Disparar()` instancia la escena `laserEscena` asignada en el nivel. `Laser._Process()` la mueve hacia arriba y la elimina si sale de pantalla.
3. La señal `area_entered` de `Laser.tscn` llama a `Laser._on_area_entered()`. Según el objeto, rompe un `BloqueRefugio`, llama a `Enemigo.Explotar()` o llama a `Boss.RecibirGolpe()`.
4. En una baja común, `Enemigo.Explotar()` asigna puntos y color y llama al `GameManager` para sumar, mostrar el texto flotante, crear partículas y tripulantes y reproducir el sonido. Finalmente elimina al enemigo.
5. `Interfaz._Process()` lee `GameManager.puntos` y `vidas` y actualiza los dos textos del HUD.

La bandera `hasHit` impide que un solo láser del jugador aplique varios impactos. Si añadís otro objeto que deba recibir disparos, revisá esa función y sus capas/máscaras de colisión.

### Del disparo enemigo a la pérdida de vida

1. `AlienManager/Timer` emite `timeout`; `_on_timer_timeout()` elige un tirador vivo y crea `LaserEnemigo.tscn`. En el nivel 3 llama a `ApuntarA()` con la posición del jugador; en 1 y 2 la bala baja recta. La nodriza también llama a `ApuntarA()`.
2. `LaserEnemigo.cs` mueve la bala en la dirección fijada al disparar. No persigue al jugador después; se elimina al salir por cualquier borde.
3. Las señales `area_entered` y `body_entered` llaman a `VerificarImpacto()`: un `BloqueRefugio` se rompe; un `Player` pierde una vida mediante `GameManager.perderVida()`.
4. `yaColisiono` evita que un mismo proyectil quite más de una vida. No existe una invulnerabilidad general después de recibir un golpe, así que proyectiles distintos muy cercanos pueden quitar varias vidas.

El jugador está en el grupo `Jugador` de `player.tscn`, que usa el administrador para encontrarlo al apuntar. Los proyectiles no deben confundirse con las partículas: las partículas y los pequeños tripulantes son decorativos y no tienen colisiones.

## 14. Recetas de modificación

### Hacer que la nave sea más rápida

1. Abrí `Escenas/player.tscn` y seleccioná el nodo raíz `CharacterBody2D`.
2. En el Inspector, buscá `velocidad`: su valor base en `Player.cs` es `350` píxeles por segundo. Probá con `400` y guardá.
3. Ejecutá F5, iniciá una partida y probá los dos bordes de la pantalla. Si solo querés cambiar un nivel, podés ajustar la propiedad en la instancia del jugador de ese nivel en lugar de modificar la escena reutilizable.
4. `margenBorde`, cuyo valor base es `30`, limita el centro de la nave. Si reemplazás el sprite o cambiás su escala, revisá también ese margen y `CollisionShape2D`.

El comentario de `Player.cs` menciona A/D, pero el código lee `ui_left` y `ui_right`, y `project.godot` no define un mapeo A/D propio. Para agregarlo configurá las acciones en **Proyecto → Configuración del proyecto → Mapa de entradas**, revisá el código y actualizá el tutorial.

### Modificar la dificultad de un nivel

1. Abrí, por ejemplo, `Escenas/Lvl2.tscn` y seleccioná `AlienManager`.
2. Ajustá `filas`, `columnas`, `espacioH`, `espacioV` o `velocidad` en el Inspector. Recordá que el ancho de la formación depende de columnas y separación.
3. Para cambiar la cadencia de los tiradores, seleccioná el hijo `Timer` y editá **Wait Time**: un valor menor produce disparos más frecuentes. El temporizador está conectado a `_on_timer_timeout()`.
4. Para cambiar cuántos amarillos o tiradores aparecen, editá `cantidadAmarillos` y `cantidadTiradores` dentro de `AlienManager.CentrarYCrearAliens()`. Las columnas se mezclan antes de asignar los tipos.
5. Guardá y probá desde F5. Si usás F6 en una escena aislada, el `nivelActual` del `GameManager` puede no corresponder a ese archivo.

La formación baja 20 píxeles al cambiar de dirección. Si `AlienManager.Position.Y` llega a `600.0f`, se pierde una vida y vuelve a su posición inicial. El amarillo desciende de forma independiente a `55.0f` y llega al fondo cuando su base alcanza `tamanoPantalla.Y - 50`; ambos valores están en sus respectivos scripts, no en el Inspector del nivel.

### Cambiar puntos o vidas

- En `Enemigo.Explotar()`, modificá los tres números del `switch` de `puntos`. El mismo valor se usa para sumar y para el texto flotante.
- En `Boss.RecibirGolpe()`, el premio aparece dos veces: en `sumarPuntos(1000)` y en `mostrarPuntos(..., 1000, ...)`. Cambiá los dos juntos para que no difieran.
- `GameManager.perderVida()` resta una vida y pone el puntaje actual en cero. Antes de perder la última conserva el valor en `puntajeFinalDerrota`. Si cambiás esa regla, revisá `SecuenciaFinal.cs` y probá tanto derrota como victoria.
- El valor inicial de 3 también se restablece explícitamente al iniciar una partida, al usar el botón de prueba del jefe, al volver al menú desde pausa y al terminar la secuencia final. Revisá `MainMenu.cs`, `PauseMenu.cs` y `SecuenciaFinal.cs` si decidís cambiarlo.

### Ajustar el jefe sin tocar la progresión

1. Abrí `Escenas/Boss.tscn` y seleccioná la raíz `NivelBoss`. Por ejemplo, cambiá `intervaloRegeneracion` de `15` a `20` para dar más tiempo entre reposiciones, o a `0` para desactivarlas.
2. Seleccioná `Jefe` para ajustar `vidaMaxima`, cuyo valor base es `15`. Guardá la escena.
3. En el juego usá **Esc → PROBAR JEFE FINAL**. Comprobá la entrada, los 10 minions, los huecos y la regeneración, la dispersión y el rayo antes de dar por terminado el ajuste.
4. Si cambiás el radio de órbita en `ActualizarOrbita()` de `NivelBoss.cs`, revisá también los radios usados en `CrearEscolta()` para calcular el lado y la altura de entrada. Si cambiás el movimiento de la nodriza, revisá sus transiciones de fase para no reintroducir saltos.

### Ajustar los refugios sin desalinear el dibujo

Las constantes `Cantidad`, `Columnas`, `Filas` y `Tamano` están en `Refugios.cs`. La fórmula `(columna - 5.5f) * Tamano` centra la cuadrícula actual de 12 columnas; las condiciones del arco también usan índices fijos. Si cambiás columnas o tamaño, recalculá el centro y las condiciones. La colisión se crea con `Tamano`, pero el dibujo de `BloqueRefugio._Draw()` usa un rectángulo fijo de 8 × 8: actualizalo también para que coincidan. Probá impactos de balas de ambos bandos y el rayo del jefe.

### Cambiar un gráfico sin romper la colisión

1. Importá el archivo nuevo dentro de `Assets/` y esperá a que Godot lo procese.
2. Abrí la escena correspondiente, seleccioná su `Sprite2D` y reemplazá **Texture** en el Inspector.
3. Ajustá escala y `CollisionShape2D`; cambiar la imagen no ajusta automáticamente la colisión.
4. En el caso del jefe, revisá además `Shaders/nodriza.gdshader`: su recorte se calculó para la imagen actual. El tutorial referencia los SVG de enemigos en `Tutorial.cs`, así que puede necesitar actualización si cambian los archivos.

### Cambiar tiempos y apariencia de los finales

`Scripts/SecuenciaFinal.cs` es la base de `Win.cs` y `GameOverScreen.cs`. `SegundosDeImagen = 2.0f` determina cuánto se ve la ilustración antes del marcador; `SegundosDePuntaje = 5.0f` determina cuánto se ve el resultado antes de volver al menú. Para cambiar imágenes, editá el `TextureRect/Fondo` de `Win.tscn` o `GameOverScreen.tscn`. Las instrucciones y puntajes son nodos de texto separados de la ilustración.

## 15. Problemas frecuentes y cómo diagnosticar

| Síntoma | Qué revisar |
|---|---|
| Godot no reconoce los scripts C# | Abrí el proyecto con la edición **.NET** de Godot y verificá que esté instalado el **SDK** de .NET 8, no solamente el runtime. |
| Cambié un `.cs` y no veo el cambio | Guardá, compilá otra vez y ejecutá. Revisá si la escena tiene un valor exportado sobrescrito en el Inspector. |
| Aparece un error de nodo o recurso inexistente | Comprobá nombre y ruta `res://`, archivo incluido y conexiones. Esperá a que el editor complete la importación de recursos. |
| F6 en nivel 2 o 3 muestra una composición o progresión diferente | F6 no actualiza `GameManager.nivelActual`. Probá el recorrido desde F5 o prepará el estado para esa prueba. |
| La nave no dispara | Revisá `laserEscena` en la instancia del jugador y la existencia de la escena del láser. Solo se permite un disparo del jugador a la vez mientras siga en pantalla. |
| Una bala atraviesa algo o choca con el objeto equivocado | Revisá `CollisionShape2D`, capas/máscaras y señales de la escena del láser; después revisá las comprobaciones de tipo en el script. |
| El jefe no deja moverse ni pausar al comenzar | Es normal durante los 2,5 segundos de introducción. Después deben entrar los minions y recuperarse los controles. |
| El fondo del menú cambia al editar el nivel 2 | La demo instancia esa escena como fondo visual. Revisá `DemoArcade.cs` además de `Lvl2.tscn`. |
| No se escucha un efecto | Comprobá la salida y volumen de la computadora, el nombre pasado a `reproducirSonido()` y la definición correspondiente en `Sonidos.gd`. La demo del menú es silenciosa a propósito. |
| El CRT dificulta leer o usar la interfaz | Ajustá `curvatura`, `intensidad_lineas`, `intensidad_vineta` y `separacion_rgb`, o desactivá temporalmente el autoload `EfectoCRT`. |
| Falta una plantilla al exportar | Instalá la plantilla de exportación que corresponda a **Godot 4.7.2 .NET** y volvé a exportar. |

## 16. Entregar el proyecto y comprobar cambios

Para compartir el **código fuente**, enviá `project.godot`, `SpaceInvaders.csproj`, `SpaceInvaders.sln`, `export_presets.cfg` y las carpetas `Escenas`, `Scripts`, `Assets`, `Shaders`, `addons` y `docs`, con sus archivos asociados `.uid` y `.import`. Las fuentes están incluidas en `Assets/Fonts`; las licencias y notas incluidas con fuentes y complementos deben acompañarlos. La carpeta `.godot/` contiene caché generada, está ignorada por Git y no sustituye a los archivos fuente.

Para generar un ejecutable, instalá las plantillas **4.7.2 .NET**, elegí el preset Windows Desktop en **Proyecto → Exportar**, seleccioná una ruta y exportá. Probá el resultado en otra computadora: tener un preset configurado no garantiza que ya se haya verificado un ejecutable. Antes de publicar o redistribuir, revisá las condiciones de las fuentes y la procedencia de los demás recursos gráficos.

Después de modificar una mecánica, usá la lista de la sección **11. Pruebas recomendadas**. Durante el desarrollo se hicieron comprobaciones automáticas de compilación, progresión, colisiones, fases y efectos, pero no hay una batería de pruebas automatizadas permanente en el repositorio. Por eso también hace falta una partida manual, en especial para valorar dificultad, sonidos, rendimiento y apariencia.
