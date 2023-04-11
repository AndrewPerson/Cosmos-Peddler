shader_type spatial;

#include "res://Shader Utils/fbm.gdshaderinc"

uniform float height_scale = 1.;
uniform float uv_height_scale = 2.;
uniform sampler2D height_colours;

const float size = 5.;

instance uniform float seed: hint_range(1, 10) = 1.;
instance uniform float rotation;

float height(vec2 uv) {
	vec2 base_fbm_uv = uv * float(size);
	
	// use multiple fbm's at different places so we can determine what color land gets
	float fbm1 = fbm(base_fbm_uv * vec2(2., 1.), 5, size);
	float fbm2 = fbm(base_fbm_uv * vec2(2., 1.) - fbm1, 5, size);
	float fbm3 = fbm(base_fbm_uv * vec2(2., 1.) - 1.5 * fbm1, 5, size);
	float fbm4 = fbm(base_fbm_uv * vec2(2., 1.) - 2. * fbm1, 5, size);
	
	float river_fbm = fbm(base_fbm_uv * vec2(2., 1.) + fbm1 * 6., 5, size);
	
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

	float height = triplanar_height(uv);

	VERTEX = VERTEX + VERTEX * height * height_scale;
}

void fragment() {
	vec3 frag_pos = (INV_VIEW_MATRIX * vec4(VERTEX, 1.)).xyz - NODE_POSITION_WORLD;
	vec3 uv = rotate(frag_pos, vec3(0, 1, 0), rotation);

	float height = triplanar_height(uv);
	ALBEDO = texture(height_colours, vec2(height, 0.)).rgb;
	
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
					uva + uva * a * height_scale * uv_height_scale,
					uvb + uvb * b * height_scale * uv_height_scale,
					uvc + uvc * c * height_scale * uv_height_scale
				),
				0.
			)).xyz, vec3(0, 1, 0), -rotation)
	);
}