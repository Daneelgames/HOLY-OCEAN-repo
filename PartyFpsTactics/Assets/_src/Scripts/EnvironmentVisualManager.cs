using System;
using System.Collections;
using System.Collections.Generic;
using Crest;
using MrPink;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

public class EnvironmentVisualManager : MonoBehaviour
{
    [SerializeField] private OceanRenderer oceanRenderer;
    [SerializeField] private float fogDensity  = 0.01f;
    [SerializeField] private Color skyColor;
    [SerializeField] private Color oceanFirstColor;
    [SerializeField] private Color oceanSecondColor;

    private void Start()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        RenderSettings.fogColor = skyColor;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.skybox.SetColor("_Color1", skyColor);
        RenderSettings.skybox.SetColor("_Color2", skyColor);
        oceanRenderer.OceanMaterial.SetColor("_Diffuse", oceanFirstColor);
        oceanRenderer.OceanMaterial.SetColor("_DiffuseGrazing", oceanSecondColor);
    }

}
