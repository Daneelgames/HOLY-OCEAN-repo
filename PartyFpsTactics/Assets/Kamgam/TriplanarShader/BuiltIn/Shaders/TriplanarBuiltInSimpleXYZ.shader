// Based on Ben Golus work which he kindly released into public doy in 2017.
// https://github.com/bgolus/Normal-Mapping-for-a-Triplanar-Shader/blob/master/TriplanarSurfaceShader.shader
// If you are reading this then you should buy him a root beer on https://ko-fi.com/bgolus

Shader "Triplanar/BuiltIn/Simple XYZ"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)

        _Blend("Blend", Range(0,40)) = 20
        _TopSkew("Top Skew", Range(0,100)) = 0
        
        _XColor("X Color", Color) = (1,1,1,1)
        _XTex("X", 2D) = "white" {}
        _XSmoothness("    Smoothness", Range(0,1)) = 0
        _XMetallic("    Metallic", Range(0,1)) = 0

        _YColor("Y Color", Color) = (1,1,1,1)
        _YTex("Y", 2D) = "white" {}
        _YSmoothness("    Smoothness", Range(0,1)) = 0
        _YMetallic("    Metallic", Range(0,1)) = 0
		
        _ZColor("Z Color", Color) = (1,1,1,1)
		_ZTex("Z", 2D) = "white" {}
        _ZSmoothness("    Smoothness", Range(0,1)) = 0
        _ZMetallic("    Metallic", Range(0,1)) = 0

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

        sampler2D _YTex;
        float4 _YTex_ST;

        sampler2D _XTex;
        float4 _XTex_ST;

        sampler2D _ZTex;
        float4 _ZTex_ST;

        sampler2D _YNormalMap;

        sampler2D _XNormalMap;

        sampler2D _ZNormalMap;

        half _YSmoothness;
		half _YMetallic;

        half _XSmoothness;
		half _XMetallic;

        half _ZSmoothness;
		half _ZMetallic;
		
		float _Blend;
        half _TopSkew;
        fixed4 _Color;
        fixed4 _XColor;
        fixed4 _YColor;
        fixed4 _ZColor;

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

            // calculate triplanar uvs
            // Applying texture scale á la TRANSFORM_TEX macro.
            float2 uvX = IN.worldPos.zy * _XTex_ST.xy;
            float2 uvY = IN.worldPos.xz * _YTex_ST.xy;
            float2 uvZ = IN.worldPos.xy * _ZTex_ST.xy;

            // minor optimization of sign(). prevents return value of 0
            half3 axisSign = IN.worldNormal < 0 ? -1 : 1;

            // flip UVs horizontally to correct for back side projection
        #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
            uvX.x *= axisSign.x;
            uvY.x *= axisSign.y;
            uvZ.x *= -axisSign.z;
        #endif

            // Apply offsets after uv flip.
            uvX += _XTex_ST.zw;
            uvY += _YTex_ST.zw;
            uvZ += _ZTex_ST.zw;

            // albedo textures
            fixed4 colX = tex2D(_XTex, uvX);
            fixed4 colY = tex2D(_YTex, uvY);
            fixed4 colZ = tex2D(_ZTex, uvZ);
            fixed4 col = colX * triblend.x * _XColor + 
                         colY * triblend.y * _YColor +
                         colZ * triblend.z * _ZColor;

            // metallicity
            half met = (triblend.x * _XMetallic +  
                        triblend.y * _YMetallic +
                        triblend.z * _ZMetallic);
            // smoothness
            half smo = (triblend.x * _XSmoothness + 
                        triblend.y * _YSmoothness + 
                        triblend.z * _ZSmoothness);

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