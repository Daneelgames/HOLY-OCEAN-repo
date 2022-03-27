using System.Collections;
using Brezg.Extensions.UniTaskExtensions;
using JetBrains.Annotations;
using MrPink.Health;
using MrPink.PlayerSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink.WeaponsSystem
{
    public class WeaponController : MonoBehaviour
    {
        public Transform shotHolder;
        
        public float cooldown = 1;

        [SerializeField, AssetsOnly, Required]
        private BaseAttackCollider _attackColliderPrefab;

        [SerializeField, ChildGameObjectsOnly, CanBeNull]
        private BaseWeaponAnimation _animation;

        public Quaternion InitLocalRotation { get; private set; }
    
        public bool OnCooldown { get; set; } = false;

    
        private void Awake()
        {
            InitLocalRotation = transform.localRotation;
        }
    
    
        [Button]
        public void Shot(HealthController ownerHc)
        {
            Shot(shotHolder.forward, ownerHc);
        }

    
        public void Shot(Vector3 direction, HealthController ownerHc)
        {
            var newProjectile = Instantiate(_attackColliderPrefab, shotHolder.position, Quaternion.LookRotation(direction));
            ScoringActionType action = ScoringActionType.NULL;
            if (ownerHc == Player.Health)
                action = Player.Movement.GetCurrentScoringAction();
        
            newProjectile.Init(ownerHc, action);
            StartCoroutine(Cooldown());
            
            if (_animation != null)
                _animation.Play().ForgetWithHandler();
        }
    

        private IEnumerator Cooldown()
        {
            OnCooldown = true;
            yield return new WaitForSeconds(cooldown);
            OnCooldown = false;
        }
    }
}