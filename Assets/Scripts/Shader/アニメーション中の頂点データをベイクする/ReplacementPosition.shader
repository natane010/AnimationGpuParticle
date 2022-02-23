Shader "Hidden/Skinning/ReplacementPosition"
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
            #include "Replacement.cginc"
            ENDCG
        }
    }
}
