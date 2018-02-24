#define PI 3.14159265359

struct ParticleStruct
{
    float3 pos;
    float3 vel;
    float4 cd;
    float age;
    float lifespan;
    float mass;
    float momentum;
};

float rand(float2 co)
{
	return frac(sin(dot(co.xy,float2(12.9898,78.233))) * 43758.5453);
}

float3 randomSpherePoint(float3 rand) {
    float3 randNew = (rand*2.)-1.;
    float ang1 = (randNew.x + 1.0) * PI; // [-1..1) -> [0..2*PI)
    float u = randNew.y; // [-1..1), cos and acos(2v-1) cancel each other out, so we arrive at [-1..1)
    float u2 = u * u;
    float sqrt1MinusU2 = sqrt(1.0 - u2);
    float x = sqrt1MinusU2 * cos(ang1);
    float y = sqrt1MinusU2 * sin(ang1);
    float z = u;
    return float3(x, y, z);
}

float fit(float val, float inMin, float inMax, float outMin, float outMax) {
    return ((outMax - outMin) * (val - inMin) / (inMax - inMin)) + outMin;
}

float2 fit(float2 val, float2 inMin, float2 inMax, float2 outMin, float2 outMax) {
    return ((outMax - outMin) * (val - inMin) / (inMax - inMin)) + outMin;
}
