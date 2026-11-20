shader_type canvas_item;
render_mode unshaded;

// Lee la imagen ya dibujada por Godot, incluidos el HUD y la pausa.
uniform sampler2D pantalla : hint_screen_texture, repeat_disable, filter_linear;
uniform float curvatura : hint_range(0.0, 0.12, 0.005) = 0.025;
uniform float intensidad_lineas : hint_range(0.0, 0.5, 0.01) = 0.16;
uniform float intensidad_vineta : hint_range(0.0, 1.0, 0.01) = 0.20;
uniform float separacion_rgb : hint_range(0.0, 2.0, 0.1) = 0.5;

void fragment() {
    vec2 centro = SCREEN_UV * 2.0 - 1.0;
    vec2 uv = (centro * (1.0 + curvatura * dot(centro, centro))) * 0.5 + 0.5;
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) {
        COLOR = vec4(0.0, 0.0, 0.0, 1.0);
    } else {
        vec2 desplazamiento = vec2(SCREEN_PIXEL_SIZE.x * separacion_rgb, 0.0);
        float rojo = texture(pantalla, uv + desplazamiento).r;
        float verde = texture(pantalla, uv).g;
        float azul = texture(pantalla, uv - desplazamiento).b;
        float lineas = 1.0 - intensidad_lineas * (0.5 + 0.5 * sin(FRAGCOORD.y * 1.5707963));
        float vineta = 1.0 - intensidad_vineta * smoothstep(0.15, 1.4, dot(centro, centro));
        COLOR = vec4(vec3(rojo, verde, azul) * lineas * vineta, 1.0);
    }
}
