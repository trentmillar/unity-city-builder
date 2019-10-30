Shader "Game/Distortion (Two-sided)"
{
	Properties
	{
		_MainTex ("Distortion (RG), Noise (B)", 2D) = "white" {}
		_Color ("Color Tint", Color) = (1,1,1,1)
		_Distortion ("Strength", Range(0.0, 0.3)) = 0.2
		_Focus ("Focus", Range(0.0, 1.0)) = 0.3
	}

	Category
	{
		Tags { "Queue" = "Transparent+10" }
		
		SubShader
		{
			LOD 300

			GrabPass
			{
				Name "BASE"
				Tags { "LightMode" = "Always" }
			}
		   
			Pass
			{
				Lighting Off
				Cull Off
				ZWrite Off
				ZTest LEqual
				Blend SrcAlpha OneMinusSrcAlpha
				//Blend SrcAlpha One
				AlphaTest Greater 0

				CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members distortion)
#pragma exclude_renderers d3d11 xbox360
				#pragma exclude_renderers xbox360
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma fragmentoption ARB_fog_exp2

				#include "UnityCG.cginc"

				sampler2D _GrabTexture : register(s0);
				sampler2D _MainTex;
				float4 _MainTex_ST;
				float4 _Color;
				float _Distortion;
				float _Focus;

				struct data
				{
					float4 vertex : POSITION;
					float3 normal : NORMAL;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 position : POSITION;
					float2 uv : TEXCOORD0;
					float4 screenPos : TEXCOORD1;
					float distortion;
				};

				v2f vert (data i)
				{
					v2f o;

					// Vertex position
					o.position = mul(UNITY_MATRIX_MVP, i.vertex);

					// Screen position (for screen space texture coordinates)
					o.screenPos = o.position;

					// Texture coordinates
					o.uv = TRANSFORM_TEX(i.texcoord, _MainTex);

					// Distortion should be affected by the normal (fade it out as the dot product reaches 0
					float eyeDot = dot(normalize(ObjSpaceViewDir(i.vertex)), i.normal);
					o.distortion = eyeDot * eyeDot;

					// Scale the distortion with vertex depth so it fades out with distance
					float depth = mul( UNITY_MATRIX_MV, i.vertex ).z;
					o.distortion /= depth = 1.0 - (depth * 0.5 + 0.5);
					o.distortion *= _Distortion;
					return o;
				}

				half4 frag (v2f i) : COLOR
				{	
					// Screen space texture coordinates
					float2 screenPos = i.screenPos.xy / i.screenPos.w;
					screenPos = screenPos * 0.5 + 0.5;
					screenPos.y = 1.0 - screenPos.y;

					// Sample the distortion texture
					half4 offsetColor1 = tex2D(_MainTex, i.uv + _Time.xz);
					half4 offsetColor2 = tex2D(_MainTex, i.uv - _Time.yx);
					
					// Distort the coordinates
					screenPos.x += (offsetColor1.r + offsetColor2.r - 1.0) * i.distortion;
					screenPos.y += (offsetColor1.g + offsetColor2.g - 1.0) * i.distortion;

					// Sample the screen buffer at the distorted coordinates
					half4 col = tex2D( _GrabTexture, screenPos ) * _Color;

					// We want to smoothly fade out the distortion as the dot product nears 0
					col.a *= i.distortion * _Focus * _Focus * 100.0;
					return col;
				}

				ENDCG
			}
		}
		
		SubShader
		{
			LOD 200

			Pass
			{
				Lighting Off
				Cull Off
				ZWrite Off
				ZTest LEqual
				Blend SrcAlpha OneMinusSrcAlpha
				//Blend SrcAlpha One
				AlphaTest Greater 0

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma fragmentoption ARB_fog_exp2

				#include "UnityCG.cginc"

				sampler2D _MainTex;
				float4 _MainTex_ST;
				float4 _Color;

				struct data
				{
					float4 vertex : POSITION;
					float3 normal : NORMAL;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 position : POSITION;
					float4 screenPos : TEXCOORD1;
				};

				v2f vert (data i)
				{
					v2f o;

					// Vertex position
					o.position = mul(UNITY_MATRIX_MVP, i.vertex);

					// Screen position (for screen space texture coordinates)
					o.screenPos = o.position;
					o.screenPos.z = dot(normalize(ObjSpaceViewDir(i.vertex)), i.normal);
					return o;
				}

				half4 frag (v2f i) : COLOR
				{	
					// Screen space texture coordinates
					float2 screenPos = i.screenPos.xy / i.screenPos.w;
					screenPos = screenPos * 0.5 + 0.5;
					screenPos.y = 1.0 - screenPos.y;

					// Time-based movement
					screenPos = screenPos * float2(4.0, 12.0) + float2(_Time.w * 3.0);

					half4 col = _Color * (0.25 * tex2D(_MainTex, screenPos).b + 0.5);
					col.a *= i.screenPos.z;
					return col;
				}

				ENDCG
			}
		}
	}
	Fallback Off
}
