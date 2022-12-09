using System;
using System.Collections;
using System.Collections.Generic;
using MrPink;
using MrPink.PlayerSystem;
using Unity.VisualScripting;
using UnityEngine;

public class LevelExit : MonoBehaviour
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
            
            playerInRange = Vector3.Distance(exitPoint.position, Game.Player.Position) < maxPlayerDistanceToExit;
            yield return null;
            if (maxGoalDistanceToExit > 0)
                goalInRange = Vector3.Distance(exitPoint.position, LevelGoal.Instance.transform.position) < maxGoalDistanceToExit;
            else
                goalInRange = true;
            
            if (playerInRange && goalInRange)
            {
                if (LevelGoal.Instance)
                    GameManager.Instance.BuildingLevelCompleted();
                else
                    GameManager.Instance.RoadLevelCompleted();
                
                yield break;
            }
        }
    }
}
