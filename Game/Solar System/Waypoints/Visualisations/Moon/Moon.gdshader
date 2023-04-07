shader_type spatial;

uniform sampler2D crater_height_map;

instance uniform float seed: hint_range(1, 10) = 1.;
instance uniform float rotation;

vec2 hash( float n )
{
    float sn = sin(n);
    return fract(vec2(sn,sn*42125.13)*seed);
}

float circleNoise(vec2 uv, float radius)
{
    float uv_y = floor(uv.y);
    uv.x += uv_y * PI;
    vec2 f = fract(uv);
    vec2 h = hash(floor(uv.x) * uv_y);
    float m = (length(f - .25 - (h.x * .5)));
    float r = h.y * radius;
	return ((m - r) * (1. - smoothstep(r-.10*r,r,m)));
}

float height(vec2 uv) {
	return (circleNoise(uv * 3., .4) + 3.) / 4.;
}

float triplanar_height(vec3 uv) {
	float height_x = texture(crater_height_map, vec2(1. - height(uv.zy), 0)).x;
	float height_y = texture(crater_height_map, vec2(1. - height(uv.xz + vec2(.1)), 0)).x;
	float height_z = texture(crater_height_map, vec2(1. - height(uv.yx + vec2(.2)), 0)).x;
	
	vec3 blend_weight = pow(abs(uv), vec3(10.));
	blend_weight /= (blend_weight.x + blend_weight.y + blend_weight.z);
	
	return height_x * blend_weight.x + height_y * blend_weight.y + height_z * blend_weight.z;
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

void fragment() {
	vec3 frag_pos = normalize((INV_VIEW_MATRIX * vec4(VERTEX, 1.)).xyz - NODE_POSITION_WORLD);
	vec3 uv = rotate(frag_pos, vec3(0, 1, 0), rotation);

	float height = triplanar_height(uv);
	//ALBEDO = vec3(height);
	
	vec3 p = normalize(perpendicular(uv));
	
	vec3 uva = rotate(uv, p, radians(.01));
	vec3 uvb = rotate(uv, rotate(p, uv, radians(60)), radians(-.01));
	vec3 uvc = rotate(uv, rotate(p, uv, radians(-60)), radians(-.01));

	float a = triplanar_height(uva);
	float b = triplanar_height(uvb);
	float c = triplanar_height(uvc);

	NORMAL = normalize(
		NORMAL + rotate((
			VIEW_MATRIX * vec4(
				normal(
					uva + uva * a,
					uvb + uvb * b,
					uvc + uvc * c
				),
				0.
			)).xyz, vec3(0, 1, 0), -rotation)
	);
}
