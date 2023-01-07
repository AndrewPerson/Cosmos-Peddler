shader_type spatial;
render_mode cull_disabled;

uniform float time_speed : hint_range(0, 1) = 0.01;
uniform float ring_width : hint_range(0, .15) = 0.1;
uniform sampler2D colorscheme;

instance uniform float seed: hint_range(1, 10) = 1;

const float size = 15.;
const int OCTAVES = 4;

float rand(vec2 coord) {
	coord = mod(coord, vec2(2.0,1.0)*size);
	return fract(sin(dot(coord.xy ,vec2(12.9898,78.233))) * 15.5453 * seed);
}

float noise(vec2 coord){
	vec2 i = floor(coord);
	vec2 f = fract(coord);
	
	float a = rand(i);
	float b = rand(i + vec2(1.0, 0.0));
	float c = rand(i + vec2(0.0, 1.0));
	float d = rand(i + vec2(1.0, 1.0));

	vec2 cubic = f * f * (3.0 - 2.0 * f);

	return mix(a, b, cubic.x) + (c - a) * cubic.y * (1.0 - cubic.x) + (d - b) * cubic.x * cubic.y;
}

float fbm(vec2 coord){
	float value = 0.0;
	float scale = 0.5;

	for(int i = 0; i < OCTAVES ; i++){
		value += noise(coord) * scale;
		coord *= 2.0;
		scale *= 0.55;
	}
	return value;
}

vec2 polar_coordinates(vec2 uv)
{
	float radius = length(uv);
	float angle = atan(uv.y, uv.x) / (PI * 2.0);
	return vec2(radius, angle);
}

void fragment() {
	// pixelize uv
	vec2 uv = UV;
	
	// center is used to determine ring position
	vec2 uv_center = uv - vec2(.5);
	
	// convert to polar coords
	vec2 polarUV = polar_coordinates(uv_center);
	
	// cut out 2 circles of different sizes and only intersection of the 2.
	float ring = smoothstep(0.5-ring_width*2.0, 0.5-ring_width, polarUV.x);
	ring *= smoothstep(polarUV.x-ring_width, polarUV.x, 0.4);

	// rotate material in the ring
	polarUV += vec2(TIME * time_speed);
	
	// some noise
	ring *= fbm(polarUV*size);
	
	vec4 col = texture(colorscheme, vec2(fbm(vec2(polarUV.x * 30., 0.)), 0.));
	float ring_a = step(0.28, ring);
	ALBEDO = col.rgb;
	ALPHA = ring_a * col.a;
}