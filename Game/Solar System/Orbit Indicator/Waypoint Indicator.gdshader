shader_type spatial;
render_mode unshaded;

instance uniform float line_width = 0.05;
instance uniform float dash_count = 2;
instance uniform float time_scale = 0.0278;

void fragment() {
	vec2 adjusted_uv = UV - vec2(.5);
	
	if (adjusted_uv.y < 0.) {
		discard;
	}
	
	if (abs(adjusted_uv.x) > line_width / 2.) {
		discard;
	}
	
	adjusted_uv.y -= TIME * time_scale;
	
	if (mod(adjusted_uv.y, .5 / dash_count) < .25 / dash_count) {
		discard;
	}
}
