Shader "Custom/UI_SpriteUnderMask" {

	Properties 
	{
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	}

	SubShader 
	{
		Tags {"Queue"="Transparent+2" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 100
		
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha 
		Cull Off

		BindChannels 
		{
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
			Bind "Color", color
		}

		Pass 
		{
			Lighting Off
			SetTexture [_MainTex] { combine texture * primary } 
		}
	}
}