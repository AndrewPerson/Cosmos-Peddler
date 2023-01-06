shader_type spatial;
render_mode unshaded;

uniform float ior;
uniform vec2 planeSize = vec2(2, 2);

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

float fresnel(float amount, vec3 normal, vec3 view)
{
	return pow((1.0 - clamp(dot(normalize(normal), normalize(view)), 0.0, 1.0 )), amount);
}

vec2 raySphere(vec3 centre, float radius, vec3 rayOrigin, vec3 rayDir) {
	vec3 offset = rayOrigin - centre;
	float b = 2. * dot(offset, rayDir);
	float c = dot(offset, offset) - radius * radius;
	float d = b * b - 4. * c;
	
	if (d > 0.) {
		float s = sqrt(d);
		float dstToSphereNear = max(0, (-b - s) / 2.);
		float dstToSphereFar = (-b + s) / 2.;

		// Ignore intersections that occur behind the ray
		if (dstToSphereFar >= 0.) {
			return vec2(dstToSphereNear, dstToSphereFar - dstToSphereNear);
		}
	}
	
	return vec2(0., 0.);
}

float distToPlane(vec3 rayOrigin, vec3 rayDir, vec3 planeCenter, vec3 planeNorm) {
	return dot(planeCenter - rayOrigin, planeNorm) / dot(rayDir, planeNorm);
}

bool collidesWithAccretionDisc(vec3 rayOrigin, vec3 rayDir, vec2 planeDimensions, out vec3 intersection) {	
	if (dot(rayDir, vec3(0, 1, 0)) > 0.) return false;
	
	float dist = distToPlane(rayOrigin, rayDir, vec3(0.), vec3(0, 1, 0));
	
	intersection = rayOrigin + rayDir * dist;
	
	if (intersection.x < -planeDimensions.x / 2. || intersection.x > planeDimensions.x / 2. ||
		intersection.z < -planeDimensions.y / 2. || intersection.z > planeDimensions.y / 2.) {
		return false;
	}
	
	return true;
}

vec3 refractRay(vec3 rayDir, float ior1, vec3 surfaceNorm, float ior2) {
	float r = ior1 / ior2;
	float c = dot(-surfaceNorm, rayDir);
	
	return r * rayDir + surfaceNorm * (r * c - sqrt(1. - pow(r, 2) * (1. - pow(c, 2))));
}

void fragment()
{
	vec3 fragNormal = (INV_VIEW_MATRIX * vec4(NORMAL, 0.)).xyz;
	
	vec3 fragPos = (INV_VIEW_MATRIX * vec4(VERTEX, 1.)).xyz;
	vec3 fragPosLocal = fragPos - NODE_POSITION_WORLD;
	vec3 fragRay = normalize(fragPos - CAMERA_POSITION_WORLD);
	
	vec2 depthOfSphere = raySphere(NODE_POSITION_WORLD, .5, CAMERA_POSITION_WORLD, fragRay);
	vec3 frontOfSphere = CAMERA_POSITION_WORLD + fragRay * depthOfSphere.x;
	
	vec3 frontOfSphereObjectCoord = frontOfSphere - NODE_POSITION_WORLD;
	
	float fres = 1. / (fresnel(1., NORMAL, VIEW) - .1);
	float fior = fres;//1. / fres;// * fres;
	
	vec3 lightRay = refractRay(fragRay, 1., normalize(frontOfSphereObjectCoord), fior);
	vec2 lightRayDepthSphere = raySphere(NODE_POSITION_WORLD, .5, frontOfSphere, lightRay);
	
	if (lightRayDepthSphere.x + lightRayDepthSphere.y <= 0.) {
		ALBEDO = vec3(0.);
	}
	else {
		vec3 lightRayExit = frontOfSphere + lightRay * (lightRayDepthSphere.x + lightRayDepthSphere.y);
		vec3 lightRayExitObjectCoord = lightRayExit - NODE_POSITION_WORLD;
		
		vec3 lightRayExitRay = refractRay(lightRay, fior,-normalize(lightRayExitObjectCoord), 1.);
		
		vec3 intersection;
		if (collidesWithAccretionDisc(lightRayExit, lightRayExitRay, planeSize, intersection)) {
			ALBEDO = vec3(1.);
		}
		else {
			ALBEDO = vec3(0.);
		}
		
		//ALBEDO = lightRayExitRay;
	}
	
	//ALBEDO = vec3(fres);
}