// Based on Ben Golus work which he kindly released into public domain in 2017.
// https://github.com/bgolus/Normal-Mapping-for-a-Triplanar-Shader/blob/master/TriplanarSurfaceShader.shader
// If you are reading this then you should buy him a root beer on https://ko-fi.com/bgolus

Shader "GPUInstancer/Triplanar/Complex"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)

        _Blend("Blend", Range(0,40)) = 20

        _MainTex("Main", 2D) = "white" {}
		[NoScaleOffset] _MainMetallicMap("     Metallic", 2D) = "black" {}
		[NoScaleOffset] _MainNormalMap("     Normal", 2D) = "bump" {} 
		[NoScaleOffset] _MainOcclusionMap("     Occlusion", 2D) = "white" {} 
		
        _Smoothness("Smoothness", Range(0,1)) = 0.0
        _Metallic("Metallic", Range(0,1)) = 0.0
		_Occlusion("Occlusion", Range(0,1)) = 1.0
        [HDR]_Emission("Emission", Color) = (0,0,0,0)
        _EmissionIntensity("Emission Intensity", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
#include "UnityCG.cginc"
#include "./../../../../GPUInstancer/Shaders/Include/GPUInstancerInclude.cginc"
#pragma instancing_options procedural:setupGPUI
#pragma multi_compile_instancing
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

        sampler2D _MainNormalMap;
        sampler2D _MainOcclusionMap;
        sampler2D _MainMetallicMap;

        half _Smoothness;
		half _Metallic;
		half _Occlusion;
        fixed4 _Emission;
        float _EmissionIntensity;

		float _Blend;

        struct Input {
            float3 worldPos;
            float3 worldNormal;
            INTERNAL_DATA
        };

        // Reoriented Normal Mapping
        // http://blog.selfshadow.com/publications/blending-in-detail/
        // Altered to take normals (-1 to 1 ranges) rather than unsigned normal maps (0 to 1 ranges)
        half3 blend_rnm(half3 n1, half3 n2)
        {
            n1.z += 1;
            n2.xy = -n2.xy;

            return n1 * dot(n1, n2) / n1.z - n2;
        }

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
			float blend = pow((40.001 - _Blend)/10, 3);

            // Work around bug where IN.worldNormal is always (0,0,0)!
            // Happpens if o.Normal is written in the surface shader:
            // See: https://forum.unity.com/threads/calculating-worldnormal.1077641/#post-6949544
            IN.worldNormal = WorldNormalVector(IN, float3(0,0,1));

            // calculate triplanar blend (skew results in favour of the top)
            float3 absWorldNormal = pow(abs(IN.worldNormal), blend);

            // Convert the world normal into a vector (x+y+z = 1). This ensures an even spread across all axis.
            float3 triblend = max(absWorldNormal / dot(absWorldNormal, half3(1, 1, 1)),0.0001);

            // calculate triplanar uvs
            // Applying texture scale � la TRANSFORM_TEX macro.
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

            // occlusion textures (grey scale)
            half occX = tex2D(_MainOcclusionMap, uvX).g;
            half occY = tex2D(_MainOcclusionMap, uvY).g;
            half occZ = tex2D(_MainOcclusionMap, uvZ).g;
            half occ = LerpOneTo(occX * triblend.x + occY * triblend.y + occZ * triblend.z, _Occlusion);

            // metallic textures
            float4 metX = tex2D(_MainMetallicMap, uvX);
            float4 metY = tex2D(_MainMetallicMap, uvY);
            float4 metZ = tex2D(_MainMetallicMap, uvZ);
            // metallicity from red channel
            half met = (metX.r * triblend.x + metY.r * triblend.y + metZ.r * triblend.z) * _Metallic;
            // smoothness from alpha channel
            half smo = (metX.a * triblend.x + metY.a * triblend.y + metZ.a * triblend.z) * _Smoothness;

            // tangent space normal maps
            half3 tnormalX = UnpackNormal(tex2D(_MainNormalMap, uvX));
            half3 tnormalY = UnpackNormal(tex2D(_MainNormalMap, uvY));
            half3 tnormalZ = UnpackNormal(tex2D(_MainNormalMap, uvZ));

            // flip normal maps' x axis to account for flipped UVs
        #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
            tnormalX.x *= axisSign.x;
            tnormalY.x *= axisSign.y;
            tnormalZ.x *= -axisSign.z;
        #endif

            half3 absVertNormal = abs(IN.worldNormal);

            // swizzle world normals to match tangent space and apply reoriented normal mapping blend
            tnormalX = blend_rnm(half3(IN.worldNormal.zy, absVertNormal.x), tnormalX);
            tnormalY = blend_rnm(half3(IN.worldNormal.xz, absVertNormal.y), tnormalY);
            tnormalZ = blend_rnm(half3(IN.worldNormal.xy, absVertNormal.z), tnormalZ);

            // apply world space sign to tangent space Z
            tnormalX.z *= axisSign.x;
            tnormalY.z *= axisSign.y;
            tnormalZ.z *= axisSign.z;

            // sizzle tangent normals to match world normal and blend together
            half3 worldNormal = normalize(
                tnormalX.zyx * triblend.x +
                tnormalY.xzy * triblend.y +
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
