// Based on Ben Golus work which he kindly released into public domain in 2017.
// https://github.com/bgolus/Normal-Mapping-for-a-Triplanar-Shader/blob/master/TriplanarSurfaceShader.shader
// If you are reading this then you should buy him a root beer on https://ko-fi.com/bgolus

Shader "Triplanar/BuiltIn/Complex Top Middle Bottom"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)

        _Blend("Blend", Range(0,40)) = 20
        _TopSkew("Top Skew", Range(0,100)) = 0

        _TopColor("Top Color", Color) = (1,1,1,1)
        _TopTex("Top (Y+)", 2D) = "white" {}
		[NoScaleOffset] _TopMetallicMap("     Metallic", 2D) = "black" {}
        _TopSmoothness("    Smoothness", Range(0,1)) = 0.3
        _TopMetallic("    Metallic", Range(0,1)) = 0.3
		[NoScaleOffset] _TopNormalMap("     Normal", 2D) = "bump" {} 
		[NoScaleOffset] _TopOcclusionMap("     Occlusion", 2D) = "white" {} 
        _TopOcclusion("    Occlusion", Range(0,1)) = 1.0

        _MainColor("Main Color", Color) = (1,1,1,1)
        _MainTex("Main (Z & X)", 2D) = "white" {}
		[NoScaleOffset] _MainMetallicMap("     Metallic", 2D) = "black" {}
        _MainSmoothness("    Smoothness", Range(0,1)) = 0.3
        _MainMetallic("    Metallic", Range(0,1)) = 0.3
		[NoScaleOffset] _MainNormalMap("     Normal", 2D) = "bump" {} 
		[NoScaleOffset] _MainOcclusionMap("     Occlusion", 2D) = "white" {} 
        _MainOcclusion("    Occlusion", Range(0,1)) = 1.0
		
        _BottomColor("Bottom Color", Color) = (1,1,1,1)
		_BottomTex("Bottom (Y-)", 2D) = "white" {}
		[NoScaleOffset] _BottomMetallicMap("     Metallic", 2D) = "black" {}
        _BottomSmoothness("    Smoothness", Range(0,1)) = 0.3
        _BottomMetallic("    Metallic", Range(0,1)) = 0.3
		[NoScaleOffset] _BottomNormalMap("     Normal", 2D) = "bump" {} 
		[NoScaleOffset] _BottomOcclusionMap("     Occlusion", 2D) = "white" {} 
        _BottomOcclusion("    Occlusion", Range(0,1)) = 1.0

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
        sampler2D _MainOcclusionMap;
        sampler2D _MainMetallicMap;

        sampler2D _TopNormalMap;
        sampler2D _TopOcclusionMap;
        sampler2D _TopMetallicMap;

        sampler2D _BottomNormalMap;
        sampler2D _BottomOcclusionMap;
        sampler2D _BottomMetallicMap;

        half _MainSmoothness;
		half _MainMetallic;
        half _MainOcclusion;

        half _TopSmoothness;
		half _TopMetallic;
        half _TopOcclusion;

        half _BottomSmoothness;
		half _BottomMetallic;
        half _BottomOcclusion;

        fixed4 _Emission;
        float _EmissionIntensity;
		
		float _Blend;
		half _TopSkew;
        fixed4 _Color;
        fixed4 _TopColor;
        fixed4 _MainColor;
        fixed4 _BottomColor;

        struct Input {
            float3 worldPos;
            float3 worldNormal;
            INTERNAL_DATA
        };

        float3 WorldToTangentNormalVector(Input IN, float3 normal)
        {
            float3 t2w0 = WorldNormalVector(IN, float3(1,0,0));
            float3 t2w1 = WorldNormalVector(IN, float3(0,1,0));
            float3 t2w2 = WorldNormalVector(IN, float3(0,0,1));
            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            return normalize(mul(t2w, normal));
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Transform input values from human readable range to a usable range.
			float skew = pow((_TopSkew + 9)/10, 5);
			float blend = pow((40.001 - _Blend)/10, 3);

            // work around bug where IN.worldNormal is always (0,0,0)!
            IN.worldNormal = WorldNormalVector(IN, float3(0,0,1));

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

            // blend to differentiate between top and bottom
            float triblendYPlus = max(0, axisSign.y * triblend.y);
            float triblendYMinus = max(0, -axisSign.y * triblend.y);

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

            // albedo textures
            fixed4 colX = tex2D(_MainTex, uvX);
            fixed4 colYPlus =  tex2D(_TopTex, uvYPlus);
            fixed4 colYMinus = tex2D(_BottomTex, uvYMinus);
            fixed4 colZ = tex2D(_MainTex, uvZ);
            fixed4 col = colX * triblend.x * _MainColor + 
                         colYPlus * triblendYPlus * _TopColor +
                         colYMinus * triblendYMinus * _BottomColor + 
                         colZ * triblend.z * _MainColor;

            // occlusion textures (grey scale)
            half occX = tex2D(_MainOcclusionMap, uvX).g;
            half occYPlus = tex2D(_TopOcclusionMap, uvYPlus).g;
            half occYMinus = tex2D(_BottomOcclusionMap, uvYMinus).g;
            half occZ = tex2D(_MainOcclusionMap, uvZ).g;
            half occ =  triblend.x * LerpOneTo(occX, _MainOcclusion) + 
                        triblendYPlus * LerpOneTo(occYPlus,_TopOcclusion) + 
                        triblendYMinus * LerpOneTo(occYMinus,_BottomOcclusion) + 
                        triblend.z * LerpOneTo(occZ, _MainOcclusion);

            // metallic textures
            float4 metX = tex2D(_MainMetallicMap, uvX);
            float4 metYPlus = tex2D(_TopMetallicMap, uvYPlus);
            float4 metYMinus = tex2D(_BottomMetallicMap, uvYMinus);
            float4 metZ = tex2D(_MainMetallicMap, uvZ);
            // metallicity from red channel
            half met = (metX.r * triblend.x * _MainMetallic +  
                        metYPlus.r * triblendYPlus * _TopMetallic +
                        metYMinus.r * triblendYMinus * _BottomMetallic +
                        metZ.r * triblend.z * _MainMetallic);
            // smoothness from alpha channel
            half smo = (metX.a * triblend.x * _MainSmoothness + 
                        metYPlus.a * triblendYPlus * _TopSmoothness + 
                        metYMinus.a * triblendYMinus * _BottomSmoothness + 
                        metZ.a * triblend.z * _MainSmoothness);

            // tangent space normal maps
            half3 tnormalX = UnpackNormal(tex2D(_MainNormalMap, uvX));
            half3 tnormalYPlus = UnpackNormal(tex2D(_TopNormalMap, uvYPlus));
            half3 tnormalYMinus = UnpackNormal(tex2D(_BottomNormalMap, uvYMinus));
            half3 tnormalZ = UnpackNormal(tex2D(_MainNormalMap, uvZ));


            // flip normal maps' x axis to account for flipped UVs
        #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
            tnormalX.x *= axisSign.x;
            tnormalYPlus.x *= axisSign.y;
            tnormalYMinus.x *= axisSign.y;
            tnormalZ.x *= -axisSign.z;
        #endif
        
            half3 absVertNormal = abs(IN.worldNormal);

            // swizzle world normals to match tangent space and apply reoriented normal mapping blend
            tnormalX = blend_rnm(half3(IN.worldNormal.zy, absVertNormal.x), tnormalX);
            tnormalYPlus = blend_rnm(half3(IN.worldNormal.xz, absVertNormal.y), tnormalYPlus);
            tnormalYMinus = blend_rnm(half3(IN.worldNormal.xz, absVertNormal.y), tnormalYMinus);
            tnormalZ = blend_rnm(half3(IN.worldNormal.xy, absVertNormal.z), tnormalZ);

            // apply world space sign to tangent space Z
            tnormalX.z *= axisSign.x;
            tnormalYPlus.z *= axisSign.y;
            tnormalYMinus.z *= axisSign.y;
            tnormalZ.z *= axisSign.z;

            // swizzle tangent normals to match world normal and blend together
            half3 worldNormal = normalize(
                tnormalX.zyx * triblend.x +
                tnormalYPlus.xzy * triblendYPlus +
                tnormalYMinus.xzy * triblendYMinus +
                tnormalZ.xyz * triblend.z
                );

            // set surface ouput properties
            o.Albedo = col.rgb * _Color;
            o.Metallic = met;
            o.Smoothness = smo;
            o.Occlusion = occ;
            o.Emission = _Emission * _EmissionIntensity;

            // convert world space normals into tangent normals
            o.Normal = WorldToTangentNormalVector(IN, worldNormal);
        }
        ENDCG
    }
    FallBack "Diffuse"
}