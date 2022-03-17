using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotSaver : MonoBehaviour
{
    public static ScreenshotSaver Instance;
    public Texture2D lastScreenshot;
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void SaveScreenshot()
    {
        lastScreenshot = ScreenCapture.CaptureScreenshotAsTexture();
    }
}