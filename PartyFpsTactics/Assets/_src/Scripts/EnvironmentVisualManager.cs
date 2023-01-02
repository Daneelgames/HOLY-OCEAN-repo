using System;
using System.Collections;
using System.Collections.Generic;
using MrPink;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

public class EnvironmentVisualManager : MonoBehaviour
{
    [SerializeField] private bool randomizeFog = true;
    [SerializeField] private List<Color> randomColorsList = new List<Color>();
    private void OnEnable()
    {
        StartCoroutine(Init());
    }

    IEnumerator Init()
    {
        while (ProgressionManager.Instance == null ||Game._instance == false || Game.LocalPlayer == null)
        {
            yield return null;
        }

        Color clr = ProgressionManager.Instance.CurrentLevel.fogColor;
        
        var fogIntensity = ProgressionManager.Instance.CurrentLevel.fogIntensity;
        
        if (randomizeFog)
        {
            fogIntensity *= Random.Range(0.2f, 1.75f);
            
            if (randomColorsList.Count > 1)
                clr = randomColorsList[Random.Range(0, randomColorsList.Count)];
        }

        RenderSettings.fogColor = clr;
        RenderSettings.fogDensity = fogIntensity;

        Game.LocalPlayer.MainCamera.backgroundColor = clr;
    }
}
