using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using MrPink;
using UnityEngine;

public class AirNeed : NetworkBehaviour
{
    [SerializeField] private float freeAirTimeMax = 10;
    private float freeAirTimeCurrent = 20;
    [SerializeField] private int drainAmount = 10;
    [SerializeField] private float maxPosY = 200;
    public override void OnOwnershipClient(NetworkConnection prevOwner)
    {
        base.OnOwnershipClient(prevOwner);

        if (_needsCoroutine != null)
            StopCoroutine(_needsCoroutine);
        _needsCoroutine = StartCoroutine(Needs());
    }

    private Coroutine _needsCoroutine;

    IEnumerator Needs()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            if (Game.LocalPlayer == null)
                continue;

            if (Game.LocalPlayer.Movement.State.HeadIsUnderwater || transform.position.y > maxPosY)
            {
                freeAirTimeCurrent -= 0.1f;
            }
            else
            {
                freeAirTimeCurrent += 0.75f;
                if (freeAirTimeCurrent > freeAirTimeMax)
                    freeAirTimeCurrent = freeAirTimeMax;
            }
            
            if (freeAirTimeCurrent < 0)
            {
                Game.LocalPlayer.Health.DrainHealth(drainAmount);
                freeAirTimeCurrent = 0;
            }
            
        }
    }
}