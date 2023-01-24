using UnityEngine;

namespace Kamgam.TriplanarShader
{
    public class DemoAPISimpleXYZ : MonoBehaviour
    {
        public TriplanarMaterialSimpleXYZ TriMat;

        public Texture[] Textures;

        public void ResetProperties()
        {
            TriMat.ResetProperties();
        }

        public void RandomColor()
        {
            TriMat.SetColor(new Color(Random.value, Random.value, Random.value, 1f));
        }

        public void RandomXColor()
        {
            TriMat.SetXColor(new Color(Random.value, Random.value, Random.value, 1f));
        }

        public void RandomYColor()
        {
            TriMat.SetYColor(new Color(Random.value, Random.value, Random.value, 1f));
        }

        public void RandomZColor()
        {
            TriMat.SetZColor(new Color(Random.value, Random.value, Random.value, 1f));
        }

        public void RandomEmissionColor()
        {
            TriMat.SetEmission(new Color(Random.value, Random.value, Random.value, 1f));
        }

        public void EmissionIntensity(float value)
        {
            TriMat.SetEmissionIntensity(value);
        }

        public void Blend(float value)
        {
            TriMat.SetBlend(value);
        }

        public void TopSkew(float value)
        {
            TriMat.SetTopSkew(value);
        }

        public void Metallic(float value)
        {
            TriMat.SetXMetallic(value);
            TriMat.SetYMetallic(value);
            TriMat.SetZMetallic(value);
        }

        public void Smoothness(float value)
        {
            TriMat.SetXSmoothness(value);
            TriMat.SetYSmoothness(value);
            TriMat.SetZSmoothness(value);
        }

        public void Offset(float value)
        {
            TriMat.SetXOffset(new Vector2(value, value));
            TriMat.SetYOffset(new Vector2(value, value));
            TriMat.SetZOffset(new Vector2(value, value));
        }

        public void Tiling(float value)
        {
            TriMat.SetXTiling(new Vector2(value, value));
            TriMat.SetYTiling(new Vector2(value, value));
            TriMat.SetZTiling(new Vector2(value, value));
        }

        int textureIndex = 0;

        public void RandomTexture()
        {
            textureIndex++;
            int index = textureIndex % Textures.Length;

            TriMat.SetXTexture(Textures[index]);

            index = (textureIndex + 1) % Textures.Length;

            TriMat.SetYTexture(Textures[index]);

            index = (textureIndex + 1) % Textures.Length;

            TriMat.SetZTexture(Textures[index]);
        }
    }
}