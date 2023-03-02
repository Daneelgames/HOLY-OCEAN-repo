using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessingWrapper : MonoBehaviour
{
    public static PostProcessingWrapper Instance;

    private Vignette vignette;
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        PostProcessVolume volume = GetComponent<PostProcessVolume>();
        volume.profile.TryGetSettings(out vignette);
    }

    public void SetVignette(float fill)
    {
        // min 0, max 0.5
        vignette.intensity.value = Mathf.Lerp(0, 0.5f, fill);
    }
}
