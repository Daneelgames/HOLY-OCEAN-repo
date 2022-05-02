using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace MrPink.Units
{
    public class UnitMovement : MonoBehaviour
    {
        // TODO вынести конфиги передвижения в SO
        
        [SerializeField]
        [Range(1, 100)]
        public float _moveSpeed = 2;
        
        [SerializeField]
        [Range(1,10)]
        private float _turnSpeed = 4;
        
        [Range(1,100)]
        private float _runSpeed = 4;

        [SerializeField]
        private float _stopDistanceFollow = 1.5f;
        
        [SerializeField]
        private float _stopDistanceMove = 0;
        
        [SerializeField, ChildGameObjectsOnly, Required]
        private NavMeshAgent _agent;

        [SerializeField, ChildGameObjectsOnly] private Rigidbody rb;
        
        [SerializeField, ChildGameObjectsOnly, Required]
        private Unit _selfUnit;

        
        private Vector3 _currentVelocity;
        private Transform _lookTransform;
        
        public bool rememberRespawPoint = false;
        private Vector3 rememberedRespawnPoint;

        private void Start()
        {
            // TODO не делать этого в старте
            _lookTransform = new GameObject(gameObject.name + "LookTransform").transform;
            _lookTransform.parent = transform.parent;
            
            if (rememberRespawPoint)
                rememberedRespawnPoint = transform.position;
        }

        private void Update()
        {
            _currentVelocity = _agent.velocity;
            _selfUnit.HumanVisualController.SetMovementVelocity(_currentVelocity);
            _lookTransform.transform.position = transform.position;
        }

        public void Death()
        {
            _agent.enabled = false;
            //this.enabled = false;
        }

        public void Resurrect()
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5.0f, NavMesh.AllAreas))
                transform.position = hit.position;
            
            _agent.enabled = true;
            this.enabled = true;
        }

        public void Run()
        {
            _agent.speed = _runSpeed;
        }
        
        public void LookAt(Vector3 targetPosition)
        {
            _lookTransform.LookAt(targetPosition, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, _lookTransform.rotation, Time.deltaTime * _turnSpeed);  
        }
        
        public IEnumerator MoveToPosition(Vector3 target)
        {
            AgentSetPath(target, false);
        
            while (Vector3.Distance(transform.position, target) > 1)
                yield return new WaitForSeconds(0.5f);
        }
        
        public void AgentSetPath(Vector3 target, bool isFollowing, bool cheap = true)
        {
            if (enabled == false)
                return;
            
            if (moveRigidbodyCoroutine != null)
                StopCoroutine(moveRigidbodyCoroutine);
            moveRigidbodyCoroutine = StartCoroutine(MoveRigidbody(target));
            // NO NAV MESH
            return;
            
            if (_agent.enabled == false)
                return;
            
            _agent.speed = _moveSpeed;
            _agent.stoppingDistance = isFollowing ? _stopDistanceFollow : _stopDistanceMove;
            Vector3 targetPos = SamplePos(target);
            if (cheap)
            {
                _agent.SetDestination(targetPos);
                return;
            }
            
            var path = new NavMeshPath();
            transform.position = SamplePos(transform.position);
            NavMesh.CalculatePath(transform.position, targetPos, NavMesh.AllAreas, path);
            
            _agent.SetPath(path);
        }

        private Coroutine moveRigidbodyCoroutine;
        IEnumerator MoveRigidbody(Vector3 targetPos)
        {
            while (true)
            {
                yield return new WaitForSeconds(0.1f);
                
                if (Vector3.Distance(transform.position, targetPos) < _stopDistanceMove)
                    continue;
                
                rb.AddForce((targetPos - transform.position).normalized * _moveSpeed * Time.deltaTime, ForceMode.VelocityChange);
                
            }
        }
        
        public void TeleportNearPosition(Vector3 pos)
        {
            if (_agent.enabled)
            {
                _agent.Warp(SamplePos(pos));
                return;
            }
            
            // if not enabled
            transform.position = SamplePos(pos);
        }

        public void TeleportToRespawnPosition()
        {
            if (rememberRespawPoint)
                TeleportNearPosition(rememberedRespawnPoint);
        }
        
        private Vector3 SamplePos(Vector3 startPos)
        {
            if (NavMesh.SamplePosition(startPos, out var hit, 10f, NavMesh.AllAreas))
                startPos = hit.position;

            return startPos;
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