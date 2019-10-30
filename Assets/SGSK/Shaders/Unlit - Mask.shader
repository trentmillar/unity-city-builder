Shader "Unlit/Mask"
{
	Properties
	{
		_Color ("Color0", Color) = (1,1,1,1)
		_Color1 ("Color1", Color) = (1,1,1,1)
		_Color2 ("Color2", Color) = (1,1,1,1)
		_MainTex ("Greyscale (R), Mask 1 (G), Mask 2 (B), Alpha(A)", 2D) = "white" {}
	}

	SubShader
	{
		Tags
		{
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
		}
		LOD 100
		
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha 
		
		Pass
		{
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

half4 _Color, _Color1, _Color2;
sampler2D _MainTex;
half4 _MainTex_ST;

struct appdata
{
	half4 vertex : POSITION;
	half4 texcoord : TEXCOORD0;
};

struct v2f
{
	float4 vertex : POSITION;
	float2 texcoord : TEXCOORD0;
};

v2f vert (appdata v)
{
	v2f o;
	o.vertex = mul( UNITY_MATRIX_MVP, v.vertex );
	o.texcoord = float2( v.texcoord.xy );
	return o;
}

half4 frag (v2f i) : COLOR
{
	half4 tex = tex2D(_MainTex, i.texcoord);
	half4 diff = _Color;
	diff = lerp(diff, _Color1, tex.g);
	diff = lerp(diff, _Color2, tex.b);
	diff *= tex.r;
	diff.a = tex.a;
	return diff;
}
ENDCG
		}
	}
	FallBack "Diffuse"
}
