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
        clr = new Color(clr.r + Random.Range(-100, 100), clr.g + Random.Range(-100, 100), clr.b + Random.Range(-100, 100), clr.a);
        var fogColor = clr;
        var camBackColor = clr;
        var fogIntensity = ProgressionManager.Instance.CurrentLevel.fogIntensity;
        fogIntensity *= Random.Range(0.2f, 2f);

        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogIntensity;

        Game.Player._mainCamera.backgroundColor = camBackColor;
    }
}
