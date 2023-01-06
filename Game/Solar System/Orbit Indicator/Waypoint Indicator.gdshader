shader_type spatial;
render_mode unshaded;

instance uniform float line_width = 0.05;
instance uniform float dash_count = 2;
instance uniform float time_scale = 0.0278;

void fragment() {
	vec2 adjustedUV = UV - vec2(.5);
	
	if (adjustedUV.y < 0.) {
		discard;
	}
	
	if (abs(adjustedUV.x) > line_width / 2.) {
		discard;
	}
	
	adjustedUV.y -= TIME * time_scale;
	
	if (mod(adjustedUV.y, .5 / dash_count) < .25 / dash_count) {
		discard;
	}
}
