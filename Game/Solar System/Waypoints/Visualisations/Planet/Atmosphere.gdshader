shader_type fog;

uniform float atmosphereDensity = 1.;
uniform sampler2D atmosphereColourGrad;

void fog() {
	vec4 colour = texture(atmosphereColourGrad, vec2(length((WORLD_POSITION - OBJECT_POSITION) / EXTENTS), 0.));
	
	DENSITY = atmosphereDensity * colour.a;
	
	ALBEDO = colour.rgb;
	EMISSION = colour.rgb * 3.;
}
