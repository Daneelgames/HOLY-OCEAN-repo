using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink.WeaponsSystem
{
    public class SimpleAnimation : BaseWeaponAnimation
    {
        [SerializeField, ChildGameObjectsOnly, Required]
        private Animator _animator;

        [SerializeField, Required] 
        private string _triggerName = "Play";

        private bool _isAnimationFinished = false;
        
        [Button, ShowIf("@UnityEngine.Application.isPlaying")]
        public override async UniTask Play()
        {
            _isAnimationFinished = false;
            
            _animator.SetTrigger(_triggerName);

            await UniTask.WaitUntil(() => _isAnimationFinished);
        }

        public void _FinishAnimation()
            => _isAnimationFinished = true;
    }
}