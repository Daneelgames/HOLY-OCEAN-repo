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

    private CoverSpot occupiedCoverSpot;

    private Vector3 currentTargetPosition;
    List<CoverSpot> goodCoverPoints = new List<CoverSpot>();
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
                else if (takeCoverCooldown <= 0 && currentOrder != Order.FollowLeader && currentOrder != Order.MoveToPosition)
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
        SetOccupiedSpot(occupiedCoverSpot, null);
        
        if (moveToPositionCoroutine != null)
            StopCoroutine(moveToPositionCoroutine);
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
        goodCoverPoints = CoverSystem.Instance.FindCover(transform, unitVision.VisibleEnemies);

        /*
        for (int i = 0; i < unitVision.VisibleEnemies.Count; i++)
        {
            var covers = CoverSystem.Instance.GetAvailableCoverPoints(transform, unitVision.VisibleEnemies[i].transform);
            for (int j = 0; j < covers.Count; j++)
            {
                if (goodCoverPoints.Contains(covers[j]))
                    continue;
                
                goodCoverPoints.Add(covers[j]);
            }
            yield return null;
        }
        */
        
        // PICK CLOSEST COVER
        CoverSpot closestCoverPoint = null;
        float distance = 1000;
        for (int i = 0; i < goodCoverPoints.Count; i++)
        {
            float newDistance = Vector3.Distance(goodCoverPoints[i].transform.position, transform.position);
            if (newDistance < distance)
            {
                distance = newDistance;
                closestCoverPoint = goodCoverPoints[i];
            }
        }

        if (closestCoverPoint == null)
        {
            yield break;
        }
        
        //Spot occupied!
        SetOccupiedSpot(closestCoverPoint, hc);
        
        //SET PATH
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, closestCoverPoint.transform.position, NavMesh.AllAreas, path);
        
        agent.speed = moveSpeed;
        agent.stoppingDistance = stopDistanceMove;
        agent.SetPath(path);
        currentTargetPosition = closestCoverPoint.transform.position;

        while (Vector3.Distance(transform.position, closestCoverPoint.transform.position) > 0.33f)
        {
            if (closestCoverPoint.Occupator != hc)
            {
                FireWatchOrder();
                yield break;
            }
            yield return new WaitForSeconds(0.5f);
        }

        FireWatchOrder();
    }

    public void SetOccupiedSpot(CoverSpot spot, HealthController occupied)
    {
        //Spot occupied!
        occupiedCoverSpot = occupied ? spot : null;

        if (spot)
        {
            if (occupied == null && spot.Occupator != hc)
                return;
            
            spot.Occupator = occupied;
        }
    }
    
    public void FireWatchOrder()
    {
        StopAllBehaviorCoroutines();
        currentTargetPosition = transform.position;
        currentOrder = Order.FireWatch;
    }
    public void FollowTargetOrder(Transform target)
    {
        StopAllBehaviorCoroutines();
        currentOrder = Order.FollowLeader;
        followTargetCoroutine = StartCoroutine(FollowTarget(target.position));
    }

    private Coroutine followTargetCoroutine;
    IEnumerator FollowTarget(Vector3 target)
    {
        agent.stoppingDistance = stopDistanceFollow;
        NavMeshPath path = new NavMeshPath();
        while (true)
        {
            if (NavMesh.SamplePosition(target, out var hit, 10f, NavMesh.AllAreas))
            {
                target = hit.position;
            }
            NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path);
            agent.speed = moveSpeed;
            agent.SetPath(path);
            currentTargetPosition = target;
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void MoveToPositionOrder(Vector3 targetPos)
    {
        StopAllBehaviorCoroutines();
        currentOrder = Order.MoveToPosition;
        moveToPositionCoroutine = StartCoroutine(MoveToPosition(targetPos));
    }
    
    private Coroutine moveToPositionCoroutine;

    IEnumerator MoveToPosition(Vector3 target)
    {
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path);
        
        agent.speed = moveSpeed;
        agent.stoppingDistance = stopDistanceMove;
        agent.SetPath(path);
        currentTargetPosition = target;
        
        while (Vector3.Distance(transform.position, target) > 1)
        {
            yield return new WaitForSeconds(0.5f);
        }

        FireWatchOrder();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(currentTargetPosition , Vector3.one);
    }

    public void RunOrder()
    {
        agent.speed = runSpeed;
    }
}
