Shader "TwilightGrove/Exploration"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Painted albedo", 2D) = "white" {}
        _PathTex ("Trail albedo", 2D) = "white" {}
        _TerrainBlend ("Blend terrain using vertex alpha", Range(0,1)) = 0
        _Glossiness ("Smoothness", Range(0,1)) = 0.12
        _VertexTint ("Use mesh painted colors", Range(0,1)) = 0
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
        half _VertexTint;
        sampler2D _GroveExplored;
        sampler2D _MainTex;
        sampler2D _PathTex;
        half _TerrainBlend;
        float4 _GroveBounds;
        float4 _GroveViewer;
        float _GroveFogReady;
        struct Input { float3 worldPos; float2 uv_MainTex; float4 color : COLOR; };
        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed3 baseColor=tex2D(_MainTex, IN.uv_MainTex).rgb;
            if(_TerrainBlend > 0.5)
                baseColor=lerp(baseColor,tex2D(_PathTex, IN.uv_MainTex).rgb,saturate(IN.color.a));
            o.Albedo = baseColor * _Color.rgb * lerp(float3(1,1,1), IN.color.rgb, _VertexTint);
            o.Smoothness = _Glossiness;
            o.Alpha = 1;
        }
        void ApplyExploration(Input IN, SurfaceOutputStandard o, inout fixed4 color)
        {
            if (_GroveFogReady < 0.5) return;
            float2 uv = (IN.worldPos.xz - _GroveBounds.xy) / _GroveBounds.zw;
            float explored = tex2D(_GroveExplored, saturate(uv)).r;
            float visible = 1 - smoothstep(_GroveViewer.z - max(_GroveViewer.w, 0.1), _GroveViewer.z,
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
