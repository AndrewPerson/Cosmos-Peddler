shader_type spatial;
render_mode unshaded;

instance uniform float line_width = 0.05;
instance uniform float dash_count = 10;
instance uniform float time_scale = 10;

vec2 polar_coordinates(vec2 uv) {
	return vec2(length(uv), degrees(atan(uv.y, uv.x)));
}

void fragment() {
	vec2 adjusted_uv = UV - vec2(.5);
	
	if (length(adjusted_uv) < .5 - line_width || length(adjusted_uv) > .5) {
		discard;
	}
	
	vec2 polar = polar_coordinates(adjusted_uv);
	polar.y = mod(polar.y + TIME * time_scale, 360.);

	if (mod(polar.y, 360. / dash_count) < 180. / dash_count) {
		discard;
	}
}
