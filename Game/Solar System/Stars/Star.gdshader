shader_type spatial;
render_mode unshaded;

instance uniform float Glow_Power = 3;
uniform vec3 Sun_Color : source_color;

uniform sampler2D emission_noise;
uniform float waveSpeed : hint_range(0,1) = 0.1;
uniform float fresnel : hint_range(0,2) = 1.;

uniform float scale : hint_range(0,2) = 0.1;
uniform float blendSharpness : hint_range(0,2) = 0.;

// TRIPLANAR FUNCTION
vec4 triplanar_texture(vec3 position, vec3 normal, vec2 offset, sampler2D noise) {
	vec4 colX = texture(noise, position.zy * scale + offset);
	vec4 colY = texture(noise, position.xz * scale + offset);
	vec4 colZ = texture(noise, position.xy * scale + offset);
	
	vec3 blendWeight = abs(normal);
	blendWeight = vec3(pow(blendWeight.x, blendSharpness), pow(blendWeight.y, blendSharpness), pow(blendWeight.z, blendSharpness));
	blendWeight /= (blendWeight.x + blendWeight.y + blendWeight.z);
	
	return colX * blendWeight.x + colY * blendWeight.y + colZ * blendWeight.z;
}

void fragment() {
	// Fresnel
	float fresnel_out = pow(fresnel - clamp(dot(NORMAL, VIEW), 0., fresnel), fresnel);

	vec2 waveOffsetA = vec2(TIME * waveSpeed, TIME * waveSpeed * 0.8);
	vec2 waveOffsetB = vec2(TIME * waveSpeed * - 0.8, TIME * waveSpeed * -0.3);
	vec2 result_offset = waveOffsetA + waveOffsetB;

	vec3 fragPos = (INV_VIEW_MATRIX * vec4(VERTEX, 1.)).xyz - NODE_POSITION_WORLD;
	vec3 fragNormal = (INV_VIEW_MATRIX * vec4(NORMAL, 0.)).xyz;

	vec3 noise_tex = triplanar_texture(fragPos, fragNormal, result_offset, emission_noise).rgb;

	EMISSION = vec3(fresnel_out) * noise_tex * Sun_Color * Glow_Power;
	ALBEDO = EMISSION * noise_tex * 1.5;
}