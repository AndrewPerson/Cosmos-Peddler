shader_type spatial;
render_mode unshaded;

uniform float blackhole_radius : hint_range(0, 0.5) = 0.3;

void fragment() {
	float dist = cos(dot(NORMAL, VIEW)) - .5;
	
	if (dist < blackhole_radius) {
		ALBEDO = vec3(0);
	}
	else {
		discard;
	}
}
