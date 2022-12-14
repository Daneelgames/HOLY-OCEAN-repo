using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateIfTimeScale : MonoBehaviour
{
    [SerializeField] private GameObject activeWhenTimeScaleLessTanOne;

    private void Start()
    {
        StartCoroutine(CheckTimeScale());
    }

    IEnumerator CheckTimeScale()
    {
        while (true)
        {
            yield return null;
            
            if (Time.timeScale < 1)
                activeWhenTimeScaleLessTanOne.SetActive(true);
            else
                activeWhenTimeScaleLessTanOne.SetActive(false);
        }
    }
}
