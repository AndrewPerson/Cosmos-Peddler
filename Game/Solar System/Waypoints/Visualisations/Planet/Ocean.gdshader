shader_type spatial;
render_mode specular_toon;

uniform float ocean_height;
uniform float ocean_height_multiplier = 1.;
uniform sampler2D ocean_gradient;
uniform float time_scale = 0.1;

uniform sampler2D depth_texture: hint_depth_texture, filter_linear_mipmap;

const int size = 50;
const int octaves = 5;

instance uniform float seed: hint_range(1, 10);
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
