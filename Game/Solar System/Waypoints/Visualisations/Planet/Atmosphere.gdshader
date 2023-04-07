shader_type fog;

uniform float atmosphere_density = 1.;
uniform sampler2D atmosphere_colour_grad;

void fog() {
	vec4 colour = texture(atmosphere_colour_grad, vec2(length((WORLD_POSITION - OBJECT_POSITION) / SIZE), 0.));
	
	DENSITY = atmosphere_density * colour.a;
	
	ALBEDO = colour.rgb;
	EMISSION = colour.rgb * 3.;
}
