shader_type spatial;
render_mode unshaded;

vec2 rand(vec2 value){
	value = vec2( dot(value, vec2(127.1,311.7) ),
				  dot(value, vec2(269.5,183.3) ) );
	return -1.0 + 2.0 * fract(sin(value) * 43758.5453123);
}

float noise(vec2 coord){
	vec2 _period = vec2(1.);
	
	coord = coord;
	vec2 cellsMinimum = floor(coord);
	vec2 cellsMaximum = ceil(coord);
	vec2 uv_fract = fract(coord);
	
	cellsMinimum = mod(cellsMinimum, _period);
	cellsMaximum = mod(cellsMaximum, _period);
	
	vec2 blur = smoothstep(0.0, 1.0, uv_fract);
	
	vec2 lowerLeftDirection = rand(vec2(cellsMinimum.x, cellsMinimum.y));
	vec2 lowerRightDirection = rand(vec2(cellsMaximum.x, cellsMinimum.y));
	vec2 upperLeftDirection = rand(vec2(cellsMinimum.x, cellsMaximum.y));
	vec2 upperRightDirection = rand(vec2(cellsMaximum.x, cellsMaximum.y));
	
	vec2 fraction = fract(coord);
	
	return mix( mix( dot( lowerLeftDirection, fraction - vec2(0, 0) ),
                     dot( lowerRightDirection, fraction - vec2(1, 0) ), blur.x),
                mix( dot( upperLeftDirection, fraction - vec2(0, 1) ),
                     dot( upperRightDirection, fraction - vec2(1, 1) ), blur.x), blur.y) * 0.8 + 0.5;
}

float fbm(vec2 coord){
	float value = 0.;
	float scale = 0.5;
	int octaves = 6;

	for(int i = 0; i < octaves ; i++){
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

void fragment() {
	vec3 fragPos = (INV_VIEW_MATRIX * vec4(VERTEX, 1.)).xyz - NODE_POSITION_WORLD;
		
	float fres = clamp(1. - pow(length(fragPos.xz / 2.), 1.), 0., 1.);
		
	float v = fres * fbm(polar_coordinates(fragPos.xz) + vec2(TIME));
	//ALBEDO = vec3(v);
	//ALPHA = clamp(v * 10., 0., 1.);
}
