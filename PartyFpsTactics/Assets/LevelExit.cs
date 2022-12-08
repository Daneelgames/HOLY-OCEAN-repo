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
    [SerializeField] private float maxGoalDistanceToExit = 2;
    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(CheckDistances());
    }

    IEnumerator CheckDistances()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.33f);
            
            if (LevelGoal.Instance == null)
                continue;
            
            float distanceToPlayer = Vector3.Distance(exitPoint.position, Game.Player.Position);
            yield return null;
            float distanceToGoal = Vector3.Distance(exitPoint.position, LevelGoal.Instance.transform.position);
            
            Debug.Log("LEVEL EXIT CHECK DISTANCE 0");
            if (distanceToPlayer < maxPlayerDistanceToExit && distanceToGoal < maxGoalDistanceToExit)
            {
                Debug.Log("LEVEL EXIT CHECK DISTANCE 1");
                ProgressionManager.Instance.SetCurrentLevel(ProgressionManager.Instance.currentLevelIndex + 1);
                GameManager.Instance.StartProcScene();
                yield break;
            }
        }
    }
}
