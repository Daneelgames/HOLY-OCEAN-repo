using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AiMovement : MonoBehaviour
{
    public enum Order
    {
        FollowLeader, MoveToPosition, TakeCover, FireWatch
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
    public UnitVision unitVision;

    private HealthController hc;
    private float takeCoverCooldown = 0;

    private void Awake()
    {
        hc = GetComponent<HealthController>();
    }

    private void Start()
    {
        StartCoroutine(Awareness());
    }

    private void Update()
    {
        currentVelocity = agent.velocity;
        humanVisualController.SetMovementVelocity(currentVelocity);
        if (takeCoverCooldown > 0)
            takeCoverCooldown -= Time.deltaTime;
    }

    IEnumerator Awareness()
    {
        while (true)
        {
            for (int i = 0; i < unitVision.VisibleEnemies.Count; i++)
            {
                if (CoverSystem.IsCoveredFrom(hc, unitVision.VisibleEnemies[i]))
                {
                    
                }
                else if (takeCoverCooldown <= 0)
                {
                    TakeCoverOrder();
                }
                yield return null;   
            }
            yield return null;   
        }
    }

    void StopAllBehaviorCoroutines()
    {
        if (followTargetCoroutine != null)
            StopCoroutine(followTargetCoroutine);
        if (takeCoverCoroutine != null)
            StopCoroutine(takeCoverCoroutine);
    }

    public void TakeCoverOrder()
    {
        currentOrder = Order.TakeCover;
        StopAllBehaviorCoroutines();
        takeCoverCooldown = 1;
        takeCoverCoroutine = StartCoroutine(TakeCover());
    }

    private Coroutine takeCoverCoroutine;

    IEnumerator TakeCover()
    {
        // FIND GOOD COVERS TO HIDE FROM EVERY VISIBLE ENEMY
        List<Transform> goodCoverPoints = new List<Transform>();
        for (int i = 0; i < unitVision.VisibleEnemies.Count; i++)
        {
            var covers = CoverSystem.Instance.GetAvailableCoverPoints(unitVision.VisibleEnemies[i].transform);
            for (int j = 0; j < covers.Count; j++)
            {
                goodCoverPoints.Add(covers[i]);
            }
            yield return null;
        }
        
        // PICK CLOSEST COVER
        Transform closestCoverPoint = null;
        float distance = 1000;
        for (int i = 0; i < goodCoverPoints.Count; i++)
        {
            float newDistance = Vector3.Distance(goodCoverPoints[i].position, transform.position);
            if (newDistance < distance)
            {
                Debug.Log(newDistance);
                distance = newDistance;
                closestCoverPoint = goodCoverPoints[i];
            }
        }

        if (closestCoverPoint == null)
        {
            yield break;
        }
        
        //SET PATH
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, closestCoverPoint.position, NavMesh.AllAreas, path);
        
        agent.speed = moveSpeed;
        agent.stoppingDistance = stopDistanceMove;
        agent.SetPath(path);

        while (Vector3.Distance(transform.position, closestCoverPoint.position) > 1)
        {
            yield return new WaitForSeconds(0.5f);
        }

        FireWatchOrder();
    }


    public void FireWatchOrder()
    {
        StopAllBehaviorCoroutines();
        currentOrder = Order.FireWatch;
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
