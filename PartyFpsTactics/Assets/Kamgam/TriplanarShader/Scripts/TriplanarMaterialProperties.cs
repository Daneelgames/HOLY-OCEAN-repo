using UnityEngine;

namespace Kamgam.TriplanarShader
{
    // A list of all used shader property names, limits and default values.

    public static class TriplanarMaterialProperties
    {
        // All
        public const string Color = "_Color";
        public static Color ColorDefault = UnityEngine.Color.white;

        public const string Blend = "_Blend";
        public static float BlendDefault = 14f;

        public const float BlendMin = 0f;
        public const float BlendMax = 40f;

        public static float NormalizeBlend(float blend)
        {
            return (blend - BlendMin) / (BlendMax - BlendMin);
        }

        public static float DenormalizeBlend(float blend)
        {
            return BlendMin + blend * (BlendMax - BlendMin);
        }

        public const string Emission = "_Emission";
        public static Color EmissionDefault = UnityEngine.Color.black;

        public const string EmissionIntensity = "_EmissionIntensity";
        public static float EmissionIntensityDefault = 1f;

        // Simple and others
        public const string MainTexture = "_MainTex";
        public static Texture MainTextureDefault = null;

        public const string MainTilingAndOffsetBuiltIn = MainTexture + "_ST";
        public static Vector4 MainTilingAndOffsetBuiltInDefault = new Vector4(1, 1, 0, 0);

        public const string MainTiling = "_MainTiling";
        public static Vector4 MainTilingDefault = new Vector4(1, 1, 1, 1);

        public const string MainOffset = "_MainOffset";
        public static Vector4 MainOffsetDefault = new Vector4(0, 0, 0, 0);

        public const string Metallic = "_Metallic";
        public static float MetallicDefault = 0f;

        public const string Smoothness = "_Smoothness";
        public static float SmoothnessDefault = 0f;

        public const string Tiling = "_Tiling";
        public static Vector4 TilingDefault = new Vector4(1, 1, 1, 1);

        public const string Offset = "_Offset";
        public static Vector4 OffsetDefault = new Vector4(0, 0, 0, 0);

        // Complex
        public const string MainMetallicMap = "_MainMetallicMap";
        public static Texture MainMetallicMapDefault = null;

        public const string MainNormalMap = "_MainNormalMap";
        public static Texture MainNormalMapDefault = null;
        
        public const string MainOcclusionMap = "_MainOcclusionMap";
        public static Texture MainOcclusionMapDefault = null;

        public const string Occlusion = "_Occlusion";
        public static float OcclusionDefault = 1f;

        // Main
        public const string MainColor = "_MainColor";
        public static Color MainColorDefault = UnityEngine.Color.white;

        public const string MainSmoothness = "_MainSmoothness";
        public static float MainSmoothnessDefault = 0f;

        public const string MainMetallic = "_MainMetallic";
        public static float MainMetallicDefault = 0f;

        public const string MainOcclusion = "_MainOcclusion";
        public static float MainOcclusionDefault = 0f;

        // Top
        public const string TopSkew = "_TopSkew";
        public static float TopSkewDefault = 0f;

        public const float TopSkewMin = 0f;
        public const float TopSkewMax = 100f;

        public static float NormalizeTopSkew(float topSkew)
        {
            return (topSkew - TopSkewMin) / (TopSkewMax - TopSkewMin);
        }

        public static float DenormalizeTopSkew(float topSkew)
        {
            return TopSkewMin + topSkew * (TopSkewMax - TopSkewMin);
        }

        public const string TopColor = "_TopColor";
        public static Color TopColorDefault = UnityEngine.Color.white;

        public const string TopTexture = "_TopTex";
        public static Texture TopTextureDefault = null;

        public const string TopMetallicMap = "_TopMetallicMap";
        public static Texture TopMetallicMapDefault = null;

        public const string TopMetallic = "_TopMetallic";
        public static float TopMetallicDefault = 1f;

        public const string TopSmoothness = "_TopSmoothness";
        public static float TopSmoothnessDefault = 0f;

        public const string TopNormalMap = "_TopNormalMap";
        public static Texture TopNormalMapDefault = null;

        public const string TopOcclusionMap = "_TopOcclusionMap";
        public static Texture TopOcclusionMapDefault = null;

        public const string TopOcclusion = "_TopOcclusion";
        public static float TopOcclusionDefault = 1f;

        public const string TopTiling = "_TopTiling";
        public static Vector4 TopTilingDefault = new Vector4(1,1,1,1);

        public const string TopOffset = "_TopOffset";
        public static Vector4 TopOffsetDefault = new Vector4(0, 0, 0, 0);

        public const string TopTilingAndOffsetBuiltIn = TopTexture + "_ST";
        public static Vector4 TopTilingAndOffsetBuiltInDefault = new Vector4(1, 1, 0, 0);

        // Bottom
        public const string BottomColor = "_BottomColor";
        public static Color BottomColorDefault = UnityEngine.Color.white;

        public const string BottomTexture = "_BottomTex";
        public static Texture BottomTextureDefault = null;

        public const string BottomMetallicMap = "_BottomMetallicMap";
        public static Texture BottomMetallicMapDefault = null;

        public const string BottomMetallic = "_BottomMetallic";
        public static float BottomMetallicDefault = 1f;

        public const string BottomSmoothness = "_BottomSmoothness";
        public static float BottomSmoothnessDefault = 0f;

        public const string BottomNormalMap = "_BottomNormalMap";
        public static Texture BottomNormalMapDefault = null;

        public const string BottomOcclusionMap = "_BottomOcclusionMap";
        public static Texture BottomOcclusionMapDefault = null;

        public const string BottomOcclusion = "_BottomOcclusion";
        public static float BottomOcclusionDefault = 1f;

        public const string BottomTiling = "_BottomTiling";
        public static Vector4 BottomTilingDefault = new Vector4(1, 1, 1, 1);

        public const string BottomOffset = "_BottomOffset";
        public static Vector4 BottomOffsetDefault = new Vector4(0, 0, 0, 0);

        public const string BottomTilingAndOffsetBuiltIn = BottomTexture + "_ST";
        public static Vector4 BottomTilingAndOffsetBuiltInDefault = new Vector4(1, 1, 0, 0);

        // X
        public const string XColor = "_XColor";
        public static Color XColorDefault = UnityEngine.Color.white;

        public const string XTexture = "_XTex";
        public static Texture XTextureDefault = null;

        public const string XMetallicMap = "_XMetallicMap";
        public static Texture XMetallicMapDefault = null;

        public const string XMetallic = "_XMetallic";
        public static float XMetallicDefault = 1f;

        public const string XSmoothness = "_XSmoothness";
        public static float XSmoothnessDefault = 0f;

        public const string XNormalMap = "_XNormalMap";
        public static Texture XNormalMapDefault = null;

        public const string XOcclusionMap = "_XOcclusionMap";
        public static Texture XOcclusionMapDefault = null;

        public const string XOcclusion = "_XOcclusion";
        public static float XOcclusionDefault = 1f;

        public const string XTiling = "_XTiling";
        public static Vector4 XTilingDefault = new Vector4(1, 1, 1, 1);

        public const string XOffset = "_XOffset";
        public static Vector4 XOffsetDefault = new Vector4(0, 0, 0, 0);

        public const string XTilingAndOffsetBuiltIn = XTexture + "_ST";
        public static Vector4 XTilingAndOffsetBuiltInDefault = new Vector4(1, 1, 0, 0);

        // Y
        public const string YColor = "_YColor";
        public static Color YColorDefault = UnityEngine.Color.white;

        public const string YTexture = "_YTex";
        public static Texture YTextureDefault = null;

        public const string YMetallicMap = "_YMetallicMap";
        public static Texture YMetallicMapDefault = null;

        public const string YMetallic = "_YMetallic";
        public static float YMetallicDefault = 1f;

        public const string YSmoothness = "_YSmoothness";
        public static float YSmoothnessDefault = 0f;

        public const string YNormalMap = "_YNormalMap";
        public static Texture YNormalMapDefault = null;

        public const string YOcclusionMap = "_YOcclusionMap";
        public static Texture YOcclusionMapDefault = null;

        public const string YOcclusion = "_YOcclusion";
        public static float YOcclusionDefault = 1f;

        public const string YTiling = "_YTiling";
        public static Vector4 YTilingDefault = new Vector4(1, 1, 1, 1);

        public const string YOffset = "_YOffset";
        public static Vector4 YOffsetDefault = new Vector4(0, 0, 0, 0);

        public const string YTilingAndOffsetBuiltIn = YTexture + "_ST";
        public static Vector4 YTilingAndOffsetBuiltInDefault = new Vector4(1, 1, 0, 0);

        // Z
        public const string ZColor = "_ZColor";
        public static Color ZColorDefault = UnityEngine.Color.white;

        public const string ZTexture = "_ZTex";
        public static Texture ZTextureDefault = null;

        public const string ZMetallicMap = "_ZMetallicMap";
        public static Texture ZMetallicMapDefault = null;

        public const string ZMetallic = "_ZMetallic";
        public static float ZMetallicDefault = 1f;

        public const string ZSmoothness = "_ZSmoothness";
        public static float ZSmoothnessDefault = 0f;

        public const string ZNormalMap = "_ZNormalMap";
        public static Texture ZNormalMapDefault = null;

        public const string ZOcclusionMap = "_ZOcclusionMap";
        public static Texture ZOcclusionMapDefault = null;

        public const string ZOcclusion = "_ZOcclusion";
        public static float ZOcclusionDefault = 1f;

        public const string ZTiling = "_ZTiling";
        public static Vector4 ZTilingDefault = new Vector4(1, 1, 1, 1);

        public const string ZOffset = "_ZOffset";
        public static Vector4 ZOffsetDefault = new Vector4(0, 0, 0, 0);

        public const string ZTilingAndOffsetBuiltIn = ZTexture + "_ST";
        public static Vector4 ZTilingAndOffsetBuiltInDefault = new Vector4(1, 1, 0, 0);
    }
}
