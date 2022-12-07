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
    private void Start()
    {
        StartCoroutine(CheckDistances());
    }

    IEnumerator CheckDistances()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.33f);
            
            bool completed = false;

            float distanceToPlayer = Vector3.Distance(exitPoint.position, Game.Player.Position);
            yield return null;
            float distanceToGoal = Vector3.Distance(exitPoint.position, LevelGoal.Instance.transform.position);
            
            if (distanceToPlayer < maxPlayerDistanceToExit && distanceToGoal < maxGoalDistanceToExit)
            {
                ProgressionManager.Instance.SetCurrentLevel(ProgressionManager.Instance.currentLevelIndex + 1);
                GameManager.Instance.StartProcScene();
                yield break;
            }
        }
    }
}
