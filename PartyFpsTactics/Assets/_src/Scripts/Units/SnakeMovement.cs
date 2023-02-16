using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityAnimation;
using Fraktalia.VoxelGen.Visualisation;
using MrPink;
using MrPink.PlayerSystem;
using UnityEngine;
using Random = UnityEngine.Random;

public class SnakeMovement : MonoBehaviour
{
    private float moveSpeed = 30;
    [SerializeField] private float gravityForce = 30;
    [SerializeField] private float moveSpeedMin = 50; 
    [SerializeField] private float moveSpeedMax = 150; 
    private float rotationSpeed = 10f;
    [SerializeField] private float rotationSpeedMin = 1f;
    [SerializeField] private float rotationSpeedMax = 10f;
    [SerializeField] private Transform target;
    [SerializeField] private float distanceToPlayerToAttack = 300;
    [SerializeField] private bool active = false;
    [SerializeField] private float changeRotationSpeedDelay = 1;
    [SerializeField] private Rigidbody rb;
    
    private Vector3 targetDir;
    float t = 0;

    public void OnEnable()
    {
        active = true;   
        if (getTargetCoroutine != null)
            StopCoroutine(getTargetCoroutine);
        getTargetCoroutine = StartCoroutine(GetTarget());
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
            if (t >= changeRotationSpeedDelay)
            {
                t = 0;
                rotationSpeed = Random.Range(rotationSpeedMin, rotationSpeedMax);
                moveSpeed = Random.Range(moveSpeedMin, moveSpeedMax);
            }
            target = closestPlayer.Player.transform;
            yield return new WaitForSeconds(0.1f);
        }
    }

    void Update()
    {
        if (!active || target == null)
            return;
        
        //transform.Translate(transform.forward * moveSpeed * Time.smoothDeltaTime, Space.World);

        targetDir = (target.position - transform.position).normalized;
        
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetDir), rotationSpeed * Time.deltaTime);
        //transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime * Input.GetAxis("Horizontal"));
    }

    private void FixedUpdate()
    {
        rb.AddForce(transform.forward * moveSpeed + Vector3.down * gravityForce, ForceMode.Acceleration);
    }
}
