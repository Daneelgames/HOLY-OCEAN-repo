using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.Timeline;
using MrPink.PlayerSystem;
using Sirenix.OdinInspector;
using MrPink.Tools;
using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MrPink.Health
{
    public class HealthController : MonoBehaviour
    {
        public int health = 100;
        public int healthMax = 100;
        public bool destroyOnDeath = false;
        public Collider visibilityTrigger;
    
        [Header("Shake On Damage")]
        public bool proceduralDamageShake = false;
        public bool shakeX = true;
        public bool shakeY = false;
        public bool shakeZ = true;
        public float maxShakeOffset = 0.1f;
        public Transform transformToShake;
    
        [Header("AI")]
        public AiMovement AiMovement;
        public HumanVisualController HumanVisualController;

        [Header("Mis")] 
        public PlayerMovement playerMovement;
        public ExplosionController explosionOnDeath;
        public List<GameObject> objectsToSpawnOnDeath;
        public DeathOnHit deathOnHit;

        [Header("This RB will be affected by explosions")]
        public Rigidbody rb;

        public Team team;

        public List<BodyPart> bodyParts;


        public List<DamageState> damageStates;

        [ShowInInspector, ReadOnly] 
        public bool IsImmortal { get; set; } = false;

        [ShowInInspector, ReadOnly] 
        public bool IsDead { get; private set; } = false;

        private void Start()
        {
            healthMax = health;
            UnitsManager.Instance.unitsInGame.Add(this);
        }
        
        #if UNITY_EDITOR

        [ContextMenu("Link Body Parts")]
        private void LinkBodyParts()
        {
            ConvertDeprecatedTileHealth();
            SetupBodyParts();
            AssetDatabase.SaveAssets();
        }

        private void ConvertDeprecatedTileHealth()
        {
            foreach (var deprecated in transform.GetComponentsInChildren<TileHealth>())
            {
                var obj = deprecated.gameObject;
                DestroyImmediate(deprecated);
                var bodyPart = obj.AddComponent<BodyPart>();
                EditorUtility.SetDirty(bodyPart);
            }
        }
        
        private void SetupBodyParts()
        {
            bodyParts = new List<BodyPart>();
            var parts = transform.GetComponentsInChildren<BodyPart>();
            for (int i = 0; i < parts.Length; i++)
            {
                bodyParts.Add(parts[i]);
                parts[i].HealthController = this;
                
                EditorUtility.SetDirty(parts[i]);
            }
            EditorUtility.SetDirty(this);
        }
        
        #endif

        public void Damage(int damage, DamageSource source, ScoringActionType action = ScoringActionType.NULL, Transform killer = null)
        {
            if (health <= 0)
                return;

            if (IsImmortal)
                damage = 0;
            
            
            if (Player.Health == this && PlayerInventory.Instance.HasTool(ToolType.OneTimeShield))
            {
                PlayerUi.Instance.RemoveShieldFeedback();
                PlayerInventory.Instance.RemoveTool(ToolType.OneTimeShield);
            }
            else
                health -= damage;

            if (Player.Health == this)
            {
                PlayerUi.Instance.UpdateHealthBar();
            }

            if (health <= 0)
            {
                StartCoroutine(Death(action, killer));
            
                if (source == DamageSource.Player && action != ScoringActionType.NULL)
                    ScoringSystem.Instance.RegisterAction(action);
                
                return;
            }
        
            if (deathOnHit)
                deathOnHit.Hit(this);
        
            if (proceduralDamageShake)
                StartCoroutine(DamageShake());
            SetDamageState();
        }

        private IEnumerator DamageShake()
        {
            float t = 0f;
            var originalPos = transformToShake.localPosition;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
            
                float x = 0;
                if (shakeX)
                    x = Random.Range(-maxShakeOffset, maxShakeOffset);
            
                float y = Random.Range(-maxShakeOffset, maxShakeOffset);
            
                float z = Random.Range(-maxShakeOffset, maxShakeOffset);
                
                transformToShake.localPosition = originalPos + new Vector3(x,y,z);
                yield return null;
            }
        }

        private void SetDamageState()
        {
            float currentHealthPercentage = health / healthMax;
            for (int i = 0; i < damageStates.Count; i++)
            {
                if (damageStates[i].healthPercentage > currentHealthPercentage)
                {
                    damageStates[i].visual.SetActive(false);
                }
                else
                {
                    damageStates[i].visual.SetActive(true);
                    break;
                }
            }
        }

        private IEnumerator Death(ScoringActionType action, Transform killer = null)
        {
            IsDead = true;
            
            if (AiMovement)
                AiMovement.Death();

            if (HumanVisualController)
                HumanVisualController.Death();
        
            if (explosionOnDeath)
            {
                var explosion = Instantiate(explosionOnDeath, visibilityTrigger.transform.position, transform.rotation);
                explosion.Init(action);
            }
        
            for (int i = 0; i < objectsToSpawnOnDeath.Count; i++)
            {
                Instantiate(objectsToSpawnOnDeath[i], visibilityTrigger.transform.position, transform.rotation);
                yield return null;
            }
        
            if (Player.Health == this)
                Player.Death(killer);

            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            // TODO инкапсулировать логику в сами классы
            
            if (UnitsManager.Instance.unitsInGame.Contains(this))
                UnitsManager.Instance.unitsInGame.Remove(this);
            if (Player.CommanderControls.unitsInParty.Contains(this))
                Player.CommanderControls.unitsInParty.Remove(this);
        }
    }
}