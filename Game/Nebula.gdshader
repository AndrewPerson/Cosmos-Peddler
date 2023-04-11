shader_type fog;

#include "res://Shader Utils/fbm.gdshaderinc"

uniform vec4 CLOUD1_COL: source_color = vec4(0.41,0.64,0.78,0.4);
uniform vec4 CLOUD2_COL: source_color = vec4(0.99,0.79,0.46,0.2);
uniform vec4 CLOUD3_COL: source_color = vec4(0.81,0.31,0.59,1.0);
uniform vec4 CLOUD4_COL: source_color = vec4(0.27,0.15,0.33,1.0);

uniform sampler2D densityScale;

void fog() {
	float fbm1 = clamp(texture(densityScale, vec2(fbm(WORLD_POSITION.xz / 10., 5, 1.), 0.)).x, 0., 1.);
	float fbm2 = clamp(texture(densityScale, vec2(fbm(WORLD_POSITION.yz / 10., 5, 1.), 0.)).x, 0., 1.);
	float fbm3 = clamp(texture(densityScale, vec2(fbm(WORLD_POSITION.xy / 10., 5, 1.), 0.)).x, 0., 1.);
	
	DENSITY = fbm1 * fbm2;
	EMISSION = normalize(
		fbm1 * CLOUD1_COL +
		fbm2 * CLOUD3_COL +
		fbm3 * CLOUD4_COL
	).rgb;
}
