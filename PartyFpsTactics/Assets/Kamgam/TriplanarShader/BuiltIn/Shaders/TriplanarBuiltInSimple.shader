// Based on Ben Golus work which he kindly released into public domain in 2017.
// https://github.com/bgolus/Normal-Mapping-for-a-Triplanar-Shader/blob/master/TriplanarSurfaceShader.shader
// If you are reading this then you should buy him a root beer on https://ko-fi.com/bgolus

Shader "Triplanar/BuiltIn/Simple"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        
        _Blend("Blend", Range(0,40)) = 20

        _MainTex("Main", 2D) = "white" {}
		_Metallic("Metallic", Range(0,1)) = 0
		_Smoothness("Smoothness", Range(0,1)) = 0

		[HDR]_Emission("Emission", Color) = (0,0,0,0)
        _EmissionIntensity("Emission Intensity", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        #include "UnityStandardUtils.cginc"

        // flip UVs horizontally to correct for back side projection
        #define TRIPLANAR_CORRECT_PROJECTED_U

        fixed4 _Color;
        sampler2D _MainTex;
        float4 _MainTex_ST;

		float _Blend;
		float _Metallic;
		float _Smoothness;
		fixed4 _Emission;
        float _EmissionIntensity;

        struct Input {
            float3 worldPos;
            float3 worldNormal;
            INTERNAL_DATA
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Transform input values from human readable range to a usable range.
			float blend = pow((40.001 - _Blend)/10, 3);

            // calculate triplanar blend (skew results in favour of the top)
            float3 absWorldNormal = pow(abs(IN.worldNormal), blend);

            // Convert the world normal into a vector (x+y+z = 1). This ensures an even spread across all axis.
            float3 triblend = max(absWorldNormal / dot(absWorldNormal, half3(1, 1, 1)),0.0001);

            // calculate triplanar uvs
            // Applying texture scale á la TRANSFORM_TEX macro.
            float2 uvX = IN.worldPos.zy * _MainTex_ST.xy;
            float2 uvY = IN.worldPos.xz * _MainTex_ST.xy;
            float2 uvZ = IN.worldPos.xy * _MainTex_ST.xy;

            // minor optimization of sign(). prevents return value of 0
            half3 axisSign = IN.worldNormal < 0 ? -1 : 1;
            
            // flip UVs horizontally to correct for back side projection
        #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
            uvX.x *= axisSign.x;
            uvY.x *= axisSign.y;
            uvZ.x *= -axisSign.z;
        #endif

            // Apply offsets after uv flip.
            uvX += _MainTex_ST.zw;
            uvY += _MainTex_ST.zw;
            uvZ += _MainTex_ST.zw;

            // albedo textures
            fixed4 colX = tex2D(_MainTex, uvX);
            fixed4 colY = tex2D(_MainTex, uvY);
            fixed4 colZ = tex2D(_MainTex, uvZ);
            fixed4 col = colX * triblend.x + colY * triblend.y + colZ * triblend.z;

            // set surface ouput properties
            o.Albedo = col.rgb * _Color;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
			o.Emission = _Emission * _EmissionIntensity;
        }
        ENDCG
    }
    FallBack "Diffuse"
}