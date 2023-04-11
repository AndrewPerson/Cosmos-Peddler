shader_type spatial;
render_mode specular_toon;

#include "res://Shader Utils/fbm.gdshaderinc"

uniform float ocean_height;
uniform float ocean_height_multiplier = 1.;
uniform sampler2D ocean_gradient;
uniform float time_scale = 0.1;

uniform sampler2D depth_texture: hint_depth_texture, filter_linear_mipmap;

const int octaves = 5;

instance uniform float seed: hint_range(1, 10);
instance uniform float rotation;

float height(vec2 uv) {
	vec2 base_fbm_uv = uv * 50.;

	float height = fbm(base_fbm_uv, octaves, seed);

	return clamp(height, 0., .5);
}

vec2 ray_sphere(vec3 centre, float radius, vec3 rayOrigin, vec3 rayDir) {
	vec3 offset = rayOrigin - centre;
	float b = 2. * dot(offset, rayDir);
	float c = dot(offset, offset) - radius * radius;
	float d = b * b - 4. * c;
	
	if (d > 0.) {
		float s = sqrt(d);
		float dstToSphereNear = max(0, (-b - s) / 2.);
		float dstToSphereFar = (-b + s) / 2.;

		// Ignore intersections that occur behind the ray
		if (dstToSphereFar >= 0.) {
			return vec2(dstToSphereNear, dstToSphereFar - dstToSphereNear);
		}
	}
	
	return vec2(0., 0.);
}

vec3 triplanar_normal(vec3 uv) {
	vec3 normalX = vec3(height(uv.yz));
	vec3 normalY = vec3(height(uv.xz + vec2(0.1)));
	vec3 normalZ = vec3(height(uv.xy + vec2(0.2)));

	vec3 blend_weight = abs(uv);
	blend_weight = normalize(blend_weight);

	vec3 normal = blend_weight.x * normalX + blend_weight.y * normalY + blend_weight.z * normalZ;
	
	return normal - vec3(0.5);
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

void vertex() {
	VERTEX = VERTEX + VERTEX * ocean_height;
}

void fragment() {
	vec3 frag_pos = (INV_VIEW_MATRIX * vec4(VERTEX, 1.)).xyz;
	vec3 frag_pos_local = rotate(frag_pos - NODE_POSITION_WORLD, vec3(0, 1, 0), rotation);
	vec3 frag_ray = normalize(frag_pos - CAMERA_POSITION_WORLD);
	
	float non_linear_depth = texture(depth_texture, SCREEN_UV).x;
	vec3 ndc = vec3(SCREEN_UV * 2. - 1., non_linear_depth);
	vec4 view = INV_PROJECTION_MATRIX * vec4(ndc, 1.);
	view.xyz /= view.w;
	
	float background_linear_depth = -view.z;
	
	float frag_linear_depth = -VERTEX.z;

	float depth_to_ocean_floor = (background_linear_depth - frag_linear_depth) * ocean_height_multiplier;
	float depth_through_ocean = ray_sphere(NODE_POSITION_WORLD, (1. + ocean_height) / 2., CAMERA_POSITION_WORLD, frag_ray).y / (1. + ocean_height * 2.);
	
	float depth = clamp(min(depth_through_ocean, depth_to_ocean_floor), 0., 1.);
	
	vec4 colour = texture(ocean_gradient, vec2(depth, 0.));
	
	ALBEDO = colour.rgb;
	ALPHA = colour.a;
	
	METALLIC = 0.0;
	ROUGHNESS = 0.01;

	NORMAL = normalize(NORMAL + rotate((VIEW_MATRIX * vec4(triplanar_normal(frag_pos_local + vec3(TIME * time_scale)), 0.)).xyz, vec3(0, 1, 0), -rotation));
}
