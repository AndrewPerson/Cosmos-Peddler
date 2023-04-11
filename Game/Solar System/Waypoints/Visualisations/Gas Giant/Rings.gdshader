shader_type spatial;
render_mode cull_disabled;

#include "res://Shader Utils/fbm.gdshaderinc"

uniform float time_speed : hint_range(0, 1) = 0.01;
uniform float ring_width : hint_range(0, .15) = 0.1;
uniform sampler2D colorscheme;

instance uniform float seed: hint_range(1, 10) = 1;

vec2 polar_coordinates(vec2 uv)
{
	float radius = length(uv);
	float angle = atan(uv.y, uv.x) / (PI * 2.0);
	return vec2(radius, angle);
}

void fragment() {
	// center is used to determine ring position
	vec2 uv_center = UV - vec2(.5);
	
	// convert to polar coords
	vec2 polarUV = polar_coordinates(uv_center);
	
	// cut out 2 circles of different sizes and only intersection of the 2.
	float ring = smoothstep(0.5-ring_width*2.0, 0.5-ring_width, polarUV.x);
	ring *= smoothstep(polarUV.x-ring_width, polarUV.x, 0.4);

	// rotate material in the ring
	polarUV += vec2(TIME * time_speed);
	
	// some noise
	ring *= fbm(polarUV*15., 4, seed);
	
	vec4 col = texture(colorscheme, vec2(fbm(vec2(polarUV.x * 30., 0.), 4, seed), 0.));
	float ring_a = step(0.28, ring);
	ALBEDO = col.rgb;
	ALPHA = ring_a * col.a;
}