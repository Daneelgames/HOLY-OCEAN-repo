using System;
using System.Collections;
using System.Collections.Generic;
using Brezg.Extensions.UniTaskExtensions;
using Cysharp.Threading.Tasks;
using FishNet.Object;
using MrPink.Health;
using MrPink.PlayerSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace MrPink.Units
{
    public class UnitAiMovement : MonoBehaviour
    {
        [SerializeField, ChildGameObjectsOnly, Required]
        private Unit _selfUnit;
        
        public EnemiesBehaviour coverFoundBehaviour = EnemiesBehaviour.ApproachEnemy;
        public EnemiesBehaviour noCoverBehaviour = EnemiesBehaviour.ApproachEnemy;
        public EnemiesBehaviour setDamagerBehaviour = EnemiesBehaviour.ApproachEnemy;
        
        public bool followClosestEnemyOnSpawn = false;
        public MovementOrder currentOrder = MovementOrder.FollowTarget;

        private bool inCover = false;

        public HealthController enemyToLookAt;
        
        private CoverSpot _occupiedCoverSpot;
        private float _takeCoverCooldown = 0;
        
        private Vector3 _currentVelocity;

        private Coroutine _takeCoverCoroutine;
        private Coroutine _getInCoverCoroutine;
        private Coroutine _moveToPositionCoroutine;
        private Coroutine _rotateToPositionCoroutine;


        private void Start()
        {
            _selfUnit.HealthController.OnDeathEvent.AddListener(StopActivities);

            Awareness().ForgetWithHandler();
        }

        private void OnEnable()
        {
            if (followClosestEnemyOnSpawn)
            {
                var targetHc = TeamsManager.Instance.FindClosestEnemyInRange(_selfUnit.HealthController.team, transform.position).transform;
                if (targetHc)
                {
                    FollowTargetOrder(targetHc.transform);
                    return;
                }
            }
            
            TakeCoverOrder();
        }

        private void Update()
        {
            if (_selfUnit.HealthController.health <= 0)
                return;
            
            if (enemyToLookAt != null && !inCover && enemyToLookAt)
                _selfUnit.UnitMovement.LookAt(enemyToLookAt.transform.position);

            if (_takeCoverCooldown > 0)
                _takeCoverCooldown -= Time.deltaTime;
        }

        private float lookForNewEnemyCooldown = 0;
        public void SetDamager(HealthController hc)
        {
            if (setDamagerBehaviour == EnemiesBehaviour.ApproachEnemy)
            {
                FollowTargetOrder(hc.transform);
            }
            else if (setDamagerBehaviour == EnemiesBehaviour.HideFromEnemy)
            {
                TakeCoverOrder(true, false, hc);
            }
            enemyToLookAt = hc;
            lookForNewEnemyCooldown = 1;
        }

        public void FollowIntruder(HealthController hc)
        {
            FollowTargetOrder(hc.transform);
            enemyToLookAt = hc;
            lookForNewEnemyCooldown = 1;
        }
        
        private async UniTask Awareness()
        {
            while (_selfUnit.HealthController.health > 0)
            {
                if (lookForNewEnemyCooldown <= 0)
                    enemyToLookAt = await _selfUnit.UnitVision.GetClosestVisibleEnemy();
                else
                    lookForNewEnemyCooldown -= Time.deltaTime;
                
                if (enemyToLookAt != null)
                    TryCoverFromClosest(enemyToLookAt);
                
                await UniTask.DelayFrame(1);
            }

            StopAllBehaviorCoroutines();
        }
        

        private void TryCoverFromClosest(HealthController enemy)
        {
            if (enemy == null)
                return;
            
            var isCovered = CoverSystem.IsCoveredFrom(_selfUnit.HealthController, enemy);

            if (isCovered)
                return;

            if (_takeCoverCooldown > 0)
                return;

            if (currentOrder == MovementOrder.FollowTarget)
                return;

            if (currentOrder == MovementOrder.MoveToPosition)
                return;
                        
            TakeCoverOrder(false, true, enemy);
        }

        private void StopAllBehaviorCoroutines()
        {
            if (_moveToPositionCoroutine != null)
                StopCoroutine(_moveToPositionCoroutine);
            if (_rotateToPositionCoroutine != null)
                StopCoroutine(_rotateToPositionCoroutine);
            
            _selfUnit.UnitFollowTarget.StopFollowing();
            
            if (_takeCoverCoroutine != null)
                StopCoroutine(_takeCoverCoroutine);
        }


        private void TakeCoverOrder(bool random = false, bool closest = true, HealthController takeCoverFrom = null)
        {
            SetOccupiedSpot(_occupiedCoverSpot, null);
            StopAllBehaviorCoroutines();
            currentOrder = MovementOrder.TakeCover;
            _takeCoverCooldown = CoverSystem.Instance? CoverSystem.Instance.TakeCoverCooldown : 5;
            _takeCoverCoroutine = StartCoroutine(TakeCover(random, closest, takeCoverFrom));
        }

        
        private IEnumerator TakeCover(bool randomCover, bool closest, HealthController enemy = null)
        {
            while (CoverSystem.Instance == null)
            {
                yield return null;
            }
            CoverSpot chosenCover = GetCover(randomCover, closest);
            
            if (chosenCover == null)
            {
                // CAN'T FIND COVER
                Debug.Log(gameObject.name +  " can't find cover. takeCoverFrom " + enemy);
                if (enemy == null)
                    yield break;
            
                if (noCoverBehaviour == EnemiesBehaviour.HideFromEnemy)
                {
                    // TO DO - переделать на систему вейпойнтов / спавнеров
                    Vector3 targetPos = transform.position + (transform.position - enemy.transform.position).normalized * 5;

                    _selfUnit.UnitMovement.AgentSetPath(targetPos, true);
                }
                else
                    FollowTargetOrder(enemy.transform);
                
                yield break;
            }
        
            // GOOD COVER FOUND!
            ///
            ///
            // TRY TO FOLLOW ENEMY
            if (enemy && coverFoundBehaviour == EnemiesBehaviour.ApproachEnemy)
            {
                FollowTargetOrder(enemy.transform);
                //AgentSetPath(enemy.transform.position, stopDistanceFollow);
                yield break;
            }
        
            // TRY TO OCCUPY COVER SPOT
            
            // CHOSEN SPOT occupied!
            SetOccupiedSpot(chosenCover, _selfUnit.HealthController);
        
            //SET PATH TO SPOT
            if (_selfUnit.UnitMovement != null)
                _selfUnit.UnitMovement.AgentSetPath(chosenCover.transform.position, false);
            
            var spotPosition = _occupiedCoverSpot.transform.position;

            while (Vector3.Distance(transform.position, spotPosition) > 0.33f)
            {
                if (_occupiedCoverSpot.Occupator != _selfUnit.HealthController)
                {
                    FireWatchOrder();
                    yield break;
                }
                yield return new WaitForSeconds(0.5f);
            }

            FireWatchOrder();
        }

        private CoverSpot GetCover(bool isFromRandomPool, bool isClosestNeeded)
        {
            var goodCoverSpots = isFromRandomPool 
                ? CoverSystem.Instance.GetAllCovers()
                : CoverSystem.Instance.FindCover(transform, _selfUnit.UnitVision.visibleEnemies);
            
            if (isClosestNeeded)
                return GetClosestCover(goodCoverSpots);
            
            if (goodCoverSpots.Count < 1) 
                return null;
            
            return goodCoverSpots[Random.Range(0, goodCoverSpots.Count)];

        }

        private CoverSpot GetClosestCover(List<CoverSpot> goodCoverSpots)
        {
            float distance = 1000;
            CoverSpot closest = null;
            foreach (var cover in goodCoverSpots)
            {
                if (cover == null)
                    continue;
                
                float newDistance = Vector3.Distance(cover.transform.position, transform.position);
                
                if (newDistance >= distance)
                    continue;
                
                distance = newDistance;
                closest = cover;
            }

            return closest;
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

            if (!spot) 
                return;
            
            if (occupator == null && spot.Occupator != _selfUnit.HealthController)
                return;

            if (occupator == _selfUnit.HealthController)
                _getInCoverCoroutine = StartCoroutine(CheckIfCloseToCover());

            spot.Occupator = occupator;
        }

        private void SetInCover(bool isInCover)
        {
            inCover = isInCover;
            _selfUnit.HealthController.HumanVisualController.SetInCover(isInCover);
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
            _selfUnit.UnitFollowTarget.FollowTarget(target);
        }

        
        public void MoveToPositionOrder(Vector3 targetPos)
        { 
            SetOccupiedSpot(_occupiedCoverSpot, null);
            StopAllBehaviorCoroutines();
            currentOrder = MovementOrder.MoveToPosition;
            _moveToPositionCoroutine = StartCoroutine(MoveToPositionAndFireWatchOrder(targetPos));
        }

        public void RotateToPositionOrder(Vector3 targetPos)
        { 
            SetOccupiedSpot(_occupiedCoverSpot, null);
            StopAllBehaviorCoroutines();
            currentOrder = MovementOrder.RotateToPosition;
            _rotateToPositionCoroutine = StartCoroutine(RotateToPositionAndFireWatchOrder(targetPos));
        }

        private IEnumerator RotateToPositionAndFireWatchOrder(Vector3 target)
        {
            yield return _selfUnit.UnitMovement.RotateToPosition(target);
            
            FireWatchOrder();
        }
        private IEnumerator MoveToPositionAndFireWatchOrder(Vector3 target)
        {
            yield return _selfUnit.UnitMovement.MoveToPosition(target);
            
            FireWatchOrder();
        }
        
        public void RunOrder()
        {
            _selfUnit.UnitMovement.Run();
        }
    
        public void StopActivities()
        {
            _selfUnit.UnitMovement.Death();
            
            //this.enabled = false;
            
            StopAllBehaviorCoroutines();
            _selfUnit.HumanVisualController?.SetMovementVelocity(Vector3.zero);
        }

        public void RestartActivities()
        {
            
            _selfUnit.UnitMovement.RestoreAgent();
            
            HealthController enemy = null;
            if (_selfUnit.UnitVision.visibleEnemies.Count > 0)
                enemy = _selfUnit.UnitVision.visibleEnemies[Random.Range(0, _selfUnit.UnitVision.visibleEnemies.Count)];
                
            TakeCoverOrder(false, true, enemy);
        }


#if UNITY_EDITOR
        
        public void SetUnit(Unit unit)
        {
            _selfUnit = unit;
            EditorUtility.SetDirty(_selfUnit);
        }
        
#endif
    }
}