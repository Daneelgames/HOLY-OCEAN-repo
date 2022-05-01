using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseMapDisplay : MonoBehaviour
{
    public Renderer textureRenderer;
    public Renderer poiTextureRenderer;
    public Renderer obstaclesTextureRenderer;
    public Renderer roadsTextureRenderer;

    public void ResetMaps()
    {
        poiTextureRenderer.gameObject.SetActive(false);
        obstaclesTextureRenderer.gameObject.SetActive(false);
        roadsTextureRenderer.gameObject.SetActive(false);
    }
    
    public void DrawTexture(Texture2D texture)
    {
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(-texture.width, 1, -texture.height);
    }
    public void DrawPoi(Texture2D texture)
    {
        poiTextureRenderer.sharedMaterial.mainTexture = texture;
        poiTextureRenderer.transform.localScale = new Vector3(-texture.width, 1, -texture.height);
        poiTextureRenderer.gameObject.SetActive(true);
    }
    public void DrawObstacles(Texture2D texture)
    {
        obstaclesTextureRenderer.sharedMaterial.mainTexture = texture;
        obstaclesTextureRenderer.transform.localScale = new Vector3(-texture.width, 1, -texture.height);
        obstaclesTextureRenderer.gameObject.SetActive(true);
    }
    public void DrawRoads(Texture2D texture)
    {
        roadsTextureRenderer.sharedMaterial.mainTexture = texture;
        roadsTextureRenderer.transform.localScale = new Vector3(-texture.width, 1, -texture.height);
        roadsTextureRenderer.gameObject.SetActive(true);
    }
}
