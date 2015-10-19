Shader "Custom/shader7" {
 	SubShader
    {
       Pass
       {
            ZTest Always Cull Off ZWrite Off
            Fog { Mode off }
 
            CGPROGRAM
            #include "UnityCG.cginc"
            #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag
            // buffers are only in DirectCompute, so must set shader target to 5.0
 
            uniform StructuredBuffer<float3> buffer; //declaring the buffer which stores the data
            uniform float4x4 cameraToWorldMatrix;
 
            struct v2f
            {
                float4  pos : SV_POSITION;
            };
 
            v2f vert(uint id : SV_VertexID) //vertex shader
            {
                float4 pos = float4(buffer[id], 1);
 				float4 pos_in_world_space = mul(cameraToWorldMatrix, pos);
                v2f OUT;
                OUT.pos = mul(UNITY_MATRIX_MVP, pos_in_world_space);
                return OUT;
            }
 
            float4 frag(v2f IN) : COLOR
            {
                return float4(1,0,0,1);
            }
 
            ENDCG
        }
    }
}
