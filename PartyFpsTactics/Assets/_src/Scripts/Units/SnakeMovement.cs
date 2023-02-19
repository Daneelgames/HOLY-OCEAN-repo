using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityAnimation;
using Fraktalia.VoxelGen.Visualisation;
using MrPink;
using MrPink.PlayerSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class SnakeMovement : MonoBehaviour
{
    [SerializeField] [ReadOnly]private float moveSpeed = 30;
    [SerializeField] private float moveSpeedMin = 50; 
    [SerializeField] private float moveSpeedMax = 150; 
    [SerializeField] private float gravityDrag = 0.1f;
    [SerializeField] [ReadOnly] private float gravityForce = 30;
    [SerializeField] private Vector2 gravityForceMinMax = new Vector2(-500,500);
    [SerializeField] private Vector2 gravityChangeTimeMinMax = new Vector2(1,10);
    [SerializeField] [ReadOnly] private float rotationSpeed = 10f;
    [SerializeField] private float rotationSpeedMin = 1f;
    [SerializeField] private float rotationSpeedMax = 10f;
    [SerializeField] [ReadOnly]private float changeRotationSpeedCooldown = 3;
    [SerializeField] [ReadOnly] private Transform target;
    public Transform Target => target;
    [SerializeField] private bool active = false;
    [SerializeField] private Rigidbody rb;
    
    private Vector3 targetDir;
    float t = 0;

    public void OnEnable()
    {
        active = true;   
        if (getTargetCoroutine != null)
            StopCoroutine(getTargetCoroutine);
        getTargetCoroutine = StartCoroutine(GetTarget());
        StartCoroutine(RandomizeGravity());
    }

    public void OnRelease()
    {
        active = false;
    }

    private Coroutine getTargetCoroutine;

    IEnumerator GetTarget()
    {
        Game.PlayerDistance closestPlayer;
        
        while (active)
        {
            if (Game._instance== null || Game.LocalPlayer == null)
            {
                yield return null;
                continue;
            }

            closestPlayer = Game._instance.DistanceToClosestPlayer(transform.position);

            t += 0.1f;
            if (t >= changeRotationSpeedCooldown)
            {
                t = 0;
                rotationSpeed = Random.Range(rotationSpeedMin, rotationSpeedMax);
                moveSpeed = Random.Range(moveSpeedMin, moveSpeedMax);
            }
            target = closestPlayer.Player.transform;
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator RandomizeGravity()
    {
        while (true)
        {
            yield return null;
            yield return new WaitForSeconds(Random.Range(gravityChangeTimeMinMax.x, gravityChangeTimeMinMax.y));
            gravityForce = Random.Range(gravityForceMinMax.x, gravityForceMinMax.y);
        }
    }
    
    void Update()
    {
        if (!active || target == null)
            return;
        
        //transform.Translate(transform.forward * moveSpeed * Time.smoothDeltaTime, Space.World);

        targetDir = (target.position - transform.position).normalized;
        
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetDir), rotationSpeed * Time.deltaTime);
        gravityForce = Mathf.Lerp(gravityForce, 0, gravityDrag * Time.deltaTime);
        //transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime * Input.GetAxis("Horizontal"));
    }

    private void FixedUpdate()
    {
        rb.AddForce(transform.forward * moveSpeed + Vector3.down * gravityForce, ForceMode.Acceleration);
    }

    public void SetMovementState(SnakeMovementBrain.MovementState movementState)
    {
        gravityForceMinMax = movementState.gravityForceMinMax;
        gravityChangeTimeMinMax = movementState.gravityChangeTimeMinMax;
        moveSpeedMin = movementState.moveSpeedMin;
        moveSpeedMax = movementState.moveSpeedMax;
        rotationSpeedMin = movementState.rotationSpeedMin;
        rotationSpeedMax = movementState.rotationSpeedMax;
        changeRotationSpeedCooldown = movementState.changeRotationSpeedCooldown;
        
    }
}
