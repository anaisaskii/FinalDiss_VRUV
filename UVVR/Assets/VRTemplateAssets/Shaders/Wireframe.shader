//with help from https://github.com/Firnox/ShaderStories-Wireframe/blob/main/Assets/Shaders/WireframeQuad.shader
//and https://www.patreon.com/posts/fun-with-shaders-103077771

Shader "Unlit/WireframeQuadSimulated"
{
    Properties
    {
        _WireThickness("Wire Thickness", Range(0.001, 10.0)) = 0.02
        _WireColor("Wire Color", Color) = (0,1,1,1)
        _BaseColor("Base Color", Color) = (1,1,1,1)
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float _WireThickness;
            float4 _WireColor;
            float4 _BaseColor;

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct v2g
            {
                float4 positionCS : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            struct g2f
            {
                float4 positionCS : SV_POSITION;
                float3 barycentric : TEXCOORD0;
            };

            v2g vert(Attributes v)
            {
                v2g o;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = vertexInput.positionCS;
                o.worldPos = vertexInput.positionWS;
                return o;
            }

            [maxvertexcount(4)]
            void geom(triangle v2g IN[3], inout TriangleStream<g2f> triStream)
            {
                g2f o;

                // Compute edge lengths
                float edgeLengthX = length(IN[1].worldPos - IN[2].worldPos);
                float edgeLengthY = length(IN[0].worldPos - IN[2].worldPos);
                float edgeLengthZ = length(IN[0].worldPos - IN[1].worldPos);

                float3 modifier = float3(0.0, 0.0, 0.0);
                if ((edgeLengthX > edgeLengthY) && (edgeLengthX > edgeLengthZ)) {
                    modifier = float3(1.0, 0.0, 0.0);
                }
                else if ((edgeLengthY > edgeLengthX) && (edgeLengthY > edgeLengthZ)) {
                    modifier = float3(0.0, 1.0, 0.0);
                }
                else {
                    modifier = float3(0.0, 0.0, 1.0);
                }

                for (int i = 0; i < 3; i++)
                {
                    o.positionCS = IN[i].positionCS;
                    o.barycentric = float3(i == 0, i == 1, i == 2) + modifier;
                    triStream.Append(o);
                }
            }

            half4 frag(g2f i) : SV_Target
            {
                float3 unitWidth = fwidth(i.barycentric);
                float3 aliased = smoothstep(float3(0.0, 0.0, 0.0), unitWidth * _WireThickness, i.barycentric);
                float wire = 1 - min(aliased.x, min(aliased.y, aliased.z));
                return lerp(_WireColor, _BaseColor, wire);
            }
            ENDHLSL
        }
    }
}