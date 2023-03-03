using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessingWrapper : MonoBehaviour
{
    public static PostProcessingWrapper Instance;

    private UnityEngine.Rendering.Universal.Vignette vignette;
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Volume volume = gameObject.GetComponent<Volume>();
        volume.profile.TryGet(out vignette);
    }

    public void SetVignette(float fill)
    {
        // min 0, max 0.5
        fill = 1 - fill;
        vignette.intensity.value = Mathf.Lerp(0, 0.5f, fill);
    }
}
