Shader "Hidden/Skinning/Replacement"
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
            #define SKINNER_POSITION
            #define SKINNER_NORMAL
            #define SKINNER_TANGENT
            #define SKINNER_MRT
            #include "Replacement.cginc"
            ENDCG
        }
    }
}
