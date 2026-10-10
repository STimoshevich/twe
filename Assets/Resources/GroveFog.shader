Shader "TwilightGrove/Exploration"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.12
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Standard finalcolor:ApplyExploration fullforwardshadows
        #pragma target 3.0
        fixed4 _Color;
        half _Glossiness;
        sampler2D _GroveExplored;
        float4 _GroveBounds;
        float4 _GroveViewer;
        float _GroveFogReady;
        struct Input { float3 worldPos; };
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = _Color.rgb;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;
        }
        void ApplyExploration(Input IN, SurfaceOutputStandard o, inout fixed4 color)
        {
            if (_GroveFogReady < 0.5) return;
            float2 uv = (IN.worldPos.xz - _GroveBounds.xy) / _GroveBounds.zw;
            float explored = tex2D(_GroveExplored, saturate(uv)).r;
            float visible = 1 - smoothstep(_GroveViewer.z - 0.75, _GroveViewer.z,
                distance(IN.worldPos.xz, _GroveViewer.xy));
            float known = max(explored, visible);
            #ifdef UNITY_PASS_FORWARDADD
            color.rgb *= known * lerp(0.32,1,visible);
            #else
            color.rgb = lerp(float3(0.018,0.025,0.04), color.rgb * lerp(0.32,1,visible), known);
            #endif
        }
        ENDCG
    }
    Fallback "Standard"
}
