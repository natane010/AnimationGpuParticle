Shader "Hidden/Skinning/ReplacementTangent"
{
    SubShader
    {
        Tags { "Skinning" = "Source" }
        Pass
        {
            ZTest Always ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #define SKINNER_TANGENT
            #include "Replacement.cginc"
            ENDCG
        }
    }
}