shader_type spatial;

#include "res://Shader Utils/fbm.gdshaderinc"

uniform sampler2D colourGrad;
uniform float time_scale = 0.01;

instance uniform float seed: hint_range(1, 10) = 1;

const float size = 100.;
const int OCTAVES = 5;

void fragment() {
	vec3 fragPos = (INV_VIEW_MATRIX * vec4(VERTEX, 1.)).xyz - NODE_POSITION_WORLD;

	float height = noise(
		vec2(
			fragPos.y * 20. +
			noise(
				(fragPos.xz + vec2(0.5)) * 15.,
			seed) * .5 +
			TIME * time_scale
		),
		seed
	);

	ALBEDO = texture(colourGrad, vec2(height, 0.)).xyz;
}
