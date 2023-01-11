using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using MrPink.Units;
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
        foreach (var hc in UnitsManager.Instance.HcInGame)
        {
            if (hc == null || hc.IsDead)
                continue;
            if (hc.team == Team.PlayerParty)
                continue;
            if (Vector3.Distance(pos, hc.transform.position) <= distance)
            {
                hc.AiMovement?.MoveToPositionOrder(pos);
            }
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
