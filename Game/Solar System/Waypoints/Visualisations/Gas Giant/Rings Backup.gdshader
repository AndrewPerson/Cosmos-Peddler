shader_type spatial;
render_mode cull_disabled;

uniform float innerRingWidth = 0.3;
uniform float outerRingWidth = 0.1;
uniform float timeScale = 0.01;
uniform sampler2D colours;

instance uniform float seed : hint_range(1, 10) = 1;

float rand(vec2 coord) {
	// land has to be tiled (or the contintents on this planet have to be changing very fast)
	// tiling only works for integer values, thus the rounding
	// it would probably be better to only allow integer sizes
	// multiply by vec2(2,1) to simulate planet having another side
	coord = mod(coord, vec2(2.,1.) * 4.);
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

	for(int i = 0; i < 5; i++){
		value += noise(coord) * scale;
		coord *= 2.;
		scale *= 0.5;
	}

	return value;
}

vec2 polar_coordinates(vec2 uv)
{
	float radius = length(uv) * 2.0;
	float angle = atan(uv.y, uv.x) / (PI * 2.0);
	return vec2(radius, angle);
}

void fragment()
{
	vec2 centeredUV = UV - vec2(.5);
	float dist = length(centeredUV);
	
	float height = fbm(polar_coordinates(centeredUV) * 12. - vec2(TIME) * timeScale);
	
	if (dist < height + .05 && dist > innerRingWidth && dist < innerRingWidth + outerRingWidth) {
		vec3 colour = texture(colours, vec2(height, 0.)).rgb;
		ALBEDO = colour;
	}
	else {
		discard;
	}
}

/*
void fragment() {
	vec2 centeredUV = UV - vec2(0.5);
	
	float height = clamp(texture(ringNoise, UV + TIME * time_scale, abs(atan(centeredUV.y / centeredUV.x) / 360.)).x * ringNoiseStrength + 1. - length(centeredUV), 0., 1.);
	
	height = floor(height + 0.4) - floor(height + 0.4 - ringWidth);
	
	ALBEDO = texture(colourNoise, vec2(length(centeredUV), abs(atan(centeredUV.y / centeredUV.x) / 360.))).rgb;
	ALPHA = height;
}
*/