shader_type spatial;

#include "res://Shader Utils/fbm.gdshaderinc"

uniform vec3 sun_colour : source_color;
uniform float wave_speed : hint_range(0,1) = 0.1;
uniform float fresnel : hint_range(0,2) = 1.;
uniform float emission_strength = 10;

instance uniform float seed : hint_range(1, 10) = 1;

float height(vec2 uv) {
	vec2 base_fbm_uv = uv * 30.;
	
	// use multiple fbm's at different places so we can determine what color land gets
	float fbm1 = fbm(base_fbm_uv, 4, seed);
	float fbm2 = fbm(base_fbm_uv - fbm1, 4, seed);
	float fbm3 = fbm(base_fbm_uv - 1.5 * fbm1, 4, seed);
	float fbm4 = fbm(base_fbm_uv - 2. * fbm1, 4, seed);
	
	float height = clamp((fbm1 + fbm2 + fbm3 + fbm4) / 4., 0.01, 1.);
	
	return height;
}

float triplanar_height(vec3 uv) {	
	float height_x = height(uv.yz);
	float height_y = height(uv.xz + vec2(0.1));
	float height_z = height(uv.xy + vec2(0.2));

	vec3 blend_weight = pow(abs(uv), vec3(1.));
	blend_weight /= blend_weight.x + blend_weight.y + blend_weight.z;

	float height = blend_weight.x * height_x + blend_weight.y * height_y + blend_weight.z * height_z;
	
	return height;
}

void fragment() {
	// Fresnel
	float fresnel_out = pow(fresnel - clamp(dot(NORMAL, VIEW), 0., fresnel), fresnel);

	vec2 wave_offset_a = vec2(TIME * wave_speed, TIME * wave_speed * 0.8);
	vec2 wave_offset_b = vec2(TIME * wave_speed * - 0.8, TIME * wave_speed * -0.3);
	vec2 result_offset = wave_offset_a + wave_offset_b;

	vec3 frag_pos = (INV_VIEW_MATRIX * vec4(VERTEX, 1.)).xyz - NODE_POSITION_WORLD;
	vec3 frag_normal = (INV_VIEW_MATRIX * vec4(NORMAL, 0.)).xyz;

	float noise_tex = triplanar_height(frag_pos + vec3(TIME * wave_speed));

	EMISSION = vec3(fresnel_out) * noise_tex * sun_colour * emission_strength;
	ALBEDO = EMISSION * noise_tex * 1.5;
}