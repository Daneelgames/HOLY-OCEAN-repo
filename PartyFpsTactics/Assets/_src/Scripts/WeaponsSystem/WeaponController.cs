using Brezg.Extensions.UniTaskExtensions;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using MrPink.Health;
using MrPink.PlayerSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace MrPink.WeaponsSystem
{
    public class WeaponController : MonoBehaviour
    {
        public Transform shotHolder;

        [SerializeField] 
        private bool _isMelee;

        [Header("Attacking")]
        public float cooldown = 1;
        
        [SerializeField]
        [FormerlySerializedAs("delay")]
        private float _delay = 0f;
        
        
        [SerializeField]
        [FormerlySerializedAs("attackSignalAu")]
        private AudioSource _attackSignalAudioSource;

        [SerializeField]
        int bulletsPerShot = 1;

        [Tooltip("Projectile prefab")]
        [SerializeField, AssetsOnly, Required]
        private Pooling.AttackColliderPool.AttackColliderPrefabTag _attackColliderTag;

        [SerializeField, ChildGameObjectsOnly, CanBeNull]
        private BaseWeaponAnimation _animation;

        public float projectileRandomRotationMax = 0;
        
        [Header("Only for Player Weapon")]
        
        public float attackStaminaCost = 0;
        public AudioSource attackAu;
        public AudioSource reloadingAu;
        public AudioClip reloadingClip;
        public AudioClip reloadingEndClip;
        public float gunsMoveDistanceScaler = 0.2f;
        
        [Header("Player Weapon Movement")] 
        [Range(0.1f,100)]
        public float gunMoveSpeedScaler = 5;
        [Range(0.1f,100)]
        public float gunRotSpeedScaler = 5;
        [Range(0.1f,500)]
        public float gunMoveSpeed = 100;
        [Range(0.1f,500)]
        public float gunRotationSpeed = 100;
        [Range(0,1)]
        public float WeaponRotationZScalerThreshold = 0.2f;
        [Range(0,1000)]
        public float WeaponRotationZScaler = 3f;

        private HealthController _ownerHc;
        public Quaternion InitLocalRotation { get; private set; }
    
        public bool OnCooldown { get; set; } = false;


        public bool IsMelee
            => _isMelee;

    
        private void Awake()
        {
            InitLocalRotation = transform.localRotation;
        }
    
    
        //[Button]
        // ReSharper disable Unity.PerformanceAnalysis
        public void Shot(HealthController ownerHc, Transform aiAimTransform = null)
        {
            ShotAsync(shotHolder.forward, ownerHc, aiAimTransform);
        }

        void SetOwnHc(HealthController hc)
        {
            _ownerHc = hc;
        }
    
        async void ShotAsync(Vector3 direction, HealthController ownerHc, Transform aiAimTransform = null)
        {
            if (_ownerHc == null)
                SetOwnHc(ownerHc);
            
            OnCooldown = true;
            if (_attackSignalAudioSource != null)
            {
                _attackSignalAudioSource.pitch = Random.Range(0.75f, 1.25f);
                _attackSignalAudioSource.Play();
            }

            await UniTask.Delay((int)(_delay * 1000));
            
            if (attackAu)
            {
                attackAu.pitch = Random.Range(0.75f, 1.25f);
                attackAu.Play();
            }
            
            if (_ownerHc.health <= 0)
                return;
            
            // AI is aiming
            if (aiAimTransform != null)
            {
                transform.LookAt(aiAimTransform.position);
                direction = (aiAimTransform.position - shotHolder.position).normalized;
            }
            
            bool isPlayer = _ownerHc == Game.LocalPlayer.Health;   

            SpawnProjectileInDirection(direction, isPlayer, _ownerHc);

            if (isPlayer)
                Game.LocalPlayer.Movement.ChangeStamina(-attackStaminaCost);

            Cooldown().ForgetWithHandler();
            
            if (_animation != null)
                _animation.Play().ForgetWithHandler();
        }

        void SpawnProjectileInDirection(Vector3 direction, bool isPlayer, HealthController ownerHc)
        {
            //ScoringActionType action = isPlayer ? GetPlayerScoringAction() : ScoringActionType.NULL;
            DamageSource source = isPlayer ? DamageSource.Player : DamageSource.Enemy;

            for (int i = 0; i < bulletsPerShot; i++)
            {
                float offsetX = Random.Range(0, projectileRandomRotationMax);
                float offsetY = Random.Range(0, projectileRandomRotationMax);
                
                NetworkProjectileSpawner.Instance.SpawnProjectileOnEveryClient(_attackColliderTag, shotHolder, direction, _ownerHc, source, offsetX, offsetY);
            }
        }

    
        private async UniTask Cooldown()
        {
            OnCooldown = true;
            
            if (reloadingAu)
            {
                reloadingAu.clip = reloadingClip;
                reloadingAu.loop = true;
                reloadingAu.pitch = Random.Range(0.8f, 1.1f);
                reloadingAu.Play();
            }
            await UniTask.Delay((int) (cooldown * 1000));
            OnCooldown = false;
            
            if (reloadingAu)
            {
                reloadingAu.clip = reloadingEndClip;
                reloadingAu.loop = false;
                reloadingAu.pitch = Random.Range(0.8f, 1.1f);
                reloadingAu.Play();
            }
        }
        
        private static ScoringActionType GetPlayerScoringAction()
        {
            var state = Game.LocalPlayer.Movement.State;
            
            if (state.IsLeaning)
            {
                if (!state.IsGrounded)
                    return ScoringActionType.KillLeaningRangedOnJump;
                
                if (state.IsRunning)
                    return ScoringActionType.KillLeaningRangedOnRun;
                
                if (state.IsMoving)
                    return ScoringActionType.KillLeaningRangedOnMove;
                
                return ScoringActionType.KillLeaningRangedIdle;
            }
            
            if (!state.IsGrounded)
                return ScoringActionType.KillRangedOnJump;
            
            if (state.IsRunning)
                return ScoringActionType.KillRangedOnRun;
            
            if (state.IsMoving)
                return ScoringActionType.KillRangedOnMove;
            
            return ScoringActionType.KillRangedIdle;
        }
    }
}