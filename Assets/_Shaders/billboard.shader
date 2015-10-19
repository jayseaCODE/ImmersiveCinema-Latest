Shader "Custom/billboard" {
    Properties {
        //_MainTex ("Texture Image", 2D) = "white" {}
        //_SpriteSize("SpriteSize", Float) = 10
    }
    SubShader {
        Pass{
        	ZTest Always Cull Off ZWrite Off
            Fog { Mode off }
            
	        CGPROGRAM
	        #pragma target 5.0
	        #pragma vertex vert
	        #pragma fragment frag
	        #include "UnityCG.cginc"
	        
	        uniform StructuredBuffer<float4> buffer;
	       
	        //sampler2D _MainTex;
	        //fixed4 _TintColor;
	        //float _SpriteSize;
	       
	        struct vertexInput {
	            float4 pos : SV_POSITION;
	            //fixed4 col : COLOR;
	            //float2 UV : TEXCOORD0;
	        };
	 
	        struct fragmentInput {
	            float4 pos : SV_POSITION;
	            //float size : PSIZE;
	            //fixed4 col : COLOR;
	            //float2 UV: TEXCOORD0;
	        };
	             
	        fragmentInput vert(uint input_id : SV_VertexID) {
	           
	            float4 pos = float4(buffer[input_id]);
	            fragmentInput output = (fragmentInput)0;
	           
	            output.pos = mul(UNITY_MATRIX_MVP, pos);
	            //output.col = input.col;
	            //output.size = _SpriteSize;
	            //output.UV = input.UV;
	            return output;
	        }
	       
	        float4 frag(fragmentInput input) : COLOR {  
	            return float4(0,0,1.0,1.0); //tex2D(_MainTex, float2(input.UV.x / _SpriteSize,input.UV.y / _SpriteSize));
	        }
	        ENDCG
    }
   
    }
}
