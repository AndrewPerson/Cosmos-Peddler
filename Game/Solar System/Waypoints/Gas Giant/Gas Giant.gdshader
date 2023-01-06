shader_type spatial;

uniform sampler2D colourGrad;
uniform float time_scale = 0.01;

instance uniform float seed: hint_range(1, 10) = 1;

const float size = 100.;
const int OCTAVES = 5;

float rand(vec2 coord) {
	coord = mod(coord, vec2(2.0,1.0)*size);
	return fract(sin(dot(coord.xy ,vec2(12.9898,78.233))) * 15.5453 * seed);
}

float noise(vec2 coord){
	vec2 i = floor(coord);
	vec2 f = fract(coord);
	
	float a = rand(i);
	float b = rand(i + vec2(1.0, 0.0));
	float c = rand(i + vec2(0.0, 1.0));
	float d = rand(i + vec2(1.0, 1.0));

	vec2 cubic = f * f * (3.0 - 2.0 * f);

	return mix(a, b, cubic.x) + (c - a) * cubic.y * (1.0 - cubic.x) + (d - b) * cubic.x * cubic.y;
}

float fbm(vec2 coord){
	float value = 0.0;
	float scale = 0.5;

	for(int i = 0; i < OCTAVES ; i++){
		value += noise(coord) * scale;
		coord *= 2.0;
		scale *= 0.55;
	}
	return value;
}

void fragment() {
	vec3 fragPos = (INV_VIEW_MATRIX * vec4(VERTEX, 1.)).xyz - NODE_POSITION_WORLD;

	float height = noise(vec2(fragPos.y * 20. + noise(fragPos.xz * 15.) * .5 + TIME * time_scale));

	ALBEDO = texture(colourGrad, vec2(height, 0.)).xyz;
}
