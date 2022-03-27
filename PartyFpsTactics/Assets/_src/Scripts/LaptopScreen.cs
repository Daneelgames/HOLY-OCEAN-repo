using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaptopScreen : MonoBehaviour
{
    public MeshRenderer mesh;
    void Start()
    {
        mesh.material.mainTexture = ScreenshotSaver.Instance.lastScreenshot;
    }
}