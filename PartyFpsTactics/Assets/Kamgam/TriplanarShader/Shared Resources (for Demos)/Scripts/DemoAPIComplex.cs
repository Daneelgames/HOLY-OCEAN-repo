using UnityEngine;

namespace Kamgam.TriplanarShader
{
    public class DemoAPIComplex : MonoBehaviour
    {
        public TriplanarMaterialComplex TriMat;

        public Texture[] Textures;
        public Texture[] NormalMaps;
        public Texture[] MetallicMaps;
        public Texture[] OcclusionMaps;

        public void ResetProperties()
        {
            TriMat.ResetProperties();
        }

        public void RandomColor()
        {
            TriMat.SetColor(new Color(Random.value, Random.value, Random.value, 1f));
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

        public void Metallic(float value)
        {
            TriMat.SetMetallic(value);
        }

        public void Smoothness(float value)
        {
            TriMat.SetSmoothness(value);
        }

        public void Occlusion(float value)
        {
            TriMat.SetOcclusion(value);
        }

        public void Offset(float value)
        {
            TriMat.SetOffset(new Vector2(value, value));
        }

        public void Tiling(float value)
        {
            TriMat.SetTiling(new Vector2(value, value));
        }

        int textureIndex = 0;

        public void RandomTexture()
        {
            textureIndex++;
            int index = textureIndex % Textures.Length;
            TriMat.SetMainTexture(Textures[index]);
            TriMat.SetNormalMap(NormalMaps[index]);
            TriMat.SetMetallicMap(MetallicMaps[index]);
            TriMat.SetOcclusionMap(OcclusionMaps[index]);
        }
    }
}