using System;
using System.Collections;
using System.Collections.Generic;
using MrPink;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private float activationDistance = 5;
    [SerializeField] private GameObject activeFeedback;

    private void OnEnable()
    {
        activeFeedback.SetActive(false);
        if (getPlayerCoroutine != null)
            StopCoroutine(getPlayerCoroutine);
        getPlayerCoroutine = StartCoroutine(GetPlayer());
    }

    private Coroutine getPlayerCoroutine;
    private IEnumerator GetPlayer()
    {
        while (Game._instance == null || Game.LocalPlayer == null)
        {
            yield return null;
        }
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            
            if (activeFeedback.activeInHierarchy) // should be already active
                continue;
            
            if (Vector3.Distance(transform.position, Game.LocalPlayer.transform.position) < activationDistance)
            {
                Game.LocalPlayer.SaveCheckpoint(this);
                activeFeedback.SetActive(true);
            }
        }
    }

    public void DisableCheckpoint()
    {
        activeFeedback.SetActive(false);
    }
}
