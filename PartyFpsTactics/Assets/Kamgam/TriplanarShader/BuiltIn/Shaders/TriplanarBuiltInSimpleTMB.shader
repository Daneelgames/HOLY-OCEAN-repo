// Based on Ben Golus work which he kindly released into public domain in 2017.
// https://github.com/bgolus/Normal-Mapping-for-a-Triplanar-Shader/blob/master/TriplanarSurfaceShader.shader
// If you are reading this then you should buy him a root beer on https://ko-fi.com/bgolus

Shader "Triplanar/BuiltIn/Simple Top Middle Bottom"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)

        _Blend("Blend", Range(0,40)) = 20
        _TopSkew("Top Skew", Range(0,100)) = 0

        _TopColor("Top Color", Color) = (1,1,1,1)
        _TopTex("Top (Y+)", 2D) = "white" {}
        _TopSmoothness("    Smoothness", Range(0,1)) = 0
        _TopMetallic("    Metallic", Range(0,1)) = 0

        _MainColor("Main Color", Color) = (1,1,1,1)
        _MainTex("Main (Z & X)", 2D) = "white" {}
        _MainSmoothness("    Smoothness", Range(0,1)) = 0
        _MainMetallic("    Metallic", Range(0,1)) = 0
		
        _BottomColor("Bottom Color", Color) = (1,1,1,1)
		_BottomTex("Bottom (Y-)", 2D) = "white" {}
        _BottomSmoothness("    Smoothness", Range(0,1)) = 0
        _BottomMetallic("    Metallic", Range(0,1)) = 0

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

        // Reoriented Normal Mapping
        // http://blog.selfshadow.com/publications/blending-in-detail/
        // Altered to take normals (-1 to 1 ranges) rather than unsigned normal maps (0 to 1 ranges)
        half3 blend_rnm(half3 n1, half3 n2)
        {
            n1.z += 1;
            n2.xy = -n2.xy;

            return n1 * dot(n1, n2) / n1.z - n2;
        }

        sampler2D _MainTex;
        float4 _MainTex_ST;

        sampler2D _TopTex;
        float4 _TopTex_ST;

        sampler2D _BottomTex;
        float4 _BottomTex_ST;

        sampler2D _MainNormalMap;

        sampler2D _TopNormalMap;

        sampler2D _BottomNormalMap;

        half _MainSmoothness;
		half _MainMetallic;

        half _TopSmoothness;
		half _TopMetallic;

        half _BottomSmoothness;
		half _BottomMetallic;
		
		float _Blend;
		half _TopSkew;
        fixed4 _Color;
        fixed4 _TopColor;
        fixed4 _MainColor;
        fixed4 _BottomColor;

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
			float skew = pow((_TopSkew + 9)/10, 5);
			float blend = pow((40.001 - _Blend)/10, 3);

            // calculate triplanar blend
            float3 absWorldNormal = pow(abs(IN.worldNormal), blend); 
			skew *= sign(IN.worldNormal.y);
			absWorldNormal.y *= max(1, skew);
			absWorldNormal.x /= max(1, skew);
			absWorldNormal.z /= max(1, skew);

            // Convert the world normal into a normal whose sum (x+y+z) is always 1.
            float3 triblend = max(absWorldNormal / dot(absWorldNormal, half3(1, 1, 1)),0.0001);

            // minor optimization of sign(). prevents return value of 0
            half3 axisSign = IN.worldNormal < 0 ? -1 : 1;

            // calculate triplanar uvs
            // Applying texture scale á la TRANSFORM_TEX macro.
            float2 uvX = IN.worldPos.zy * _MainTex_ST.xy;
            float2 uvYPlus = IN.worldPos.xz * _TopTex_ST.xy;
            float2 uvYMinus = IN.worldPos.xz * _BottomTex_ST.xy;
            float2 uvZ = IN.worldPos.xy * _MainTex_ST.xy;

            // flip UVs horizontally to correct for back side projection
        #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
            uvX.x *= axisSign.x;
            uvYPlus.x *= axisSign.y;
            uvYMinus.x *= axisSign.y;
            uvZ.x *= -axisSign.z;
        #endif

            // Apply offsets after uv flip.
            uvX += _MainTex_ST.zw;
            uvYPlus += _TopTex_ST.zw;
            uvYMinus += _BottomTex_ST.zw;
            uvZ += _MainTex_ST.zw;

            float triblendYPlus = max(0, axisSign.y * triblend.y);
            float triblendYMinus = max(0, -axisSign.y * triblend.y);

            // albedo textures
            fixed4 colX = tex2D(_MainTex, uvX);
            fixed4 colYPlus =  tex2D(_TopTex, uvYPlus);
            fixed4 colYMinus = tex2D(_BottomTex, uvYMinus);
            fixed4 colZ = tex2D(_MainTex, uvZ);
            fixed4 col = colX * triblend.x * _MainColor + 
                         colYPlus * triblendYPlus * _TopColor +
                         colYMinus * triblendYMinus * _BottomColor + 
                         colZ * triblend.z * _MainColor;

            // metallic
            half met = (triblend.x * _MainMetallic +  
                        triblendYPlus * _TopMetallic +
                        triblendYMinus * _BottomMetallic +
                        triblend.z * _MainMetallic);
            // smoothness
            half smo = (triblend.x * _MainSmoothness + 
                        triblendYPlus * _TopSmoothness + 
                        triblendYMinus * _BottomSmoothness + 
                        triblend.z * _MainSmoothness);

            // set surface ouput properties
            o.Albedo = col.rgb * _Color;
            o.Metallic = met;
            o.Smoothness = smo;
            o.Emission = _Emission * _EmissionIntensity;
        }
        ENDCG
    }
    FallBack "Diffuse"
}