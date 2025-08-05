Shader "Custom/WorldMaskDecal"
{
    Properties
    {
        _MainTex    ("Base (RGB)", 2D) = "white" {}
        _MaskTex    ("Mask (R)", 2D)  = "white" {}
        _TintColor  ("Tint Color", Color) = (1,0,0,1)
        _Tile       ("Tile", Float) = 10
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Standard

        sampler2D _MainTex;
        sampler2D _MaskTex;
        float4   _TintColor;
        float    _Tile;

        struct Input {
            float2 uv_MainTex;
            float2 uv2_MaskTex;
        };

        void surf (Input IN, inout SurfaceOutputStandard o) {
            // UV0 に Tile をかけて繰り返し
            float2 tiledUV = IN.uv_MainTex * _Tile;
            float4 baseCol = tex2D(_MainTex, tiledUV);
            // UV2 は 0–1 のままマスクを一度だけ
            float mask = tex2D(_MaskTex, IN.uv2_MaskTex).r;
            baseCol.rgb = lerp(baseCol.rgb, _TintColor.rgb, mask);
            o.Albedo = baseCol.rgb;
            o.Alpha  = baseCol.a;
        }
        ENDCG
    }
}
