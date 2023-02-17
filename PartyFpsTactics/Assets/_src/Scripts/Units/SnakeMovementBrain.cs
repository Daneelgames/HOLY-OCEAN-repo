using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using FishNet.Object;
using UnityEngine;

public class SnakeMovementBrain : NetworkBehaviour
{
    [SerializeField] private List<MovementState> _movementStates;
    [SerializeField] private SnakeMovement _snakeMovement;
    [SerializeField] private float updateTime = 0.5f;

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
}