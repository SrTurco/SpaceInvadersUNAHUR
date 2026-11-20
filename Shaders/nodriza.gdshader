shader_type canvas_item;
// Recorta el fondo blanco de la imagen original sin modificar el archivo fuente.
void fragment() {
    vec2 puntos[40] = vec2[40](
        vec2(.30,.00), vec2(.70,.00), vec2(.70,.13), vec2(.82,.13),
        vec2(.82,.26), vec2(.88,.26), vec2(.88,.39), vec2(.94,.39),
        vec2(.94,.52), vec2(1.,.52), vec2(1.,.73), vec2(.88,.73),
        vec2(.88,.86), vec2(.82,.86), vec2(.82,1.), vec2(.72,1.),
        vec2(.72,.86), vec2(.66,.86), vec2(.66,.73), vec2(.57,.73),
        vec2(.57,.86), vec2(.43,.86), vec2(.43,.73), vec2(.34,.73),
        vec2(.34,.86), vec2(.28,.86), vec2(.28,1.), vec2(.18,1.),
        vec2(.18,.86), vec2(.12,.86), vec2(.12,.73), vec2(0.,.73),
        vec2(0.,.52), vec2(.06,.52), vec2(.06,.39), vec2(.12,.39),
        vec2(.12,.26), vec2(.18,.26), vec2(.18,.13), vec2(.30,.13)
    );
    bool dentro = false;
    int anterior = 39;
    for (int i = 0; i < 40; i++) {
        vec2 a = puntos[i];
        vec2 b = puntos[anterior];
        if ((a.y > UV.y) != (b.y > UV.y)) {
            float cruce = (b.x - a.x) * (UV.y - a.y) / (b.y - a.y) + a.x;
            if (UV.x < cruce) dentro = !dentro;
        }
        anterior = i;
    }
    vec4 original = texture(TEXTURE, UV);
    float brillo = dot(original.rgb, vec3(.299, .587, .114));
    vec3 color_arcade = mix(vec3(.04, .52, .50), vec3(.90, 1.0, .96), brillo);
    COLOR = vec4(color_arcade, dentro ? original.a : 0.0) * COLOR;
}
