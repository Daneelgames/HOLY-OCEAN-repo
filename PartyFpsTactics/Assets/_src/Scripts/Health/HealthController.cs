using System;
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
        public Unit selfUnit;
        public int health = 100;
        public int healthMax = 100;
        public CharacterNeeds needs;
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

        [Header("AI")] // TODO здоровье не должно разруливать интеллект, подкрутить архитектуру
        
        public Team team;

        public UnitVision UnitVision;
        public UnitAiMovement AiMovement;
        public AiVehicleControls aiVehicleControls;
        public HumanVisualController HumanVisualController;

        [Header("Mis")] 
        public AiShop AiShop;
        public CrimeLevel crimeLevel;
        public ControlledMachine controlledMachine;
        public PlayerMovement playerMovement;
        public ExplosionController explosionOnDeath;
        public InteractiveObject npcInteraction;
        public DeathOnHit deathOnHit;
        public List<HealthController> unitsVisibleBy = new List<HealthController>();

        [Header("This RB will be affected by explosions. For barrels etc")]
        public Rigidbody rb;


        public List<BodyPart> bodyParts;
        public List<Transform> bodyPartsTransforms;

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

        public bool OwnCollider(Collider coll)
        {
            if (bodyPartsTransforms.Contains(coll.transform))
                return true;
            
            return false;
        }

        [ContextMenu("SetupBodyPartsTransforms")]
        public void SetupBodyPartsTransforms()
        {
            bodyPartsTransforms.Clear();
            for (int i = 0; i < bodyParts.Count; i++)
            {
                bodyPartsTransforms.Add(bodyParts[i].transform);
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


        public void AddHealth(int hpToRegen)
        {
            Debug.Log("AddHealth " + hpToRegen);
            health = Mathf.Clamp(health + hpToRegen, 0, healthMax);
            if (Game.Player.Health == this)
            {
                PlayerUi.Instance.UpdateHealthBar();
            }

            if (health <= 0)
                StartCoroutine(Death(ScoringActionType.NULL));
        }
        
        public void SetDamager(HealthController damager)
        {
            if (AiMovement == null)
                return;
            
            var vision = AiMovement.GetComponent<UnitVision>();
            
            if (vision)
            {
                if (team != damager.team)
                    Debug.Log("SetDamager other team: " + damager);
                vision.SetDamager(damager, true, true);
            }
            if (damager.crimeLevel)
                damager.crimeLevel.CrimeCommitedAgainstTeam(team, true, true);
        }

        public void DrainHealth(int drainAmount)
        {
            if (health <= 0)
                return;
            health -= drainAmount;
            
            if (Game.Player.Health == this)
            {
                PlayerUi.Instance.UpdateHealthBar();
            }

            if (health <= 0)
            {
                StartCoroutine(Death(ScoringActionType.NULL, null));
            }
        }
        
        public void Damage(int damage, DamageSource source, ScoringActionType action = ScoringActionType.NULL, Transform killer = null)
        {
            if (health <= 0)
                return;

            if (IsImmortal)
                damage = 0;
            
            
            if (Game.Player.Health == this && PlayerInventory.Instance.HasTool(ToolType.OneTimeShield))
            {
                PlayerUi.Instance.RemoveShieldFeedback();
                PlayerInventory.Instance.RemoveTool(ToolType.OneTimeShield);
            }
            else
                health -= damage;

            if (Game.Player.Health == this && PlayerUi.Instance != null)
                PlayerUi.Instance.UpdateHealthBar();

            if (health <= 0)
            {
                health = 0;
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
            
            if (Game.Player.Health == this)
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
                AiMovement.StopActivities();

            if (HumanVisualController)
                HumanVisualController.Death();
        
            if (explosionOnDeath)
            {
                var explosion = Instantiate(explosionOnDeath, visibilityTrigger.transform.position, transform.rotation);
                explosion.Init(action);
            }

            if (selfUnit)
                selfUnit.SpawnLootOnDeath.SpawnLoot();
        
            if (Game.Player.Health == this)
            {
                GameManager.Instance.SetPlayerSleepTimeScale(false);
                Game.Player.Death(killer);
            }

            if (npcInteraction)
            {
                PhoneDialogueEvents.Instance.NpcDied(this);
                Destroy(npcInteraction.gameObject);
            }
            
            OnDeathEvent.Invoke();

            yield return null;
            if (destroyOnDeath)
                Destroy(gameObject);
        }

        public void AddToVisibleByUnits(HealthController unit)
        {
            if (!unitsVisibleBy.Contains(unit))
                unitsVisibleBy.Add(unit);
        }
        public void RemoveFromVisibleByUnits(HealthController unit)
        {
            if (unitsVisibleBy.Contains(unit))
                unitsVisibleBy.Remove(unit);
        }
        
        private void OnDestroy()
        {
            // TODO инкапсулировать логику в сами классы
            
            if (UnitsManager.Instance.unitsInGame.Contains(this))
                UnitsManager.Instance.unitsInGame.Remove(this);
            if (Game.Player.CommanderControls.unitsInParty.Contains(this))
                Game.Player.CommanderControls.unitsInParty.Remove(this);
        }

        public void Resurrect()
        {
            AddHealth(healthMax/2);
            IsDead = false;
        }
    }
}