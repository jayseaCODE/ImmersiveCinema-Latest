// Based on Unlit shader. Simplest possible textured shader.
// - no lighting
// - no lightmap support
// - no per-material color
// http://docs.unity3d.com/Manual/SL-Shader.html

Shader "Custom/InsideVisible" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {} 
		//* defines a variable _MainTex which is seen in the Inspector as Base (RGB), to be a 2D texture
		// with white as its default, and white is a built-in texture
	}

	SubShader {
		Tags { "RenderType"="Opaque" } //These are name/value strings that communicate Pass' intent to the rendering engine
		Cull front //Added into to cull the front face
		LOD 100
		
		Pass {  
			CGPROGRAM
				#pragma vertex vert 
				   // this specifies the vert function as the vertex shader 
				#pragma fragment frag
				   // this specifies the frag function as the fragment shader
				#include "UnityCG.cginc"
				   // this includes all pre-defined input structures, 
				   // appdata_base; appdata_tan; and appdata_full, in Unity
				   // http://en.wikibooks.org/wiki/Cg_Programming/Unity/Debugging_of_Shaders
				   // http://docs.unity3d.com/Manual/ShaderTut2.html

				struct appdata_t {
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					half2 texcoord : TEXCOORD0;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				
				v2f vert (appdata_t v)
				{
					v2f o; //output vertex
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex); //UNITY_MATRIX_MVP is the current model*view*projection matrix
					v.texcoord.x = 1 - v.texcoord.x; //Added into
					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex); 
					//uses the TRANSFORM_TEX macro from UnityCG.cginc to make sure texture scale&offset is applied correctly
					return o;
				}
				
				fixed4 frag (v2f i) : SV_Target
				{
					fixed4 col = tex2D(_MainTex, i.texcoord); //samples the texture
					return col;
				}
			ENDCG
		}
	}
}
