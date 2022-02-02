using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class AiMovement : MonoBehaviour
{
    public enum Order
    {
        FollowLeader, MoveToPosition, TakeCover, FireWatch
    }

    public Order currentOrder = Order.FollowLeader;
    public NavMeshAgent agent;
    [Range(1,5)]
    public float moveSpeed = 2;
    [Range(1,10)]
    public float runSpeed = 4;
    [Range(1,10)]
    public float turnSpeed = 4;
    
    public float stopDistanceFollow = 1.5f;
    public float stopDistanceMove = 0;

    float updateDelayCurrent = 0.1f;
    private Vector3 currentVelocity;

    public HumanVisualController humanVisualController;
    public UnitVision unitVision;

    private HealthController hc;
    private float takeCoverCooldown = 0;

    private CoverSpot occupiedCoverSpot;
    private bool inCover = false;

    private Vector3 currentTargetPosition;
    List<CoverSpot> goodCoverPoints = new List<CoverSpot>();
    public HealthController enemyToLookAt;
    private Transform lookTransform;
    public NavMeshSurface navMeshBubble;
    private void Awake()
    {
        hc = GetComponent<HealthController>();
    }

    private void Start()
    {
        lookTransform = new GameObject(gameObject.name + "LookTransform").transform;

        if (navMeshBubble)
        {
            navMeshBubble.BuildNavMesh();
            LevelGenerator.Instance.AddNavMeshBubble(navMeshBubble);
        }
        StartCoroutine(Awareness());
    }

    private void Update()
    {
        if (hc.health <= 0)
            return;
        
        currentVelocity = agent.velocity;
        humanVisualController.SetMovementVelocity(currentVelocity);
        lookTransform.transform.position = transform.position;
        if (enemyToLookAt && !inCover)
        {
            lookTransform.LookAt(enemyToLookAt.transform.position, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookTransform.rotation, Time.deltaTime * turnSpeed);   
        }
        if (takeCoverCooldown > 0)
            takeCoverCooldown -= Time.deltaTime;
    }

    IEnumerator Awareness()
    {
        while (hc.health > 0)
        {
            float distance = 1000;
            HealthController closestVisibleEnemy = null;
            for (int i = unitVision.VisibleEnemies.Count - 1; i >= 0; i--)
            {
                if (unitVision.VisibleEnemies[i])
                {
                  float newDistance = Vector3.Distance(transform.position, unitVision.VisibleEnemies[i].transform.position);
                  if (newDistance < distance)
                  {
                      distance = newDistance;
                      closestVisibleEnemy = unitVision.VisibleEnemies[i];
                  }

                  if (CoverSystem.IsCoveredFrom(hc, unitVision.VisibleEnemies[i]))
                  {

                  }
                  else if (takeCoverCooldown <= 0 && currentOrder != Order.FollowLeader &&
                           currentOrder != Order.MoveToPosition)
                  {
                      TakeCoverOrder();
                  }
                }
                yield return null;   
            }

            enemyToLookAt = closestVisibleEnemy;
            yield return null;   
        }

        StopAllBehaviorCoroutines();
    }

    public void StopAllBehaviorCoroutines()
    {
        if (moveToPositionCoroutine != null)
            StopCoroutine(moveToPositionCoroutine);
        if (followTargetCoroutine != null)
            StopCoroutine(followTargetCoroutine);
        if (takeCoverCoroutine != null)
            StopCoroutine(takeCoverCoroutine);
    }


    public void TakeCoverOrder(bool random = false, bool closest = true)
    {
        SetOccupiedSpot(occupiedCoverSpot, null);
        StopAllBehaviorCoroutines();
        currentOrder = Order.TakeCover;
        takeCoverCooldown = CoverSystem.Instance.TakeCoverCooldown;
        takeCoverCoroutine = StartCoroutine(TakeCover(random, closest));
    }

    
    
    private Coroutine takeCoverCoroutine;

    IEnumerator TakeCover(bool randomCover, bool closest)
    {
        if (randomCover)
        {
            goodCoverPoints = CoverSystem.Instance.GetAllCovers();
        }
        else
        {
            // FIND GOOD COVERS TO HIDE FROM EVERY VISIBLE ENEMY
            goodCoverPoints = CoverSystem.Instance.FindCover(transform, unitVision.VisibleEnemies);   
        }


        CoverSpot chosenCover = null;
        if (closest)
        {
            // PICK CLOSEST COVER
            float distance = 1000;
            for (int i = 0; i < goodCoverPoints.Count; i++)
            {
                if (goodCoverPoints[i] == null)
                    continue;
                
                float newDistance = Vector3.Distance(goodCoverPoints[i].transform.position, transform.position);
                if (newDistance < distance)
                {
                    distance = newDistance;
                    chosenCover = goodCoverPoints[i];
                }
            }   
        }
        else
        {
            chosenCover = goodCoverPoints[Random.Range(0, goodCoverPoints.Count)];
        }

        if (chosenCover == null)
        {
            yield break;
        }
        
        //Spot occupied!
        SetOccupiedSpot(chosenCover, hc);
        
        //SET PATH
        NavMeshPath path = new NavMeshPath();
        NavMesh.CalculatePath(transform.position, chosenCover.transform.position, NavMesh.AllAreas, path);
        
        agent.speed = moveSpeed;
        agent.stoppingDistance = stopDistanceMove;
        agent.SetPath(path);
        currentTargetPosition = occupiedCoverSpot.transform.position;

        while (Vector3.Distance(transform.position, currentTargetPosition) > 0.33f)
        {
            if (occupiedCoverSpot.Occupator != hc)
            {
                FireWatchOrder();
                yield break;
            }
            yield return new WaitForSeconds(0.5f);
        }

        FireWatchOrder();
    }

    public void SetOccupiedSpot(CoverSpot spot, HealthController occupator)
    {
        //Spot occupied!
        occupiedCoverSpot = occupator ? spot : null;

        if (!spot || !occupator)
        {
            if (getInCoverCoroutine!= null)
                StopCoroutine(getInCoverCoroutine);
            
            SetInCover(false);
        }
        
        if (spot)
        {
            if (occupator == null && spot.Occupator != hc)
                return;

            if (occupator == hc)
            {
                getInCoverCoroutine = StartCoroutine(CheckIfCloseToCover());
            }
            
            spot.Occupator = occupator;
        }
    }

    void SetInCover(bool _inCover)
    {
        inCover = _inCover;
        hc.HumanVisualController.SetInCover(_inCover);
    }

    private Coroutine getInCoverCoroutine;
    IEnumerator CheckIfCloseToCover()
    {
        while (occupiedCoverSpot)
        {
            if (Vector3.Distance(transform.position, occupiedCoverSpot.transform.position) < 1)
            {
                SetInCover(true);
                transform.rotation = Quaternion.Slerp(transform.rotation, occupiedCoverSpot.transform.rotation, Time.deltaTime * 3);
            }
            else
            {
                SetInCover(false);
            }
            yield return null;

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
        SetOccupiedSpot(occupiedCoverSpot, null);
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
            if (!agent || !agent.enabled)
                yield break;
            
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
        SetOccupiedSpot(occupiedCoverSpot, null);
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

    public void RunOrder()
    {
        agent.speed = runSpeed;
    }
    
    public void Death()
    {
        agent.enabled = false;
        if (navMeshBubble)
            LevelGenerator.Instance.RemoveNavMeshBubble(navMeshBubble);
        StopAllBehaviorCoroutines();
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(currentTargetPosition , Vector3.one);
    }

    private void OnDestroy()
    {
        if (lookTransform)
            Destroy(lookTransform.gameObject);
    }
}
