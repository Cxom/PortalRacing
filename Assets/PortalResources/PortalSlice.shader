Shader "Custom/PortalSlice"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0, 1)) = 0.0
        
        sliceNormal("normal", Vector) = (0,0,0,0)
        sliceCenter("center", Vector) = (0,0,0,0)
        sliceOffsetDst("offset", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Geometry" "IgnoreProjector"="True" "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard addshadow
        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        
        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // World spae normal of slice, anything along this direction from center will be invisible
        float3 sliceCenter;
        // World space center of slice
        float3 sliceNormal;
        // Increasing makes more of the mesh visible
        float sliceOffsetDst;

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float3 adjustedCenter = sliceCenter + sliceNormal * sliceOffsetDst;
            float3 offsetToSliceCenter = adjustedCenter - IN.worldPos;
            
            float sliceSide = dot(sliceNormal, offsetToSliceCenter);
            clip(sliceSide);
            
            fixed4 color = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = color.rbg;

            // Metallic and smoothness from sliders
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = color.a;
        }
        ENDCG
    }
    FallBack "VertexLit"
}
