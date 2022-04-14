using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace MrPink.Units
{
    public class UnitAi : MonoBehaviour
    {
        [SerializeField, ChildGameObjectsOnly, Required]
        private UnitMovement _selfMovement;
        
        [SerializeField, ChildGameObjectsOnly, Required]
        private UnitFollowTarget _selfFollow;
        
        [SerializeField, ChildGameObjectsOnly, Required]
        private HealthController _selfHealth;
        
        
        public EnemiesBehaviour coverFoundBehaviour = EnemiesBehaviour.ApproachEnemy;
        public EnemiesBehaviour noCoverBehaviour = EnemiesBehaviour.ApproachEnemy;

        public MovementOrder currentOrder = MovementOrder.FollowTarget;


        public HumanVisualController humanVisualController;
        public UnitVision unitVision;
        
        private bool inCover = false;
        
        public HealthController enemyToLookAt;

        private List<CoverSpot> _goodCoverPoints = new List<CoverSpot>();
        private CoverSpot _occupiedCoverSpot;
        private float _takeCoverCooldown = 0;
        
        private Vector3 _currentVelocity;
        
        
        private Coroutine _takeCoverCoroutine;
        private Coroutine _getInCoverCoroutine;
        private Coroutine _moveToPositionCoroutine;
        

        private void Start()
        {
            _selfHealth.OnDeathEvent.AddListener(Death);
            
            StartCoroutine(Awareness());
        }

        private void Update()
        {
            if (enemyToLookAt != null && !inCover)
                _selfMovement.LookAt(enemyToLookAt.transform.position);

            if (_takeCoverCooldown > 0)
                _takeCoverCooldown -= Time.deltaTime;
        }
        

        private IEnumerator Awareness()
        {
            while (_selfHealth.health > 0)
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

                        var isCovered = CoverSystem.IsCoveredFrom(_selfHealth, unitVision.VisibleEnemies[i]);
                        
                        if (
                            !isCovered &&
                            _takeCoverCooldown <= 0 &&
                            currentOrder != MovementOrder.FollowTarget &&
                            currentOrder != MovementOrder.MoveToPosition
                            )
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

        private void StopAllBehaviorCoroutines()
        {
            if (_moveToPositionCoroutine != null)
                StopCoroutine(_moveToPositionCoroutine);
            
            _selfFollow.StopFollowing();
            
            if (_takeCoverCoroutine != null)
                StopCoroutine(_takeCoverCoroutine);
        }


        public void TakeCoverOrder(bool random = false, bool closest = true, HealthController takeCoverFrom = null)
        {
            SetOccupiedSpot(_occupiedCoverSpot, null);
            StopAllBehaviorCoroutines();
            currentOrder = MovementOrder.TakeCover;
            _takeCoverCooldown = CoverSystem.Instance.TakeCoverCooldown;
            _takeCoverCoroutine = StartCoroutine(TakeCover(random, closest, takeCoverFrom));
        }

        
        private IEnumerator TakeCover(bool randomCover, bool closest, HealthController enemy = null)
        {
            if (randomCover)
                _goodCoverPoints = CoverSystem.Instance.GetAllCovers();
            else
                // FIND GOOD COVERS TO HIDE FROM EVERY VISIBLE ENEMY
                _goodCoverPoints = CoverSystem.Instance.FindCover(transform, unitVision.VisibleEnemies);


            CoverSpot chosenCover = null;
            if (closest)
            {
                // PICK CLOSEST COVER
                float distance = 1000;
                for (int i = 0; i < _goodCoverPoints.Count; i++)
                {
                    if (_goodCoverPoints[i] == null)
                        continue;
                
                    float newDistance = Vector3.Distance(_goodCoverPoints[i].transform.position, transform.position);
                    if (newDistance < distance)
                    {
                        distance = newDistance;
                        chosenCover = _goodCoverPoints[i];
                    }
                }   
            }
            else
                chosenCover = _goodCoverPoints[Random.Range(0, _goodCoverPoints.Count)];

            if (chosenCover == null)
            {
                // CAN'T FIND COVER
                Debug.Log(gameObject.name +  " can't find cover. takeCoverFrom " + enemy);
                if (enemy == null)
                    yield break;
            
                if (noCoverBehaviour == EnemiesBehaviour.HideFromEnemy)
                {
                    Vector3 targetPos = enemy.transform.position;
                    targetPos = transform.position + (enemy.transform.position - transform.position).normalized * 5;

                    _selfMovement.AgentSetPath(targetPos, true);
                }
                else
                    FollowTargetOrder(enemy.transform);
                
                yield break;
            }
        
            // GOOD COVER FOUND
            if (enemy && coverFoundBehaviour == EnemiesBehaviour.ApproachEnemy)
            {
                FollowTargetOrder(enemy.transform);
                //AgentSetPath(enemy.transform.position, stopDistanceFollow);
                yield break;
            }
        
        
            // CHOSEN SPOT occupied!
            SetOccupiedSpot(chosenCover, _selfHealth);
        
            //SET PATH
            if (_selfMovement != null)
                _selfMovement.AgentSetPath(chosenCover.transform.position, false);
            
            var spotPosition = _occupiedCoverSpot.transform.position;

            while (Vector3.Distance(transform.position, spotPosition) > 0.33f)
            {
                if (_occupiedCoverSpot.Occupator != _selfHealth)
                {
                    FireWatchOrder();
                    yield break;
                }
                yield return new WaitForSeconds(0.5f);
            }

            FireWatchOrder();
        }

        private void SetOccupiedSpot(CoverSpot spot, HealthController occupator)
        {
            //Spot occupied!
            _occupiedCoverSpot = occupator ? spot : null;

            if (!spot || !occupator)
            {
                if (_getInCoverCoroutine!= null)
                    StopCoroutine(_getInCoverCoroutine);
            
                SetInCover(false);
            }
        
            if (spot)
            {
                if (occupator == null && spot.Occupator != _selfHealth)
                    return;

                if (occupator == _selfHealth)
                {
                    _getInCoverCoroutine = StartCoroutine(CheckIfCloseToCover());
                }
            
                spot.Occupator = occupator;
            }
        }

        private void SetInCover(bool isInCover)
        {
            inCover = isInCover;
            _selfHealth.HumanVisualController.SetInCover(isInCover);
        }

        
        private IEnumerator CheckIfCloseToCover()
        {
            while (_occupiedCoverSpot)
            {
                if (Vector3.Distance(transform.position, _occupiedCoverSpot.transform.position) < 1)
                {
                    SetInCover(true);
                    transform.rotation = Quaternion.Slerp(transform.rotation, _occupiedCoverSpot.transform.rotation, Time.deltaTime * 3);
                }
                else
                    SetInCover(false);
                
                yield return null;

            }
        }
    
        private void FireWatchOrder()
        {
            StopAllBehaviorCoroutines();
            currentOrder = MovementOrder.FireWatch;
        }
        
        public void FollowTargetOrder(Transform target)
        {
            SetOccupiedSpot(_occupiedCoverSpot, null);
            StopAllBehaviorCoroutines();
            currentOrder = MovementOrder.FollowTarget;
            _selfFollow.FollowTarget(target);
        }

        
        public void MoveToPositionOrder(Vector3 targetPos)
        { 
            SetOccupiedSpot(_occupiedCoverSpot, null);
            StopAllBehaviorCoroutines();
            currentOrder = MovementOrder.MoveToPosition;
            _moveToPositionCoroutine = StartCoroutine(MoveToPositionAndFireWatchOrder(targetPos));
        }

        private IEnumerator MoveToPositionAndFireWatchOrder(Vector3 target)
        {
            yield return _selfMovement.MoveToPosition(target);
            
            FireWatchOrder();
        }
        
        public void RunOrder()
        {
            _selfMovement.Run();
        }
    
        public void Death()
        {
            _selfMovement.Death();
            this.enabled = false;
            
            StopAllBehaviorCoroutines();
            humanVisualController.SetMovementVelocity(Vector3.zero);
        }

        public void Resurrect()
        {
            _selfMovement.Resurrect();
            
            HealthController enemy = null;
            if (unitVision.VisibleEnemies.Count > 0)
                enemy = unitVision.VisibleEnemies[Random.Range(0, unitVision.VisibleEnemies.Count)];
                
            TakeCoverOrder(false, true, enemy);
        }


#if UNITY_EDITOR

        [ContextMenu("Transfer movement")]
        private void TransferMovementData()
        {
            _selfHealth = GetComponent<HealthController>();
            
            _selfMovement = GetComponent<UnitMovement>();
            if (_selfMovement == null)
            {
                _selfMovement = gameObject.AddComponent<UnitMovement>();
                EditorUtility.SetDirty(this);
            }
            
            _selfMovement.TransferData(this);
            EditorUtility.SetDirty(_selfMovement);
            
            _selfFollow = GetComponent<UnitFollowTarget>();
            if (_selfFollow == null)
            {
                _selfFollow = gameObject.AddComponent<UnitFollowTarget>();
                EditorUtility.SetDirty(this);
            }
            
            _selfFollow.TransferData();
            EditorUtility.SetDirty(_selfFollow);
            
            AssetDatabase.SaveAssets();
        }
        
#endif
    }
}