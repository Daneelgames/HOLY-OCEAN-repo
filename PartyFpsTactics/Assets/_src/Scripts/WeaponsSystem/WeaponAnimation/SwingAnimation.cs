using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink.WeaponsSystem
{
    public class SwingAnimation : BaseWeaponAnimation
    {
        [SerializeField, ChildGameObjectsOnly, Required]
        private Transform _model;
        
        // TODO cделать тулзу, чтобы редактировать не трансформы, а вектора
        [SerializeField, ChildGameObjectsOnly, Required]
        private List<Transform> _trajectory = new List<Transform>();
        
        [SerializeField]
        private int _trajectoryMovementTime = 300;

        private Vector3 _initialPosition;

        private void Awake()
        {
            _initialPosition = transform.position;
        }

        [Button, ShowIf("@UnityEngine.Application.isPlaying")]
        public override async UniTask Play()
        {
            var tweenTime = _trajectoryMovementTime / 1000f;

            Sequence sequence = DOTween.Sequence();
            
            foreach (var point in _trajectory)
            {
                var moveTween = DOTween.To(
                    () => _model.transform.position,
                    value => _model.transform.position = value,
                    point.position,
                    tweenTime);

                sequence.Append(moveTween);
            }
            
            var finishTween = DOTween.To(
                () => _model.transform.position,
                value => _model.transform.position = value,
                _initialPosition,
                tweenTime);

            sequence.Append(finishTween);

            sequence.Play();
            
            await UniTask.Delay(_trajectoryMovementTime * (_trajectory.Count + 1), DelayType.Realtime);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;

            for (var i = 0; i < _trajectory.Count; i++)
            {
                Gizmos.DrawSphere(_trajectory[i].position, 0.5f);
                
                if (i+1 < _trajectory.Count)
                    Gizmos.DrawLine(_trajectory[i].position, _trajectory[i+1].position);
            }
        }
    }
}