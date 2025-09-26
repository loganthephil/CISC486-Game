Shader "Custom/Outline"
{
    Properties
    {
        [HDR] [PerRendererData] _OutlineColor("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineWidth("Outline Width", Range(0.0, 1.0)) = 0.02
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                half3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            // Per material properties
            CBUFFER_START(UnityPerMaterial)
                half _OutlineWidth;
            CBUFFER_END

            // Per instance properties
            UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
                UNITY_DEFINE_INSTANCED_PROP(half4, _OutlineColor)
            UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // Move vertex along vertex position in object space.
                IN.positionOS.xyz += IN.positionOS.xyz * _OutlineWidth;

                // Transform vertex from object space to clip space.
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);

                // Transform normal vector from object space to clip space.
                float3 normalHCS = mul((float3x3)UNITY_MATRIX_VP, mul((float3x3)UNITY_MATRIX_M, IN.normalOS));

                // Move vertex along normal vector in clip space.
                OUT.positionHCS.xy += normalize(normalHCS.xy) / _ScreenParams.xy * OUT.positionHCS.w * _OutlineWidth * 2;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _OutlineColor);
            }
            ENDHLSL
        }
    }
}