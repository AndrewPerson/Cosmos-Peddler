shader_type spatial;
render_mode unshaded;

uniform vec3 Sun_Color : source_color;

uniform float waveSpeed : hint_range(0,1) = 0.1;
uniform float fresnel : hint_range(0,2) = 1.;

instance uniform float seed : hint_range(1, 10) = 1;
instance uniform float Glow_Power = 3;

const float size = 30.;
const int octaves = 4;

float rand(vec2 coord) {
	// land has to be tiled (or the contintents on this planet have to be changing very fast)
	// tiling only works for integer values, thus the rounding
	// it would probably be better to only allow integer sizes
	// multiply by vec2(2,1) to simulate planet having another side
	coord = mod(coord, vec2(2.,1.) * size);
	return fract(sin(dot(coord.xy, vec2(12.9898,78.233))) * 15.5453 * seed);
}

float noise(vec2 coord){
	vec2 i = floor(coord);
	vec2 f = fract(coord);
		
	float a = rand(i);
	float b = rand(i + vec2(1., 0.));
	float c = rand(i + vec2(0., 1.));
	float d = rand(i + vec2(1., 1.));

	vec2 cubic = f * f * (3. - 2. * f);

	return mix(a, b, cubic.x) + (c - a) * cubic.y * (1. - cubic.x) + (d - b) * cubic.x * cubic.y;
}

float fbm(vec2 coord){
	float value = 0.;
	float scale = 0.5;

	for(int i = 0; i < octaves ; i++){
		value += noise(coord) * scale;
		coord *= 2.;
		scale *= 0.5;
	}

	return value;
}

float height(vec2 uv) {
	vec2 base_fbm_uv = uv * float(size);
	
	// use multiple fbm's at different places so we can determine what color land gets
	float fbm1 = fbm(base_fbm_uv);
	float fbm2 = fbm(base_fbm_uv - fbm1);
	float fbm3 = fbm(base_fbm_uv - 1.5 * fbm1);
	float fbm4 = fbm(base_fbm_uv - 2. * fbm1);
	
	float river_fbm = fbm(base_fbm_uv + fbm1 * 6.);
	
	float height = clamp((fbm1 + fbm2 + fbm3 + fbm4 - river_fbm) / 4., 0.01, 1.);
	
	return height;
}

float triplanar_height(vec3 uv) {	
	float heightX = height(uv.yz);
	float heightY = height(uv.xz + vec2(0.1));
	float heightZ = height(uv.xy + vec2(0.2));

	vec3 blend_weight = pow(abs(uv), vec3(1.));
	blend_weight /= blend_weight.x + blend_weight.y + blend_weight.z;

	float height = blend_weight.x * heightX + blend_weight.y * heightY + blend_weight.z * heightZ;
	
	return height;
}

void fragment() {
	// Fresnel
	float fresnel_out = pow(fresnel - clamp(dot(NORMAL, VIEW), 0., fresnel), fresnel);

	vec2 waveOffsetA = vec2(TIME * waveSpeed, TIME * waveSpeed * 0.8);
	vec2 waveOffsetB = vec2(TIME * waveSpeed * - 0.8, TIME * waveSpeed * -0.3);
	vec2 result_offset = waveOffsetA + waveOffsetB;

	vec3 fragPos = (INV_VIEW_MATRIX * vec4(VERTEX, 1.)).xyz - NODE_POSITION_WORLD;
	vec3 fragNormal = (INV_VIEW_MATRIX * vec4(NORMAL, 0.)).xyz;

	float noise_tex = triplanar_height(fragPos + vec3(TIME * waveSpeed));

	EMISSION = vec3(fresnel_out) * noise_tex * Sun_Color * Glow_Power;
	ALBEDO = EMISSION * noise_tex * 1.5;
}