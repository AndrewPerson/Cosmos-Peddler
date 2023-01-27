shader_type particles;

float rand_from_seed(in uint seed) {
	int k;
	int s = int(seed);
	
	if (s == 0) s = 305420679;
	
	k = s / 127773;
	s = 16807 * (s - k * 127773) - 2836 * k;
	
	if (s < 0) s += 2147483647;
	
	seed = uint(s);
	
	return float(seed % uint(65536)) / 65535.;
}

uint hash(uint x) {
	x = ((x >> uint(16)) ^ x) * uint(73244475);
	x = ((x >> uint(16)) ^ x) * uint(73244475);
	x = (x >> uint(16)) ^ x;
	return x;
}

mat3 rotation_matrix(vec3 angles) {
	mat3 X = mat3(
		vec3(cos(angles.x), -sin(angles.x), 0),
		vec3(sin(angles.x), cos(angles.x), 0),
		vec3(0, 0, 1)
	);
	
	mat3 Y = mat3(
		vec3(1, 0, 0),
		vec3(0, cos(angles.y), -sin(angles.y)),
		vec3(0, sin(angles.y), cos(angles.y))
	);
	
	mat3 Z = mat3(
		vec3(cos(angles.z), -sin(angles.z), 0),
		vec3(sin(angles.z), cos(angles.z), 0),
		vec3(0, 0, 1)
	);
	
	return X * Y * Z;
}

vec3 point_in_sphere(vec3 angles, float dist) {
	return rotation_matrix(angles) * vec3(0, 1, 0) * dist;
}

void process() {
	uint alt_seed1 = hash(RANDOM_SEED + INDEX + 1u);
	uint alt_seed2 = hash(RANDOM_SEED + INDEX + 27u);
	uint alt_seed3 = hash(RANDOM_SEED + INDEX + 43u);
	uint alt_seed4 = hash(RANDOM_SEED + INDEX + 111u);
	
	float dist = mod(rand_from_seed(alt_seed4), .3) + .2;
	
	vec3 angles = vec3(
		rand_from_seed(alt_seed1) * 360.,
		rand_from_seed(alt_seed2) * 360.,
		rand_from_seed(alt_seed3) * 360.
	) * TIME / 2000.;
	
	float sizeScale = 50. * (dist + 0.1);
	
	mat3 transform_matrix = rotation_matrix(angles) / sizeScale;
	
	vec3 x_axis = vec3(1., 0., 0.) * transform_matrix;
	vec3 y_axis = vec3(0., 1., 0.) * transform_matrix;
	vec3 z_axis = vec3(0., 0., 1.) * transform_matrix;
	
	TRANSFORM = mat4(
		vec4(x_axis, 0),
		vec4(y_axis, 0),
		vec4(z_axis, 0),
		TRANSFORM[3]
	);
	
	if (RESTART) {
		vec3 position = point_in_sphere(
			vec3(
				rand_from_seed(alt_seed1) * 360.,
				rand_from_seed(alt_seed2) * 360.,
				rand_from_seed(alt_seed3) * 360.
			),
			dist
		);
		
		TRANSFORM[3].xyz = position;
	}
}
