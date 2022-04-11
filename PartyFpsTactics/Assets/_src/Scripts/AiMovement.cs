using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class AiMovement : MonoBehaviour
{
    public enum Order
    {
        FollowTarget, MoveToPosition, TakeCover, FireWatch
    }
    
    public enum EnemiesBehaviour
    {
        HideFromEnemy, ApproachEnemy
    }

    public EnemiesBehaviour coverFoundBehaviour = EnemiesBehaviour.ApproachEnemy;
    public EnemiesBehaviour noCoverBehaviour = EnemiesBehaviour.ApproachEnemy;

    public Order currentOrder = Order.FollowTarget;
    public NavMeshAgent agent;
    [Range(1,100)]
    public float moveSpeed = 2;
    [Range(1,100)]
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
    private void Awake()
    {
        hc = GetComponent<HealthController>();
    }

    private void Start()
    {
        lookTransform = new GameObject(gameObject.name + "LookTransform").transform;

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
                yield return null;
                
                if (i >= unitVision.VisibleEnemies.Count)
                    continue;
                
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
                  else if (takeCoverCooldown <= 0 && currentOrder != Order.FollowTarget &&
                           currentOrder != Order.MoveToPosition)
                  {
                      TakeCoverOrder(false, true, unitVision.VisibleEnemies[i]);
                      yield return null;
                  }
                }   
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


    public void TakeCoverOrder(bool random = false, bool closest = true, HealthController takeCoverFrom = null)
    {
        SetOccupiedSpot(occupiedCoverSpot, null);
        StopAllBehaviorCoroutines();
        currentOrder = Order.TakeCover;
        takeCoverCooldown = CoverSystem.Instance.TakeCoverCooldown;
        takeCoverCoroutine = StartCoroutine(TakeCover(random, closest, takeCoverFrom));
    }

    
    
    private Coroutine takeCoverCoroutine;

    IEnumerator TakeCover(bool randomCover, bool closest, HealthController enemy = null)
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
            // CAN'T FIND COVER
            Debug.Log(gameObject.name +  " can't find cover. takeCoverFrom " + enemy);
            if (enemy == null)
                yield break;
            
            if (noCoverBehaviour == EnemiesBehaviour.HideFromEnemy)
            {
                Vector3 targetPos = enemy.transform.position;
                float targetStopDistance = stopDistanceFollow;
                targetPos = transform.position + (enemy.transform.position - transform.position).normalized * 5;
                targetStopDistance = stopDistanceFollow;
                AgentSetPath(targetPos, targetStopDistance);
            }
            else
            {
                RunOrder();
                FollowTargetOrder(enemy.transform);
            }
            yield break;
        }
        
        // GOOD COVER FOUND
        if (enemy && coverFoundBehaviour == EnemiesBehaviour.ApproachEnemy)
        {
            RunOrder();
            FollowTargetOrder(enemy.transform);
            //AgentSetPath(enemy.transform.position, stopDistanceFollow);
            yield break;
        }
        
        
        // CHOSEN SPOT occupied!
        SetOccupiedSpot(chosenCover, hc);
        
        //SET PATH
        if (agent && agent.enabled)
        {
            AgentSetPath(chosenCover.transform.position, stopDistanceMove);
        }
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
                RunOrder();
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
        currentOrder = Order.FollowTarget;
        followTargetCoroutine = StartCoroutine(FollowTarget(target));
    }

    private Coroutine followTargetCoroutine;
    IEnumerator FollowTarget(Transform target)
    {
        while (true)
        {
            if (!agent || !agent.enabled)
                yield break;
            
            AgentSetPath(target.position, stopDistanceFollow);
            
            currentTargetPosition = target.position;
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
        AgentSetPath(target, stopDistanceMove);
        currentTargetPosition = target;
        
        while (Vector3.Distance(transform.position, target) > 1)
        {
            yield return new WaitForSeconds(0.5f);
        }

        FireWatchOrder();
    }

    void AgentSetPath(Vector3 target, float stopDistance)
    {
        NavMeshPath path = new NavMeshPath();
        
        transform.position = SamplePos(transform.position);
        NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, path);
        agent.speed = moveSpeed;
        agent.stoppingDistance = stopDistance;
        agent.SetPath(path);
    }
    
    Vector3 SamplePos(Vector3 startPos)
    {
        if (NavMesh.SamplePosition(startPos, out var hit, 10f, NavMesh.AllAreas))
        {
            startPos = hit.position;
        }

        return startPos;
    }
    
    public void RunOrder()
    {
        agent.speed = runSpeed;
    }
    
    public void Death()
    {
        agent.enabled = false;
        StopAllBehaviorCoroutines();
        humanVisualController.SetMovementVelocity(Vector3.zero);
    }

    public void Resurrect()
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 5.0f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
        }
        agent.enabled = true;
        HealthController enemy = null;
        if (unitVision.VisibleEnemies.Count > 0)
            enemy = unitVision.VisibleEnemies[Random.Range(0, unitVision.VisibleEnemies.Count)];
                
        TakeCoverOrder(false, true, enemy);
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
