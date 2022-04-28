using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace MrPink.Units
{
    public class UnitFollowTarget : MonoBehaviour
    {
        [SerializeField, ChildGameObjectsOnly, Required]
        private NavMeshAgent _selfAgent;

        [SerializeField, ChildGameObjectsOnly, Required]
        private UnitMovement _selfMovement;
        
        private Vector3 _currentTargetPosition;
        private Coroutine _followCoroutine;


        public void FollowTarget(Transform target)
        {
            _followCoroutine = StartCoroutine(FollowTargetCoroutine(target));
        }

        public void StopFollowing()
        {
            if (_followCoroutine == null)
                return;
            
            StopCoroutine(_followCoroutine);
        }
        
        // TODO переписать на таске
        private IEnumerator FollowTargetCoroutine(Transform target)
        {
            while (_selfAgent && _selfAgent.enabled)
            {
                _currentTargetPosition = target.position;
                _selfMovement.AgentSetPath(_currentTargetPosition, true);
                //Debug.Log("FollowTargetCoroutine");
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(_currentTargetPosition , Vector3.one);
        }
        
        
#if UNITY_EDITOR

        public void TransferData()
        {
            _selfAgent = GetComponent<NavMeshAgent>();
            _selfMovement = GetComponent<UnitMovement>();
        }
        
#endif
    }
}