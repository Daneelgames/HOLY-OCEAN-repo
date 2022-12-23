using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using MrPink;
using MrPink.PlayerSystem;
using Unity.VisualScripting;
using UnityEngine;

public class LevelExit : NetworkBehaviour
{
    [SerializeField] private Transform exitPoint;
    [SerializeField] private float maxPlayerDistanceToExit = 2;
    [Header("IF GOAL DISTANCE < 0 - IT WILL NOT CHECK FOR GOAL")]
    [SerializeField] private float maxGoalDistanceToExit = 2;
    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(CheckDistances());
    }
    

    IEnumerator CheckDistances()
    {
        while (Game._instance == null || Game.LocalPlayer == null)
        {
            yield return null;
        }
        
        if (IsServer == false)
        {
            Debug.Log("LEVEL EXIT: dont check distance on client");
            yield break;
        }
        
        bool goalInRange = false;
        bool playerInRange = false;

        while (GameManager.Instance == null)
        {
            yield return null;
        }
        if (GameManager.Instance.GetLevelType == GameManager.LevelType.Building)
        {
            while (LevelGoal.Instance == null)
            {
                yield return null;
            }
        }
        
        while (true)
        {
            yield return new WaitForSeconds(0.33f);
            
            playerInRange = Vector3.Distance(exitPoint.position, Game.LocalPlayer.Position) < maxPlayerDistanceToExit;
            yield return null;
            if (maxGoalDistanceToExit > 0)
                goalInRange = Vector3.Distance(exitPoint.position, LevelGoal.Instance.transform.position) < maxGoalDistanceToExit;
            else
                goalInRange = true;
            
            if (playerInRange && goalInRange)
            {
                GameManager.Instance.LevelCompleted();
                
                yield break;
            }
        }
    }
}
