Shader "Game/Spaceship"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		_EmissiveColor ("Emissive Color", Color) = (0.0, 0.0, 0.0, 1)
		_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
		_MainTex ("Diffuse (RGB), Glow (A)", 2D) = "white" {}
		_NormalMapTex ("Normal Map (RGB), Specular (A)", 2D) = "" {}
	}
	
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 300
		
CGPROGRAM
#pragma surface surf PPL

sampler2D _MainTex;
sampler2D _NormalMapTex;
fixed4 _Color;
fixed4 _EmissiveColor;
float _Shininess;

struct Input
{
	float2 uv_MainTex;
	float2 uv_NormalMapTex;
};

// Forward lighting
half4 LightingPPL (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
{
	half3 nNormal = normalize(s.Normal);
	half shininess = s.Gloss * 250.0 + 4.0;

#ifndef USING_DIRECTIONAL_LIGHT
	lightDir = normalize(lightDir);
#endif

	// Phong shading model
	half reflectiveFactor = max(0.0, dot(-viewDir, reflect(lightDir, nNormal)));

	// Blinn-Phong shading model
	//half reflectiveFactor = max(0.0, dot(nNormal, normalize(lightDir + viewDir)));
	
	half diffuseFactor = max(0.0, dot(nNormal, lightDir));
	half specularFactor = pow(reflectiveFactor, shininess) * s.Specular;

	half4 c;
	c.rgb = (s.Albedo * diffuseFactor + _SpecColor * specularFactor) * _LightColor0.rgb;
	c.rgb *= (atten * 2.0);
	c.a = s.Alpha;
	return c;
}

void surf (Input IN, inout SurfaceOutput o)
{
	half4 tex 	= tex2D(_MainTex, IN.uv_MainTex) * _Color;
	half4 nm 	= tex2D(_NormalMapTex, IN.uv_NormalMapTex);
	o.Albedo 	= tex.rgb;
	o.Alpha 	= _Color.a;
	o.Normal 	= nm.rgb * 2.0 - 1.0;
	o.Specular 	= nm.a;
	o.Emission	= _EmissiveColor * (tex.a * 2.0);
	o.Gloss 	= _Shininess;
}
ENDCG
	}
	Fallback "VertexLit"
}