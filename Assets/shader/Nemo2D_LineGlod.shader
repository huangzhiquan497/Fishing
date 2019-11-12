// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Nemo2D_LineGlod" {
Properties {
		_MainTex ("Main Texture", 2D) = "white" {} 
		_Color("Main Color",Color) = (1,1,1,1)
		_TexcoordScalOffset ("TexCoordScalOffset",Vector) =  (1,1,0,0)
	}
	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Opaque"}
		
		Lighting Off ZWrite Off Fog { Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha
		//Blend One One 
		Pass {
	 
	 
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			uniform float4 _MainTex_ST;  
		 
			float4 _TexcoordScalOffset; 
			float4 _Color;
			struct appdata_t {
				float4 vertex : POSITION; 
				float2 texcoord : TEXCOORD0; 
			};
			
			struct v2f {
				float4 pos : POSITION; 
				float2 texcoord : TEXCOORD0; 
			};

			v2f vert (appdata_t v)
			{
				v2f o; 
				o.pos = UnityObjectToClipPos (v.vertex); 
				o.texcoord = v.vertex.xy*_TexcoordScalOffset.xy + _TexcoordScalOffset.zw;//TRANSFORM_TEX(v.texcoord,_MainTex); 
				return o; 
			}

			half4 frag (v2f i) : COLOR
			{
				 
				return  tex2D(_MainTex,i.texcoord)*_Color ;
			}
			ENDCG

		}
		 
	} 
} 
