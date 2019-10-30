Shader "Game/Plasma Beam"
{
	Properties
	{
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_Color ("Tint Color", Color) = (1,1,1,1)
	}
	
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		
		LOD 100
		ZWrite Off
		Blend SrcAlpha One
		Cull Off
		
		Pass
		{
			ColorMaterial AmbientAndDiffuse
			Lighting Off
			
			SetTexture [_MainTex]
			{
				Combine Texture * Primary
			}
			
			SetTexture [_MainTex]
			{
				ConstantColor [_Color]
				Combine Constant * Previous DOUBLE
			}
		}
	}
}