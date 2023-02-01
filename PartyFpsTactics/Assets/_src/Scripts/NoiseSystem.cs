using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using MrPink.Units;
using Unity.VisualScripting;
using UnityEngine;

public class NoiseSystem : MonoBehaviour
{
    public static NoiseSystem Instance;
    [SerializeField] private float defaultNoiseDistance = 10;
    [SerializeField] private float stepsNoiseDistance = 5;

    private void Awake()
    {
        Instance = this;
    }

    public void MakeNoise(Vector3 pos, float distance)
    {
        StartCoroutine(MakeNoiseOverTime(pos, distance));
    }

    IEnumerator MakeNoiseOverTime(Vector3 pos, float distance)
    {
        
        if (UnitsManager.Instance.HcInGame.Count < 1)
            yield break;
        
        for (var index = UnitsManager.Instance.HcInGame.Count - 1; index >= 0; index--)
        {
            if (UnitsManager.Instance.HcInGame.Count <= index)
                continue;
            var hc = UnitsManager.Instance.HcInGame[index];
            if (hc == null || hc.IsDead)
                continue;
            if (hc.team == Team.PlayerParty)
                continue;
            if (hc.AiMovement == null)
                continue;
            if (hc.AiMovement.enemyToLookAt != null)
                continue;

            if (!(Vector3.Distance(pos, hc.transform.position) <= distance)) continue;

            hc.AiMovement.MoveToPositionOrder(pos);
            yield return null;
        }
    }

    public void DefaultNoise(Vector3 pos)
    {
        MakeNoise(pos, defaultNoiseDistance);
    }
    public void StepsNoise(Vector3 pos)
    {
        MakeNoise(pos, stepsNoiseDistance);
    }
}
