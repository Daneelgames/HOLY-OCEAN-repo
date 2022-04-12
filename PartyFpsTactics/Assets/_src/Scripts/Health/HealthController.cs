using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.Timeline;
using MrPink.PlayerSystem;
using Sirenix.OdinInspector;
using MrPink.Tools;
using MrPink.Units;
using UnityEngine;
using UnityEngine.Events;
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
        
        public float endurance = 100;
        float enduranceMax = 100;
        public float enduranceRegenSpeed = 100;
        public float UnitRagdollStandupCooldown = 1;
        
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
        public Team team;

        public UnitAi AiMovement;
        public HumanVisualController HumanVisualController;

        [Header("Mis")] 
        public PlayerMovement playerMovement;
        public ExplosionController explosionOnDeath;
        public List<GameObject> objectsToSpawnOnDeath;
        public InteractiveObject npcInteraction;
        public DeathOnHit deathOnHit;

        [Header("This RB will be affected by explosions. For barrels etc")]
        public Rigidbody rb;


        public List<BodyPart> bodyParts;

        public List<DamageState> damageStates;

        [ShowInInspector, ReadOnly] 
        public bool IsImmortal { get; set; } = false;

        [ShowInInspector, ReadOnly] 
        public bool IsDead { get; private set; } = false;

        public UnityEvent OnDeathEvent = new UnityEvent();
        
        private void Start()
        {
            healthMax = health;
            enduranceMax = endurance;
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

        public float DamageEndurance(int dmg)
        {
            endurance = Mathf.Clamp(endurance - dmg, 0, enduranceMax);
            
            if (EnduranceRegenCoroutine != null)
                StopCoroutine(EnduranceRegenCoroutine);
            EnduranceRegenCoroutine = StartCoroutine(EnduranceRegen());

            return endurance;
        }

        private Coroutine EnduranceRegenCoroutine;
        IEnumerator EnduranceRegen()
        {
            while (endurance < enduranceMax)
            {
                yield return null;

                endurance += enduranceRegenSpeed * Time.deltaTime;
                endurance = Mathf.Clamp(endurance, 0, enduranceMax);
            }
        }

        public void RestoreEndurance()
        {
            endurance = enduranceMax;
        }
        
        public void SetDamager(HealthController damager)
        {
            if (AiMovement && AiMovement.unitVision)
            {
                AiMovement.unitVision.SetDamager(damager);
            }
        }

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
            
            if (Player.Health == this)
                originalPos = Vector3.up;
            
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

            transformToShake.localPosition = originalPos;
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
            {
                Player.Interactor.SetInteractionText("R TO RESTART");
                Player.Death(killer);
            }

            if (npcInteraction)
            {
                PhoneDialogueEvents.Instance.NpcDied(this);
                Destroy(npcInteraction.gameObject);
            }
            
            OnDeathEvent.Invoke();
            
            if (destroyOnDeath)
                Destroy(gameObject);
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