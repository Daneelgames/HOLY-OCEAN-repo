Shader "Fraktalia/Core/MultiTexture/TextureAtlas_UVBlend_Triplanar"
{
    Properties
    {
        _TransformTex("Texture Transform", Vector) = (0, 0, 1, 1)

        [Header(Main Maps)]    
        [SingleLine(_Color)] _DiffuseMap ("Diffuse (Albedo) Maps", 2D) = "white" {}
        [HideInInspector] _Color("Color", Color) = (1,1,1,1)
                
        [SingleLine(_Metallic)] _MetallicGlossMap("Metallic Maps", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        
        [HideInInspector] _Metallic("Metallic", Range(0,1)) = 0.0

         
        [SingleLine(_BumpScale)] _BumpMap("Normal Maps", 2D) = "bump" {}
        [HideInInspector] _BumpScale("Normal Scale", Float) = 1.0
            

        [SingleLine(_Parallax)]
        _ParallaxMap("Height Maps", 2D) = "grey" {}
        [HideInInspector]_Parallax("Height Scale", Range(0.005, 0.08)) = 0.02
        
        [SingleLine(_OcclusionStrength)]
        _OcclusionMap("Occlusion Maps", 2D) = "white" {}
        [HideInInspector]_OcclusionStrength("Occlusion Strength", Range(0.0, 1.0)) = 1.0
           
        [SingleLine(_EmissionColor)]
        _EmissionMap("Emission Maps", 2D) = "black" {}       
        [HideInInspector]_EmissionColor("Emission Color", Color) = (0,0,0)

        _TextureAtlas_Width("Texture Atlas Width", float) = 2
        _TextureAtlas_Height("Texture Atlas Width", float) = 2

        [HideInInspector]_IndexTex("Albedo (RGB)", 2D) = "black" {}
        [HideInInspector]_IndexTex2("Albedo (RGB)", 2D) = "black" {}

        [Header(Texture Blending)]
        _BaseSlice("Base Slice", float) = 1
        _BaseSupression("Base Supression", float) = 1
        _UV3Power("UV3 Power", float) = 1
        _UV3Slice("UV3 Slice", float) = 1
        _UV4Power("UV4 Power", float) = 1
        _UV4Slice("UV4 Slice", float) = 1
        _UV5Power("UV5 Power", float) = 1
        _UV5Slice("UV5 Slice", float) = 1
        _UV6Power("UV6 Power", float) = 1
        _UV6Slice("UV6 Slice", float) = 1

        


        [Header(Triplanar Settings)]
        _MapScale("Triplanar Scale", Float) = 1
               
        [MultiCompileOption(PULSATING)]
        PULSATING("Pulsating", float) = 0
        [KeywordDependent(PULSATING)] _PulseFrequency("Pulse Frequency", float) = 1
        [KeywordDependent(PULSATING)] _PulseAmplitude("Pulse Amplitude", float) = 1

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        //Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 200

        CGPROGRAM

        
        //#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON

        //#pragma shader_feature _EMISSION
        //#pragma shader_feature _METALLICGLOSSMAP
        //#pragma shader_feature ___ _DETAIL_MULX2
        //#pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
        //#pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
        //#pragma shader_feature _ _GLOSSYREFLECTIONS_OFF
        //#pragma shader_feature _PARALLAXMAP
   
        #pragma surface surf Standard fullforwardshadows vertex:disp

        #pragma require 2darray        
        #pragma target 2.5

        sampler2D _DiffuseMap;
        sampler2D _BumpMap;
        sampler2D _ParallaxMap;
        sampler2D _MetallicGlossMap;
        sampler2D _OcclusionMap;
        sampler2D _EmissionMap;

        
        float4 _TransformTex;

        sampler2D _IndexTex;
        sampler2D _IndexTex2;

        float _BaseSlice;
        float _BaseSupression;
        float _UV3Power;
        float _UV3Slice;

        float _UV4Power;
        float _UV4Slice;

        float _UV5Power;
        float _UV5Slice;

        float _UV6Power;
        float _UV6Slice;

        float _Parallax;

        float _MapScale;

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
            float2 uv_MainTex;
            float2 uv3_IndexTex;
            float2 uv4_IndexTex2;
            float3 viewDir;        
            INTERNAL_DATA
        };

        half _Glossiness;
        half _Metallic;

        fixed4 _Color;
        float _BumpScale;
        float4 _EmissionColor;
        float _OcclusionStrength;
        float _Tess;
        float _TesselationPower;
        float _Tess_MinEdgeDistance;
        float _TesselationOffset;
        float minDist;
        float maxDist;

        float _TextureAtlas_Width;
        float _TextureAtlas_Height;


        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        float2 convertToSliceUV(float3 uv)
        {
            float2 texUV = abs(uv.xy);
            

            float2 index = uv.z;

            float texturesizeX = 1 / _TextureAtlas_Width;
            float texturesizeY = 1 / _TextureAtlas_Height;

            float2 uv1slice = float2(texUV.x % texturesizeX, texUV.y % texturesizeY);
           
            uv1slice.x += texturesizeX * (int)(index % _TextureAtlas_Width);
            uv1slice.y += texturesizeY * ((int)(index / _TextureAtlas_Width) % _TextureAtlas_Height);
            return uv1slice;
        }

        float4 GET_FROMTEXTUREARRAY(sampler2D tex,float3 uv1, float3 uv2,float blendfactor)
        { 
            float2 uv1slice = convertToSliceUV(uv1);
            float2 uv2slice = convertToSliceUV(uv2);
   
            float4 o1 = tex2D(tex, uv1slice) * (1 - blendfactor);
            float4 o2 = tex2D(tex, uv2slice) * (blendfactor);
            return o1 + o2;
        }

       
        float4 GET_FROMTEXTUREARRAY_TRIPLANAR(sampler2D tex, float3 uvxy1, float3 uvxz1, float3 uvyz1, float3 bf)
        {
            float4 o1 = tex2D(tex, convertToSliceUV(uvxy1));
            float4 result1 = (o1)  * bf.z;
            o1 = tex2D(tex, convertToSliceUV(uvxz1));
            float4 result2 = (o1)  * bf.y;
            o1 = tex2D(tex, convertToSliceUV(uvyz1));
            float4 result3 = (o1)  * bf.x;
            return result1 + result2 + result3;
        }

        //On error, remove white spaces and add them again.
        void GET_FROMTEXTUREARRAY_TRIPLANAR_RESULTSONLY(sampler2D tex, float3 uvxy1, float3 uvxz1, float3 uvyz1, float3 bf, out float4 result1, out float4 result2, out float4 result3)
        {
            float4 o1 = tex2D(tex, convertToSliceUV(uvxy1));
            result3 = (o1)  * bf.z;
            o1 = tex2D(tex, convertToSliceUV(uvxz1));
            result2 = (o1)  * bf.y;
            o1 = tex2D(tex, convertToSliceUV(uvyz1));
            result1 = (o1)  * bf.x;
        }

        void disp(inout appdata_full v)
        {
            
#if !TESSELLATION
            return;
#endif
              
            float3 position = v.vertex.xyz * _MapScale;
            float3 normal = v.normal;

            float3 bf = normalize(abs(normal));
            bf /= dot(bf, (float3)1);

            float2 trix = position.zy;
            float2 triy = position.xz;
            float2 triz = position.xy;

            trix += _TransformTex.xy;
            triy += _TransformTex.xy;
            triz += _TransformTex.xy;

            trix *= _TransformTex.zw;
            triy *= _TransformTex.zw;
            triz *= _TransformTex.zw;

            if (normal.x < 0) {
                trix.x = -trix.x;
            }
            if (normal.y < 0) {
                triy.x = -triy.x;
            }
            if (normal.z >= 0) {
                triz.x = -triz.x;
            }

            int textureindex = _BaseSlice;
            float3 xy1 = fixed3(triz, textureindex);          
            float3 xz1 = fixed3(triy, textureindex); 
            float3 yz1 = fixed3(trix, textureindex);
           
           
            float4 o2 = tex2D(_ParallaxMap, convertToSliceUV(xy1));
            float4 result1 = (o2)*bf.z;
            o2 = tex2D(_ParallaxMap, convertToSliceUV(xz1));
            float4 result2 = (o2)*bf.y; \
            o2 = tex2D(_ParallaxMap, convertToSliceUV(yz1));
            float4 result3 = (o2)*bf.x; \
            float4 result = result1 + result2 + result3;


            float  displacement = result.r;

            displacement *= _TesselationPower;
            displacement += _TesselationOffset;
    
            v.vertex.xyz += v.normal * displacement;        
        }

        struct Result {
            float4  Diffuse;
            half3   Normal;
            float4  Metallic;
            float   Smoothness;
            float4  Occlusion;
            float4  Emission;
        };

        Result CalculateLayer(Input IN, float2 trix, float2 triy, float2 triz, float3 bf, float power, float textureindex)
        {
            float4 result;
            Result output;          
            float3 xy1 = fixed3(triz, textureindex);
            float3 xz1 = fixed3(triy, textureindex);
            float3 yz1 = fixed3(trix, textureindex);

            float4 parallaxX, parallaxY, parallaxZ;
            GET_FROMTEXTUREARRAY_TRIPLANAR_RESULTSONLY(_ParallaxMap, xy1, xz1, yz1, bf, parallaxX, parallaxY, parallaxZ);
            xy1.xy += ParallaxOffset(parallaxZ.r * power, _Parallax, IN.viewDir);
            xz1.xy += ParallaxOffset(parallaxY.r * power, _Parallax, IN.viewDir);
            yz1.xy += ParallaxOffset(parallaxX.r * power, _Parallax, IN.viewDir);


            float4 resultfinal = GET_FROMTEXTUREARRAY_TRIPLANAR(_DiffuseMap, xy1, xz1, yz1, bf);                   
            output.Diffuse = GET_FROMTEXTUREARRAY_TRIPLANAR(_DiffuseMap, xy1, xz1, yz1, bf) * power;

            float4 normalX, normalY, normalZ;
            GET_FROMTEXTUREARRAY_TRIPLANAR_RESULTSONLY(_BumpMap, xy1, xz1, yz1, bf, normalX, normalY, normalZ);
            float4 normalvalue = (normalX + normalY + normalZ) * clamp(power,0,1);
            output.Normal = UnpackScaleNormal(normalvalue , _BumpScale);

            result = GET_FROMTEXTUREARRAY_TRIPLANAR(_MetallicGlossMap, xy1, xz1, yz1, bf);
            output.Metallic = result * _Metallic * power;
            output.Smoothness = result.a * _Glossiness * power;

            result = GET_FROMTEXTUREARRAY_TRIPLANAR(_OcclusionMap, xy1, xz1, yz1, bf);
            output.Occlusion = result * _OcclusionStrength * power;

            result = GET_FROMTEXTUREARRAY_TRIPLANAR(_EmissionMap, xy1, xz1, yz1, bf);
            output.Emission = result * _EmissionColor * power;

            return output;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float4 result;
                             
            float3 position = mul(unity_WorldToObject, float4(IN.worldPos, 1)) * _MapScale;
            float3 normal = WorldNormalVector(IN, o.Normal);

            float3 bf = normalize(abs(normal));
            bf /= dot(bf, (float3)1);

            float2 trix = position.zy;
            float2 triy = position.xz;
            float2 triz = position.xy;

            trix += _TransformTex.xy;
            triy += _TransformTex.xy;
            triz += _TransformTex.xy;

            trix *= _TransformTex.zw;
            triy *= _TransformTex.zw;
            triz *= _TransformTex.zw;

            if (normal.x < 0) {
                trix.x = -trix.x;
            }
            if (normal.y < 0) {
                triy.x = -triy.x;
            }
            if (normal.z >= 0) {
                triz.x = -triz.x;
            }

            Result base = CalculateLayer(IN, trix, triy, triz, bf, 1, _BaseSlice);
            Result layeruv3 = CalculateLayer(IN, trix, triy, triz, bf, 1, _UV3Slice);
            Result layeruv4 = CalculateLayer(IN, trix, triy, triz, bf, 1, _UV4Slice);
            Result layeruv5 = CalculateLayer(IN, trix, triy, triz, bf, 1, _UV5Slice);
            Result layeruv6 = CalculateLayer(IN, trix, triy, triz, bf, 1, _UV6Slice);

            float sumPower = 1 + (IN.uv3_IndexTex.x * _UV3Power + IN.uv3_IndexTex.y * _UV4Power + IN.uv4_IndexTex2.x * _UV5Power + IN.uv4_IndexTex2.y * _UV6Power) * _BaseSupression;
            float basefactor = 1 / sumPower;
            float layer1factor = IN.uv3_IndexTex.x * _UV3Power * _BaseSupression / sumPower;
            float layer2factor = IN.uv3_IndexTex.y * _UV4Power * _BaseSupression / sumPower;
            float layer3factor = IN.uv4_IndexTex2.x * _UV5Power * _BaseSupression / sumPower;
            float layer4factor = IN.uv4_IndexTex2.y * _UV6Power * _BaseSupression / sumPower;

            float biggest = max(basefactor, max(layer1factor, max(layer2factor, max(layer3factor, layer4factor))));
            layer1factor /= biggest;
            layer2factor /= biggest;
            layer3factor /= biggest;
            layer4factor /= biggest;
            basefactor /= biggest;

            o.Albedo = base.Diffuse.rgb * basefactor + layeruv3.Diffuse.rgb * layer1factor + layeruv4.Diffuse.rgb * layer2factor + layeruv5.Diffuse.rgb * layer3factor + layeruv6.Diffuse.rgb * layer4factor;
            o.Normal = base.Normal * basefactor + layeruv3.Normal * layer1factor + layeruv4.Normal * layer2factor + layeruv5.Normal * layer3factor + layeruv6.Normal * layer4factor;
            o.Metallic = (base.Metallic + layeruv3.Metallic * layer1factor + layeruv4.Metallic * layer2factor + layeruv5.Metallic * layer3factor + layeruv6.Metallic * layer4factor) * _Metallic;
            o.Smoothness = (base.Smoothness * basefactor + layeruv3.Smoothness * layer1factor + layeruv4.Smoothness * layer2factor + layeruv5.Smoothness * layer3factor + layeruv6.Smoothness * layer4factor) * _Glossiness;
            o.Occlusion = base.Occlusion * basefactor + layeruv3.Occlusion * layer1factor + layeruv4.Occlusion * layer2factor + layeruv5.Occlusion * layer3factor + layeruv6.Occlusion * layer4factor;
            o.Emission = base.Emission * basefactor + layeruv3.Emission * layer1factor + layeruv4.Emission * layer2factor + layeruv5.Emission * layer3factor + layeruv6.Emission * layer4factor;
            o.Alpha = base.Diffuse.a * basefactor + layeruv3.Diffuse.a * layer1factor + layeruv4.Diffuse.a * layer2factor + layeruv5.Diffuse.a * layer3factor + layeruv6.Diffuse.a * layer4factor;

        }
        ENDCG
    }
    FallBack "Diffuse"
    CustomEditor "TextureArrayGUI"
}
