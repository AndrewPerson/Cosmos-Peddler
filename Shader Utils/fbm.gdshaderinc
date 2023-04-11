float rand(vec2 coord, float seed) {
	return fract(sin(dot(coord.xy, vec2(12.9898,78.233))) * 15.5453 * seed);
}

float noise(vec2 coord, float seed){
	vec2 i = floor(coord);
	vec2 f = fract(coord);
		
	float a = rand(i, seed);
	float b = rand(i + vec2(1., 0.), seed);
	float c = rand(i + vec2(0., 1.), seed);
	float d = rand(i + vec2(1., 1.), seed);

	vec2 cubic = f * f * (3. - 2. * f);

	return mix(a, b, cubic.x) + (c - a) * cubic.y * (1. - cubic.x) + (d - b) * cubic.x * cubic.y;
}

float fbm(vec2 coord, int octaves, float seed){
	float value = 0.;
	float scale = 0.5;

	for(int i = 0; i < octaves; i++){
		value += noise(coord, seed) * scale;
		coord *= 2.;
		scale *= 0.5;
	}

	return value;
}
