shader_type spatial;

const int size = 5;
const int octaves = 5;

instance uniform float seed = 1.;
instance uniform float rotation;

float rand(vec2 coord) {
	coord = mod(coord, vec2(2.,1.) * float(size));
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

vec3 triplanar_texture(vec3 position, vec3 normal, float blendSharpness) {
	vec3 colX = vec3(height(position.zy));
	vec3 colY = vec3(height(position.xz + vec2(.1)));
	vec3 colZ = vec3(height(position.yx + vec2(.2)));
	
	vec3 blendWeight = abs(normal);
	blendWeight = vec3(pow(blendWeight.x, blendSharpness), pow(blendWeight.y, blendSharpness), pow(blendWeight.z, blendSharpness));
	blendWeight /= (blendWeight.x + blendWeight.y + blendWeight.z);
	
	return colX * blendWeight.x + colY * blendWeight.y + colZ * blendWeight.z;
}


vec3 rotate(vec3 v, vec3 n, float angle) {
	mat3 w = mat3(
		vec3(0., n.z, -n.y),
		vec3(-n.z, 0., n.x),
		vec3(n.y, -n.x, 0.)
	);
	
	mat3 I = mat3(
		vec3(1., 0., 0.),
		vec3(0., 1., 0.),
		vec3(0., 0., 1.)
	);
	
	return v * (I + w * sin(angle) + w * w * (1. - cos(angle)));
}

void fragment() {
	vec3 frag_pos = rotate((INV_VIEW_MATRIX * vec4(VERTEX, 1.)).xyz - NODE_POSITION_WORLD, vec3(0, 1, 0), rotation);

	float height = triplanar_texture(frag_pos, frag_pos, 10.).x;
	height += 0.1;
	height /= 1.1;
	ALBEDO = vec3(height);
}
