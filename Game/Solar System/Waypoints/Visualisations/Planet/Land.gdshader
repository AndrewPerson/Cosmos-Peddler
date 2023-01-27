shader_type spatial;

uniform float height_scale = 1.;
uniform sampler2D height_colours;

const int size = 5;
const int octaves = 5;

instance uniform float seed: hint_range(1, 10) = 1.;
instance uniform float rotation;

float rand(vec2 coord) {
	// land has to be tiled (or the contintents on this planet have to be changing very fast)
	// tiling only works for integer values, thus the rounding
	// it would probably be better to only allow integer sizes
	// multiply by vec2(2,1) to simulate planet having another side
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

float triplanar_height(vec3 uv) {	
	float heightX = height(uv.yz);
	float heightY = height(uv.xz + vec2(0.1));
	float heightZ = height(uv.xy + vec2(0.2));

	vec3 blend_weight = pow(abs(uv), vec3(10.));
	blend_weight /= blend_weight.x + blend_weight.y + blend_weight.z;

	float height = blend_weight.x * heightX + blend_weight.y * heightY + blend_weight.z * heightZ;
	
	return height;
}

vec3 perpendicular(vec3 v) {
	float sz = sign(v.z) * length(v);
	
	return vec3(sz * (v.z + sz) - v.x * v.x, -v.x * v.y, -v.x * (v.z + sz));
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

vec3 normal(vec3 a, vec3 b, vec3 c) {
	vec3 v = b - a;
	vec3 w = c - a;
	
	return normalize(vec3((v.y * w.z) - (v.z * w.y), (v.z * w.x) - (v.x * w.z), (v.x * w.y) - (v.y * w.x)));
}

void vertex() {
	vec3 uv = VERTEX;

	float k = triplanar_height(uv);

	VERTEX = VERTEX + VERTEX * k * height_scale;
}

void fragment() {
	vec3 uv = rotate((INV_VIEW_MATRIX * vec4(VERTEX, 1.)).xyz - NODE_POSITION_WORLD, vec3(0, 1, 0), rotation);

	float height = triplanar_height(uv);
	ALBEDO = texture(height_colours, vec2(height, 0.)).rgb;
	
	vec3 p = normalize(perpendicular(uv));
	
	vec3 uva = rotate(uv, p, radians(.01));
	vec3 uvb = rotate(uv, rotate(p, uv, radians(60)), radians(-.01));
	vec3 uvc = rotate(uv, rotate(p, uv, radians(-60)), radians(-.01));

	float a = triplanar_height(uva);
	float b = triplanar_height(uvb);
	float c = triplanar_height(uvc);

	NORMAL = normalize(NORMAL + rotate((VIEW_MATRIX * vec4(normal(uva * a, uvb * b, uvc * c), 0.)).xyz, vec3(0, 1, 0), -rotation));
}