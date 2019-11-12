// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Nemo2D_Wave" {
	Properties {
		_MainTex ("Main Texture", 2D) = "white" {} 
		_DistScal ("DisortScale",Float) = 1 
	}
	SubShader {
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Opaque"}
		
		Lighting Off ZWrite Off Fog { Mode Off }
		//Blend SrcAlpha OneMinusSrcAlpha
		Blend SrcAlpha OneMinusSrcAlpha
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			uniform float4 _MainTex_ST;  
			float _DistScal;
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
				float4 vertLocalPos = v.vertex;
				//vertLocalPos += float4(10,0,10,0);
				vertLocalPos.x =  v.vertex.x + cos(_Time.y*3f + (v.vertex.x+v.vertex.z)/2f)*_DistScal ;
				 
				vertLocalPos.z =  v.vertex.z + sin(_Time.y*3f + (v.vertex.x+v.vertex.z)/2f)*_DistScal ;
				
				//vertLocalPos += v.vertex;
				o.pos = UnityObjectToClipPos (vertLocalPos); 
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex); 
				return o; 
			}

			half4 frag (v2f i) : COLOR
			{
				 
				return  tex2D(_MainTex,i.texcoord)  ;
			}
			ENDCG

		}
		 
	} 
} 

