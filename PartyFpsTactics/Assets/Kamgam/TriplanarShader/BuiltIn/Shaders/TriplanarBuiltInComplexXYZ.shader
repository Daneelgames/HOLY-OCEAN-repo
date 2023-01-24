// Based on Ben Golus work which he kindly released into public doy in 2017.
// https://github.com/bgolus/Normal-Mapping-for-a-Triplanar-Shader/blob/master/TriplanarSurfaceShader.shader
// If you are reading this then you should buy him a root beer on https://ko-fi.com/bgolus

Shader "Triplanar/BuiltIn/Complex XYZ"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)

        _Blend("Blend", Range(0,40)) = 20
        _TopSkew("Top Skew", Range(0,100)) = 0

        _XColor("X Color", Color) = (1,1,1,1)
        _XTex("X", 2D) = "white" {}
		[NoScaleOffset] _XMetallicMap("     Metallic", 2D) = "black" {}
        _XSmoothness("    Smoothness", Range(0,1)) = 0.3
        _XMetallic("    Metallic", Range(0,1)) = 0.3
		[NoScaleOffset] _XNormalMap("     Normal", 2D) = "bump" {} 
		[NoScaleOffset] _XOcclusionMap("     Occlusion", 2D) = "white" {} 
        _XOcclusion("    Occlusion", Range(0,1)) = 1.0

        _YColor("Y Color", Color) = (1,1,1,1)
        _YTex("Y", 2D) = "white" {}
		[NoScaleOffset] _YMetallicMap("     Metallic", 2D) = "black" {}
        _YSmoothness("    Smoothness", Range(0,1)) = 0.3
        _YMetallic("    Metallic", Range(0,1)) = 0.3
		[NoScaleOffset] _YNormalMap("     Normal", 2D) = "bump" {} 
		[NoScaleOffset] _YOcclusionMap("     Occlusion", 2D) = "white" {} 
        _YOcclusion("    Occlusion", Range(0,1)) = 1.0
		
        _ZColor("Z Color", Color) = (1,1,1,1)
		_ZTex("Z", 2D) = "white" {}
		[NoScaleOffset] _ZMetallicMap("     Metallic", 2D) = "black" {}
        _ZSmoothness("    Smoothness", Range(0,1)) = 0.3
        _ZMetallic("    Metallic", Range(0,1)) = 0.3
		[NoScaleOffset] _ZNormalMap("     Normal", 2D) = "bump" {} 
		[NoScaleOffset] _ZOcclusionMap("     Occlusion", 2D) = "white" {} 
        _ZOcclusion("    Occlusion", Range(0,1)) = 1.0

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
        sampler2D _YOcclusionMap;
        sampler2D _YMetallicMap;

        sampler2D _XNormalMap;
        sampler2D _XOcclusionMap;
        sampler2D _XMetallicMap;

        sampler2D _ZNormalMap;
        sampler2D _ZOcclusionMap;
        sampler2D _ZMetallicMap;

        half _YSmoothness;
		half _YMetallic;
        half _YOcclusion;

        half _XSmoothness;
		half _XMetallic;
        half _XOcclusion;

        half _ZSmoothness;
		half _ZMetallic;
        half _ZOcclusion;
		
        fixed4 _Emission;
        float _EmissionIntensity;

        fixed4 _Color;
        fixed4 _XColor;
        fixed4 _YColor;
        fixed4 _ZColor;

		float _Blend;
        half _TopSkew;

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

            // occlusion textures (value from green channel)
            half occX = tex2D(_XOcclusionMap, uvX).g;
            half occY = tex2D(_YOcclusionMap, uvY).g;
            half occZ = tex2D(_ZOcclusionMap, uvZ).g;
            half occ =  triblend.x * LerpOneTo(occX, _XOcclusion) + 
                        triblend.y * LerpOneTo(occY, _YOcclusion) + 
                        triblend.z * LerpOneTo(occZ, _ZOcclusion);

            // metallic textures
            float4 metX = tex2D(_XMetallicMap, uvX);
            float4 metY = tex2D(_YMetallicMap, uvY);
            float4 metZ = tex2D(_ZMetallicMap, uvZ);
            // metallicity from red channel
            half met = (metX.r * triblend.x * _XMetallic +  
                        metY.r * triblend.y * _YMetallic +
                        metZ.r * triblend.z * _ZMetallic);
            // smoothness from alpha channel
            half smo = (metX.a * triblend.x * _XSmoothness + 
                        metY.a * triblend.y * _YSmoothness + 
                        metZ.a * triblend.z * _ZSmoothness);

            // tangent space normal maps
            half3 tnormalX = UnpackNormal(tex2D(_XNormalMap, uvX));
            half3 tnormalY = UnpackNormal(tex2D(_YNormalMap, uvY));
            half3 tnormalZ = UnpackNormal(tex2D(_ZNormalMap, uvZ));

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

            // swizzle tangent normals to match world normal and blend together
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