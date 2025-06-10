Shader "Custom/UnlitFrontOfOpaque"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,0.3)
    }
    SubShader
    {
        Tags { "Queue"="Transparent+100" "RenderType"="Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off

        Pass
        {
            Color[_Color]
        }
    }
}
