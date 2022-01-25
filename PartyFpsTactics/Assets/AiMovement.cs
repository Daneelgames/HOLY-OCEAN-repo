using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiMovement : MonoBehaviour
{
    public enum Order
    {
        FollowLeader, MoveToPosition
    }

    public Order currentOrder = Order.FollowLeader;
    public NavMeshAgent agent;
    public float moveSpeed = 2;
    public float runSpeed = 4;
    
    public float stopDistanceFollow = 1.5f;
    public float stopDistanceMove = 0;

    float updateDelayCurrent = 0.1f;
    private Vector3 currentVelocity;

    public HumanVisualController humanVisualController;

    private void Update()
    {
        currentVelocity = agent.velocity;
        humanVisualController.SetMovementVelocity(currentVelocity);
    }

    void StopAllBehaviorCoroutines()
    {
        if (followTargetCoroutine != null)
            StopCoroutine(followTargetCoroutine);
    }
    public void FollowLeaderOrder(Transform target)
    {
        StopAllBehaviorCoroutines();
        currentOrder = Order.FollowLeader;
        followTargetCoroutine = StartCoroutine(FollowTargetCoroutine(target));
    }

    private Coroutine followTargetCoroutine;
    IEnumerator FollowTargetCoroutine(Transform target)
    {
        agent.stoppingDistance = stopDistanceFollow;
        NavMeshPath path = new NavMeshPath();
        while (true)
        {
            NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, path);
            agent.speed = moveSpeed;
            agent.SetPath(path);
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void MoveOrder(Vector3 targetPos)
    {
        StopAllBehaviorCoroutines();
        currentOrder = Order.MoveToPosition;
        
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, targetPos, NavMesh.AllAreas, path);
        
        agent.speed = moveSpeed;
        agent.stoppingDistance = stopDistanceMove;
        agent.SetPath(path);
    }

    public void RunOrder()
    {
        agent.speed = runSpeed;
    }
}
