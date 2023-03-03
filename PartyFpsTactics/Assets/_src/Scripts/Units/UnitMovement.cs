using System;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityAudioSource;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityVector3;
using FishNet.Object;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using Stop = BehaviorDesigner.Runtime.Tasks.Unity.UnityParticleSystem.Stop;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MrPink.Units
{
    public class UnitMovement : NetworkBehaviour
    {
        // TODO вынести конфиги передвижения в SO
        
        [SerializeField]
        [Range(1, 100)]
        public float _moveSpeed = 2;
        [SerializeField]
        [Range(1, 100)]
        public float _vaultPower = 20;
        [SerializeField]
        [Range(0.1f, 5f)]
        float rotateTime = 1;
        
        [SerializeField]
        [Range(1,10)]
        private float _turnSpeed = 4;
        
        [Range(1,100)]
        private float _runSpeed = 4;

        public float gravityForce = 13;
        [SerializeField] [ReadOnly] float currentGravityForce = 1;
        
        [SerializeField]
        private float distanceToPickNexPathPoint = 5;
    
        [SerializeField, ChildGameObjectsOnly] private Rigidbody rb;
        
        [SerializeField, ChildGameObjectsOnly, Required]
        private Unit _selfUnit;

        
        private Transform _lookTransform;
        
        public bool rememberRespawPoint = false;
        private Vector3 rememberedRespawnPoint;

        [SerializeField][ReadOnly]private Vector3 targetPositionToReach;
        public Vector3 GetTargetPositionToReach => targetPositionToReach;
        [SerializeField][ReadOnly]private DynamicPathfinder.Path currentPath;

        public override void OnStartClient() 
        { 
            base.OnStartClient();
            rb.isKinematic = base.IsClientOnly;
            rb.useGravity = !base.IsClientOnly;
        }

        private void OnEnable()
        {
            targetPositionToReach = transform.position;
            
            if (_lookTransform == null){
                _lookTransform = new GameObject(gameObject.name + "LookTransform").transform;
                _lookTransform.parent = transform;
            }
            
            if (rememberRespawPoint)
                rememberedRespawnPoint = transform.position;
            if (unitGravityCoroutine != null)
                StopCoroutine(unitGravityCoroutine);
            unitGravityCoroutine = StartCoroutine(UnitGravity());
        }

        private Coroutine unitGravityCoroutine;
        IEnumerator UnitGravity()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.1f);
                
                if (_selfUnit.HumanVisualController.IsGrounded)
                    continue;

                rb.AddForce(Vector3.down * currentGravityForce);
            }
        }

        private void Update()
        {
            if (_lookTransform)
                _lookTransform.transform.position = transform.position;

            //transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, new Vector3(0, transform.eulerAngles.y, 0), _turnSpeed * Time.deltaTime);
        }

        public void Death()
        {
            currentGravityForce = gravityForce;
            rb.isKinematic = base.IsClientOnly;
            rb.useGravity = !base.IsClientOnly;
        }

        public void RestoreMovement()
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = base.IsClientOnly;
            rb.useGravity = !base.IsClientOnly;
            targetPositionToReach = transform.position;
        }


        public void Run()
        {
            //_agent.speed = _runSpeed;
        }
        
        public void LookAt(Vector3 targetPosition)
        {
            if (_lookTransform == null)
                return;
            _lookTransform.LookAt(targetPosition, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, _lookTransform.rotation, Time.deltaTime * _turnSpeed);  
        }
        
        public IEnumerator RotateToPosition(Vector3 target)
        {
            float t = 0;
            var startRot = transform.rotation;
            var targetRot = Quaternion.LookRotation(target - transform.position, Vector3.up);
            
            while (t < rotateTime)
            {
                t += Time.deltaTime;
                transform.rotation = Quaternion.Slerp(startRot, targetRot, t/rotateTime);
                yield return null;
            }
        }
 
        public void SetTargetPositionToReach(Vector3 targetPos)
        {
            targetPositionToReach = targetPos;
        }

        public void SetNewPath(DynamicPathfinder.Path path)
        {
            currentGravityForce = 1;
            currentPath = path;
            if (moveRigidbodyCoroutine != null)
                StopCoroutine(moveRigidbodyCoroutine);
            moveRigidbodyCoroutine = StartCoroutine(MoveRigidbodyOnPath());
        }

        [SerializeField] [ReadOnly] private int currentClosestIndexOnPath;
        private Coroutine moveRigidbodyCoroutine;
        IEnumerator MoveRigidbodyOnPath()
        {
            if (currentPath.points.Count < 1)
                yield break;
            Vector3 targetPos = currentPath.points[Mathf.Clamp(GetCurrentClosestPointOnPathIndex(currentPath), 0, currentPath.points.Count-1)];
            float t = 0;
            while (_selfUnit.HealthController.health > 0)
            {
                yield return new WaitForSeconds(0.1f);
                t += 0.1f;
                if (t >= 1)
                {
                    var raycastOrigin = transform.position + Vector3.up;
                    if (Physics.Raycast(raycastOrigin, (targetPos - raycastOrigin).normalized, out var hit,
                        Mathf.Infinity, 1 << 6))
                    {
                        targetPos = hit.point;
                    }
                    t = 0;
                }

                var distance = Vector3.Distance(transform.position, targetPos);
                //Debug.Log("Current Distance To Target " + distance);
                if (distance < distanceToPickNexPathPoint)
                {
                    //Debug.Log("PICK NEXT CLOSEST POINT Current Distance To Target " + distance);
                    targetPos = currentPath.points[Mathf.Clamp(GetCurrentClosestPointOnPathIndex(currentPath) + 1, 0, currentPath.points.Count-1)];
                }
                
                Debug.DrawLine(transform.position + Vector3.up, targetPos, Color.red, 1);
                
                if (targetPos.y > transform.position.y)
                {
                    rb.AddForce(
                        ((targetPos - transform.position).normalized * _moveSpeed + Vector3.up * _vaultPower) *
                        Time.deltaTime, ForceMode.VelocityChange);
                    currentGravityForce = 1;
                }
                else
                {
                    rb.AddForce((targetPos - transform.position).normalized * _moveSpeed * Time.deltaTime,
                        ForceMode.VelocityChange);
                    currentGravityForce = 1;
                }
            }
            currentGravityForce = gravityForce;
        }

        int GetCurrentClosestPointOnPathIndex(DynamicPathfinder.Path path)
        {
            int closestIndex = 0;
            float distance = 10000;
            for (var index = 0; index < path.points.Count; index++)
            {
                var pathPoint = path.points[index];
                var newDistance = Vector3.Distance(pathPoint, transform.position);
                if (newDistance < distance)
                {
                    distance = newDistance;
                    closestIndex = index;
                }
            }

            return closestIndex;
        }
        
        void TeleportNearPosition(Vector3 pos)
        {
            rb.MovePosition(pos);
        }

        public void TeleportToRespawnPosition()
        {
            if (rememberRespawPoint)
                TeleportNearPosition(rememberedRespawnPoint);
        }
        
        
        private void OnDestroy()
        {
            if (_lookTransform != null)
                Destroy(_lookTransform.gameObject);
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