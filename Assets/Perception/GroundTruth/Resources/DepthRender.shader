Shader "Perception/DepthRender"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        ZWrite Off Cull Off
        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            #pragma vertex Vert
            #pragma fragment Frag

            float _Near;
            float _Far;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float rawDepth = SampleSceneDepth(input.uv);
                float linearDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                
                float gray = saturate((linearDepth - _Near) / (_Far - _Near));

                return half4(gray, gray, gray, 1.0);
            }
            ENDHLSL
        }
    }
}