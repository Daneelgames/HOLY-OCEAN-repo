using System.Collections;
using System.Collections.Generic;
using Brezg.Extensions.UniTaskExtensions;
using Cysharp.Threading.Tasks;
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
        [SerializeField] 
        private Team _team;
        
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
        
        private CoverSpot _occupiedCoverSpot;
        private float _takeCoverCooldown = 0;
        
        private Vector3 _currentVelocity;
        
        
        private Coroutine _takeCoverCoroutine;
        private Coroutine _getInCoverCoroutine;
        private Coroutine _moveToPositionCoroutine;


        private void Start()
        {
            _selfHealth.OnDeathEvent.AddListener(StopActivities);

            Awareness().ForgetWithHandler();

            if (_team == Team.Red && Random.value > 0.9f)
                MoveToPositionOrder(Player.GameObject.transform.position);
            else
                TakeCoverOrder();
        }

        private void Update()
        {
            if (enemyToLookAt != null && !inCover)
                _selfMovement.LookAt(enemyToLookAt.transform.position);

            if (_takeCoverCooldown > 0)
                _takeCoverCooldown -= Time.deltaTime;
        }


        private async UniTask Awareness()
        {
            while (_selfHealth.health > 0)
            {
                enemyToLookAt = await unitVision.GetClosestVisibleEnemy();
                
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
            
            var isCovered = CoverSystem.IsCoveredFrom(_selfHealth, enemy);

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
            
            _selfFollow.StopFollowing();
            
            if (_takeCoverCoroutine != null)
                StopCoroutine(_takeCoverCoroutine);
        }


        private void TakeCoverOrder(bool random = false, bool closest = true, HealthController takeCoverFrom = null)
        {
            SetOccupiedSpot(_occupiedCoverSpot, null);
            StopAllBehaviorCoroutines();
            currentOrder = MovementOrder.TakeCover;
            _takeCoverCooldown = CoverSystem.Instance.TakeCoverCooldown;
            _takeCoverCoroutine = StartCoroutine(TakeCover(random, closest, takeCoverFrom));
        }

        
        private IEnumerator TakeCover(bool randomCover, bool closest, HealthController enemy = null)
        {
            CoverSpot chosenCover = GetCover(randomCover, closest);
            
            if (chosenCover == null)
            {
                // CAN'T FIND COVER
                Debug.Log(gameObject.name +  " can't find cover. takeCoverFrom " + enemy);
                if (enemy == null)
                    yield break;
            
                if (noCoverBehaviour == EnemiesBehaviour.HideFromEnemy)
                {
                    Vector3 targetPos = transform.position + (enemy.transform.position - transform.position).normalized * 5;

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

        private CoverSpot GetCover(bool isFromRandomPool, bool isClosestNeeded)
        {
            var goodCoverSpots = isFromRandomPool 
                ? CoverSystem.Instance.GetAllCovers()
                : CoverSystem.Instance.FindCover(transform, unitVision.visibleEnemies);
            
            if (isClosestNeeded)
                return GetClosestCover(goodCoverSpots);
            
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
            
            if (occupator == null && spot.Occupator != _selfHealth)
                return;

            if (occupator == _selfHealth)
                _getInCoverCoroutine = StartCoroutine(CheckIfCloseToCover());

            spot.Occupator = occupator;
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
    
        public void StopActivities()
        {
            _selfMovement.Death();
            this.enabled = false;
            
            StopAllBehaviorCoroutines();
            humanVisualController.SetMovementVelocity(Vector3.zero);
        }

        public void RestartActivities()
        {
            _selfMovement.Resurrect();
            
            HealthController enemy = null;
            if (unitVision.visibleEnemies.Count > 0)
                enemy = unitVision.visibleEnemies[Random.Range(0, unitVision.visibleEnemies.Count)];
                
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