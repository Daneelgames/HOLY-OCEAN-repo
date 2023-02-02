using System;
using System.Collections;
using System.Collections.Generic;
using MrPink;
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
            
            if (GameManager.Instance == null || Shop.Instance.IsActive || PlayerInventoryUI.Instance.IsActive)
            {
                activeWhenTimeScaleLessTanOne.SetActive(false);
                continue;
            }
            activeWhenTimeScaleLessTanOne.SetActive(GameManager.Instance.CurrentTimeScale < 1);
        }
    }
}
