Shader "Game/Asteroid Bump"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_TilingMain ("UV Tiling (Main)", Float) = 1.0
		_TilingBump ("UV Tiling (Bump)", Float) = 1.0
	}
   
	SubShader
	{
		LOD 300
		Tags { "RenderType" = "Opaque" }

CGPROGRAM
#pragma surface surf Lambert vertex:vert

sampler2D _MainTex;
sampler2D _BumpMap;
fixed4 _Color;
float _TilingMain;
float _TilingBump;

struct Input
{
	float3 objPos;
	float3 normal;
};

void vert (inout appdata_full v, out Input o)
{
	// NOTE: Due to the nature of unity_Scale, this shader may not work properly with non-uniformly scaled objects.
	// See http://answers.unity3d.com/questions/47200/what-does-the-unityscale-shader-uniform-represent.html
	o.objPos = (v.vertex.xyz / v.vertex.w) * (_TilingMain / unity_Scale.w);
	o.normal = normalize(v.normal);

	// Sides that get sampled upside-down (-X, -Y, -Z) need to have their normal map's Y flipped.
	// Positive sides end up with +1, while negatives end up with -1.
	float lowest = min(min(o.normal.x, o.normal.y), o.normal.z);

	// For top and bottom, the tangent points to the right. In all other cases it points straight down.
	// The reason for this is to ensure that no two adjacent faces end up with tangents that point
	// in opposite directions, as this causes visible artifacts.

	float3 tangent = float3(abs(o.normal.y), -max(abs(o.normal.x), abs(o.normal.z)), 0.0);
	v.tangent = float4(normalize(tangent), floor(lowest) * 2.0 + 1.0);
}

void surf (Input IN, inout SurfaceOutput o)
{
	// Texture contribution should add up to 1.0
	float3 contribution = normalize(IN.normal);
	contribution *= contribution;
	contribution *= contribution;
	contribution *= contribution;
	contribution  = contribution / (contribution.x + contribution.y + contribution.z);
	
	// All faces are readable as long as they are on increasing axes (+X, +Y, +Z)
	float2 tc0 = float2(-IN.objPos.y, -IN.objPos.x);
	float2 tc1 = IN.objPos.xz;
	float2 tc2 = float2(-IN.objPos.y,  IN.objPos.z);

	// Add up the diffuse texture contribution
	half4 c = 	tex2D(_MainTex, tc0) * contribution.z +
				tex2D(_MainTex, tc1) * contribution.y +
				tex2D(_MainTex, tc2) * contribution.x;

	// Take the main color into consideration
	c *= _Color;

	// Add up the normals
	float3 n =	UnpackNormal(
		tex2D(_BumpMap, tc0 * _TilingBump) * contribution.z +
		tex2D(_BumpMap, tc1 * _TilingBump) * contribution.y +
		tex2D(_BumpMap, tc2 * _TilingBump) * contribution.x);

	// Uhh... I don't know why I need to flip this. Z-up -> Y-up coordinate system translation perhaps?
	// I originally wrote this shader in GLSL for a Z-up coordinate system.
	n.y = -n.y;

	// Set the final values
	o.Albedo = c.rgb;
	o.Alpha = c.a;
	o.Normal = n;
}
ENDCG
	}
	Fallback "Game/Asteroid"
}