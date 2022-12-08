using System;
using System.Collections;
using System.Collections.Generic;
using MrPink;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class EnvironmentVisualManager : MonoBehaviour
{
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
        
        var fogColor = ProgressionManager.Instance.CurrentLevel.fogColor;
        var camBackColor = ProgressionManager.Instance.CurrentLevel.fogColor;
        var fogIntensity = ProgressionManager.Instance.CurrentLevel.fogIntensity;

        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogIntensity;

        Game.Player._mainCamera.backgroundColor = camBackColor;
    }
}
