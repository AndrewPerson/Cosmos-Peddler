shader_type spatial;
render_mode unshaded;

instance uniform float line_width = 0.05;
instance uniform float dash_count = 10;
instance uniform float time_scale = 10;

vec2 polarCoord(vec2 uv) {
	return vec2(length(uv), degrees(atan(uv.y, uv.x)));
}

void fragment() {
	vec2 adjustedUV = UV - vec2(.5);
	
	if (length(adjustedUV) < .5 - line_width || length(adjustedUV) > .5) {
		discard;
	}
	
	vec2 polar = polarCoord(adjustedUV);
	polar.y = mod(polar.y + TIME * time_scale, 360.);

	if (mod(polar.y, 360. / dash_count) < 180. / dash_count) {
		discard;
	}
}
