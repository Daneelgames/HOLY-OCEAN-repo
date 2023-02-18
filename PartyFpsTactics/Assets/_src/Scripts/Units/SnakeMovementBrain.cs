using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using FishNet.Object;
using MrPink.Health;
using Sirenix.OdinInspector;
using UnityEngine;

public class SnakeMovementBrain : NetworkBehaviour
{
    [SerializeField] private SnakeMovement _snakeMovement;
    [SerializeField] private HealthController _ownHealth;
    [SerializeField] private float updateTime = 0.5f;
    [SerializeField] private float chargeSignalTime = 1f;
    [SerializeField] [ReadOnly] private float prevHealthFill = 1f;
    [SerializeField] private float healthFillStepToCharge = 0.1f;
    
    [SerializeField] private MovementState _chargeState;
    [SerializeField] private MovementState _backOffState;
    [SerializeField] private List<MovementState> _movementStates;
    [SerializeField] private List<DamageUnitsOnCollision> _damageUnitsTriggers = new List<DamageUnitsOnCollision>();

    [Serializable]
    public struct MovementState
    {
        public string stateName;
        [Header("Player distance to head")]
        public float distanceMin;
        public float distanceMax;
        [Header("Move settings")]
        public Vector2 gravityForceMinMax;
        public Vector2 gravityChangeTimeMinMax;
        public float moveSpeedMin;
        public float moveSpeedMax;
        public float rotationSpeedMin;
        public float rotationSpeedMax;
        public float changeRotationSpeedCooldown;
    }

    
    public override void OnStartClient()
    {
        base.OnStartClient();
        
        if (switchStatesCoroutine != null)
            StopCoroutine(switchStatesCoroutine);

        foreach (var trigger in _damageUnitsTriggers)
        {
            trigger.OnPlayerDamaged += DamageUnitsTrigger_OnPlayerDamaged;
        }

        _ownHealth.OnDamagedEvent.AddListener(HealthController_OnDamagedEvent);
        switchStatesCoroutine = StartCoroutine(SwitchStates());
    }


    private Coroutine switchStatesCoroutine;
    IEnumerator SwitchStates()
    {
        float currentDistance;
        List<int> availableMovementStateIndexes = new List<int>();
        while (true)
        {
            yield return new WaitForSeconds(updateTime);
            
            availableMovementStateIndexes.Clear();
            currentDistance = Vector3.Distance(transform.position, _snakeMovement.Target.position);
            for (var index = 0; index < _movementStates.Count; index++)
            {
                var state = _movementStates[index];
                if (currentDistance > state.distanceMin && currentDistance < state.distanceMax)
                {
                    availableMovementStateIndexes.Add(index);
                }
            }

            if (availableMovementStateIndexes.Count > 0)
                _snakeMovement.SetMovementState(_movementStates[availableMovementStateIndexes[0]]);
        }
    }


    private Coroutine signalBeforeChargeCoroutine;
    IEnumerator SignalBeforeCharge()
    {
        // play audio clue
        yield return new WaitForSeconds(chargeSignalTime);
        SetChargeState();
        signalBeforeChargeCoroutine = null;
    }
    void SetChargeState()
    {
        _snakeMovement.SetMovementState(_chargeState);
    }
    
    void HealthController_OnDamagedEvent()
    {
        // set charge pattern
        var currentFill = _ownHealth.GetHealthFill;
        
        if (prevHealthFill - currentFill < healthFillStepToCharge)
            return;
        
        if (signalBeforeChargeCoroutine != null)
            return;
        
        prevHealthFill = currentFill;
        signalBeforeChargeCoroutine = StartCoroutine(SignalBeforeCharge());
    }
    void DamageUnitsTrigger_OnPlayerDamaged()
    {
        // set back off pattern
        _snakeMovement.SetMovementState(_backOffState);
    }
}