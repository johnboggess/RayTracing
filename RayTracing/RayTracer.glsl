-- Compute
#version 430
#define PI 3.1415926538

struct Sphere
{
	vec3 center;
	float radius;
	vec3 color;
	float diffuse;
};

struct Ray
{
	vec3 origin;
	vec3 direction;
	vec3 energy;
};

struct HitInfo
{
	vec3 hitPosition;
	vec3 hitNormal;
	bool hit;
	Sphere sphere;
};


uniform writeonly image2D destTex;

layout (binding=1, rgba8)
uniform image2D backgroundTex;

uniform mat4 ToWorldSpace;
uniform vec3 CameraPos;
uniform float ViewPortWidth;
uniform float ViewPortHeight;
uniform int BackgroundWidth;
uniform int BackgroundHeight;
uniform int WindowWidth;
uniform int WindowHeight;

Sphere[3] spheres = Sphere[3]
(
	Sphere(vec3(0,0,1), .5, vec3(1,.8,.8), 0),
	Sphere(vec3(0,-100.5,1), 100, vec3(.8,1,.8), 1),
	Sphere(vec3(0,100.5,1), 100, vec3(.8,.8,1), 1)
);

layout (local_size_x = 32, local_size_y = 32) in;

float rand(vec2 co){
    return fract(sin(dot(co, vec2(12.9898, 78.233))) * 43758.5453);
}

vec3 random_in_unit_sphere(float rndOffset)
{
	ivec2 screenPos = ivec2(gl_GlobalInvocationID.xy);
	vec2 floatScreenPos = vec2(screenPos);
	vec2 UV = floatScreenPos/vec2(WindowWidth, WindowHeight);
	float theta = rand(UV+rndOffset) * PI;
	float phi = rand(UV+.02+rndOffset) * PI;

	return vec3(sin(phi) * cos(theta),sin(phi) * sin(theta),cos(phi));
}

float HitSphere(Sphere sphere, Ray r)
{
	vec3 oc = r.origin-sphere.center;
	float a = dot(r.direction, r.direction);
	float b = 2.0*dot(oc,r.direction);
	float c = dot(oc,oc) - sphere.radius*sphere.radius;
    float discriminant = b*b - 4*a*c;

	if(discriminant < 0.0)
		return -1.0;
	return (-b - sqrt(discriminant))/(2.0*a);
}

HitInfo FireRay(Ray ray)
{
	Sphere s;
	float dist = 1.0/0.0;
	HitInfo info = HitInfo(vec3(0,0,0), vec3(0,0,0), false, s);
	for(int i = 0; i < spheres.length(); i++)
	{
		float t = HitSphere(spheres[i], ray);
		if( t < dist && t >= 0.0)
		{
			dist = t;
			info.hit = true;
			info.sphere = spheres[i];
			info.hitPosition = (ray.direction * dist) + ray.origin;
			info.hitNormal = normalize(info.hitPosition - info.sphere.center);
		}
	}
	return info;
}

vec3 RayTrace(Ray startRay, vec3 energy, int newRaysPerBounce, int maxBounces)
{
	int bounces = 0;
	Ray[100] rays;
	rays[0] = startRay;
	int raysSize = 1;
	int newRaysSize = raysSize;

	for(int bounces = 0; bounces < maxBounces; bounces++)
	{
		for(int i = 0; i < raysSize; i++)
		{
			Ray ray = rays[i];
			HitInfo info = FireRay(ray);

			if(!info.hit)
				continue;
			ray.origin = info.hitPosition;
			ray.direction = info.hitNormal;
			ray.energy = ray.energy * info.sphere.color;

			rays[i] = ray;

			for(int newRay = 0; newRay < newRaysPerBounce; newRay++)
			{
				newRaysSize = newRaysSize + 1;
				rays[newRaysSize] = Ray(ray.origin, ray.direction, ray.energy);
				rays[newRaysSize].direction += random_in_unit_sphere(.1) * info.sphere.diffuse;
				rays[newRaysSize].direction = normalize(rays[newRaysSize].direction);
			}

			rays[i].direction += random_in_unit_sphere(0) * info.sphere.diffuse;
			rays[i].direction = normalize(rays[i].direction);
		}
		raysSize = newRaysSize;
	}

	vec3 result = vec3(0);
	for(int i = 0; i < raysSize; i++)
	{
		result += rays[i].energy;
	}

	return result/raysSize;
}

vec3 ScreenPosToWorldPos(vec2 screenPos)
{
	vec2 UVpos = vec2(float(screenPos.x)/float(WindowWidth), float(screenPos.y)/float(WindowHeight));
	vec2 NDCPos = UVpos * 2.0 - vec2(1.0,1.0);
	vec3 VSpos = vec3(NDCPos.x * ViewPortWidth, NDCPos.y * ViewPortHeight, 1);
	return (ToWorldSpace * vec4(VSpos,1)).xyz;
}

void main()
{
	ivec2 screenPos = ivec2(gl_GlobalInvocationID.xy);
	
	int raysPerPxl = 1;

	vec3 resultColor = vec3(0,0,0);

	for(int i = 0; i < raysPerPxl; i++)
	{
		vec2 floatScreenPos = vec2(screenPos);
		vec2 UV = floatScreenPos/vec2(WindowWidth, WindowHeight);
		vec2 offset;
		offset.x = rand(UV+vec2(i));
		offset.y = rand(UV+vec2(i)+vec2(.01));

		offset.x = (offset.x+1.0)/2.0;
		offset.y = (offset.y+1.0)/2.0;

		floatScreenPos += offset;

		vec3 WSpos = ScreenPosToWorldPos(floatScreenPos);
		Ray ray = Ray(CameraPos, WSpos-CameraPos, vec3(1,1,1));
		resultColor += RayTrace(ray, vec3(1,1,1), 1, 5);
	}
	
	resultColor = resultColor/raysPerPxl;
	imageStore(destTex, screenPos, vec4(resultColor,0.0));
}