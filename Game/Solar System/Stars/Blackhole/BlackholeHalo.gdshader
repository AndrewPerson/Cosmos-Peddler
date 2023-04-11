shader_type spatial;

#include "res://Shader Utils/fbm.gdshaderinc"

uniform sampler2D colours;

instance uniform float seed : hint_range(1, 10) = 1;
instance uniform float emission_strength = 2.;

vec2 polar_coordinates(vec2 uv)
{
	float radius = length(uv) * 2.0;
	float angle = atan(uv.y, uv.x) / (PI * 2.0);
	return vec2(radius, angle);
}

void fragment() {
	ROUGHNESS = 1.;
	METALLIC = 0.;
	
	float dist = cos(dot(NORMAL, VIEW)) - .5;
	
	float height = fbm(polar_coordinates(NORMAL.xy - VIEW.xy).xy * 8. + vec2(TIME), 5, seed);
	
	if (dist < height) {
		vec3 colour = texture(colours, vec2(height, 0.)).rgb;
		ALBEDO = colour;
		EMISSION = colour * emission_strength;
	}
	else {
		discard;
	}
}
