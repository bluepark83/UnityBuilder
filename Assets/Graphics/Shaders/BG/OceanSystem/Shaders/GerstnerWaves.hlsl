#ifndef GERSTNER_WAVES_INCLUDED
#define GERSTNER_WAVES_INCLUDED

uniform uint _WaveCount;

struct Wave
{
	// 진푹
	float amplitude;
	// 방향
	float direction;
	// 길이
	float wavelength;
	// 기울기
	float steepness;
	float2 origin;
	float omni;
};

struct WaveStruct
{
	float3 position;
	float3 normal;
};

half4 waveData[20];

// 사용하지 않는 버전
WaveStruct GerstnerWave(half2 pos, float waveCountMulti, half amplitude, half direction, half wavelength, half omni, half2 omniPos)
{
	WaveStruct waveOut;
	
	float time = _Time.y;
	half3 wave = 0;
	half w = 6.28318 / wavelength;
	half wSpeed = sqrt(9.8 * w);
	half peak = 1.5;
	half qi = peak / (amplitude * w * _WaveCount);

	direction = radians(direction);
	half2 dirWaveInput = half2(sin(direction), cos(direction)) * (1 - omni);
	half2 omniWaveInput = (pos - omniPos) * omni;

	// 바람의 방향 계산
	half2 windDir = normalize(dirWaveInput + omniWaveInput);
	half dir = dot(windDir, pos - (omniPos * omni));

	// 방향 * wave 길이 * speed
	half calc = dir * w + -time * wSpeed;
	half cosCalc = cos(calc);
	half sinCalc = sin(calc);
	
	wave.xz = qi * amplitude * windDir.xy * cosCalc;
	wave.y = ((sinCalc * amplitude)) * waveCountMulti;
	
	half wa = w * amplitude;
	// 노말 계산
	half3 n = half3(-(windDir.xy * wa * cosCalc),
					1-(qi * wa * sinCalc));
	
	waveOut.position = wave * saturate(amplitude * 10000);
	waveOut.normal = (n.xzy * waveCountMulti);

	return waveOut;
}

// 사용하지 않는 버전
WaveStruct GerstnerWave2(half2 pos, float waveCountMulti, float4 amplitude4,
	half direction, half omni, half2 omniPos)
{
	WaveStruct waveOut;
	float4 speed = float4(1.2f * _Time.x, 1.375f * _Time.y,
		1.1f * _Time.x, _Time.y);
	half3 wave = 0;

	// 방향 계산
	direction = radians(direction);
	half2 dirWaveInput = half2(sin(direction), cos(direction)) * (1 - omni);
	half2 omniWaveInput = (pos - omniPos) * omni;

	// 바람의 방향 계산
	half2 windDir = normalize(dirWaveInput + omniWaveInput);
	half dir = dot(windDir, pos - (omniPos * omni));
	float4 _WaveDirection = float4(dir,dir,dir,dir);
	
	const float4 dirAB = float4(0.3, 0.85, 0.85, 0.25) * _WaveDirection;
	const float4 dirCD = float4(0.1, 0.9, -0.5, -0.5) * _WaveDirection;
	
	float4 dotABCD = float4(dot(dirAB.xy, pos), dot(dirAB.zw, pos),
		dot(dirCD.xy, pos), dot(dirCD.zw, pos));
	float4 COS = cos(dotABCD + speed);
	float4 SIN = sin(dotABCD + speed);
	// 방향 계산
	float4 AB = 0.01 * dirAB.xxzw * amplitude4.xxyy;
	float4 CD = 0.01 * dirCD.xyzw * amplitude4.zzww;
	// 위치 계산
	wave.x = dot(COS, float4(AB.xz, CD.xz));
	wave.z = dot(COS, float4(AB.yw, CD.yw));
	wave.y = dot(SIN, amplitude4);
	wave *= waveCountMulti;
	// 노말 계산
	float3 nrml = float3(0, 12.0, 0);
	nrml.x -= dot(COS, float4(AB.xz, CD.xz));
	nrml.z -= dot(COS, float4(AB.yw, CD.yw));
	nrml = normalize(nrml) * waveCountMulti;
	
	waveOut.position = wave;
	waveOut.normal = nrml;
	return waveOut;
}

WaveStruct GerstnerWave3(half2 pos, float waveCountMulti, half amplitude,
	half direction, half wavelength, half omni, half2 omniPos, float steepness)
{
	WaveStruct waveOut;
	
	float time = _Time.y * _WaveSpeed;
	half3 wave = 0;
	
	half w = 2 * PI / wavelength;
	half wSpeed = sqrt(9.8 * w);
	half peak = 1.5;
	half qi = peak / (amplitude * w * _WaveCount);

	direction = radians(direction);
	half2 dirWaveInput = half2(sin(direction), cos(direction)) * (1 - omni);
	half2 omniWaveInput = (pos - omniPos) * omni;

	// 바람의 방향 계산
	half2 windDir = normalize(dirWaveInput + omniWaveInput);
	
	float2 d = normalize(windDir) * _Frequency;
	float calc2 = w * dot(d, pos) - wSpeed * time * 0.3;
	float a = amplitude;
	
	half cosCalc = cos(calc2) * steepness;
	half sinCalc = sin(calc2);

	// 바람의 방향에 따라 뒤집히는 현상 개선
	half2 dirWaveInput2 = half2(sin(direction), cos(direction));
	
	wave.x = d.x * a * (float)windDir.xy * cosCalc * 10 * dirWaveInput2.x;
	wave.z = d.y * a * (float)windDir.xy * cosCalc * 10 * dirWaveInput2.x;
	wave.y = ((sinCalc * a));
	
	half wa = w * amplitude;
	// 노말 계산
	half3 n = half3(-(windDir.xy * wa * cosCalc),
					1-(qi * wa));
	
	waveOut.position = wave * saturate(amplitude * 10000);
	waveOut.normal = (n.xzy * waveCountMulti);
	return waveOut;
}

// Seed에 따른 랜덤 생성 (문제의 여지가 있어 사용하지 않음)
float Random(float seed)
{
	return frac(sin(seed * 12.9898 + 78.233) * 43758.5453);
}

inline void SampleWaves(float3 position, half opacity, float distanceBlend, out WaveStruct waveOut)
{
	half2 pos = position.xz;
	waveOut.position = 0;
	waveOut.normal = 0;
	half waveCountMulti = 1.0 / _WaveCount;
	half3 opacityMask = saturate(half3(3, 3, 1) * opacity);

	float r = 1.0f / _WaveCount;
	
	for(uint i = 0; i < _WaveCount; i++)
	{
		Wave w;
		float p = lerp(0.5f, 1.5f, i * r);
		w.amplitude = waveData[i].x;
		w.direction = waveData[i].y;
		w.wavelength = waveData[i].z;
		w.omni = waveData[i].w;
		w.origin = waveData[i + 10].xy;
		w.steepness = waveData[i + 10].z;
		
		WaveStruct wave = GerstnerWave3(pos,
								waveCountMulti,
								w.amplitude * distanceBlend,
								w.direction,
								w.wavelength,
								w.omni,
								w.origin,
								w.steepness);

		waveOut.position += wave.position;
		waveOut.normal += wave.normal;
	}

	waveOut.position *= opacityMask;
	waveOut.normal *= half3(opacity, 1, opacity);
}
#endif