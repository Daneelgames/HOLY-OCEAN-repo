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
        while (ProgressionManager.Instance == null ||Game._instance == false || Game.Player == null)
        {
            yield return null;
        }

        Color clr = ProgressionManager.Instance.CurrentLevel.fogColor;
        
        var fogColor = clr;
        var camBackColor = clr;
        var fogIntensity = ProgressionManager.Instance.CurrentLevel.fogIntensity;
        
        if (randomizeFog)
        {
            fogIntensity *= Random.Range(0.2f, 2f);
            if (randomColorsList.Count > 1)
                fogColor = randomColorsList[Random.Range(0, randomColorsList.Count)];
            camBackColor = fogColor;
        }

        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogIntensity;

        Game.Player._mainCamera.backgroundColor = camBackColor;
    }
}
