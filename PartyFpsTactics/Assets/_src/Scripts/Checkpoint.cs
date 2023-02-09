using System;
using System.Collections;
using System.Collections.Generic;
using MrPink;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private float activationDistance = 5;
    [SerializeField] private GameObject activeFeedback;

    private void Start()
    {
        activeFeedback.SetActive(false);
        StartCoroutine(GetPlayer());
    }

    private IEnumerator GetPlayer()
    {
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
