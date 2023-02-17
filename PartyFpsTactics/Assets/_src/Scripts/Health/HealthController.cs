using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Object.Synchronizing;
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
    public class HealthController : NetworkBehaviour
    {
        [SerializeField] [ReadOnly] [SyncVar] private bool isPlayer = false;
        public bool IsPlayer => isPlayer;

        public void SetIsPlayerTrue()
        {
            isPlayer = true;
        }

        [Header("USE FOR BOSSES")][SerializeField] private bool completeGameLevelOnDeath = false;

        public Unit selfUnit;
        [SyncVar] public int health = 100;
        public int healthMax = 100;
        public float GetHealthFill => (float)health / healthMax;
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
        public ControlledMachine GetControlledMachine => controlledMachine;
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
        [SyncVar] public bool IsDead = false;
        //public bool IsDead { get; private set; } = false;

        public UnityEvent OnDeathEvent = new UnityEvent();
        public UnityEvent OnDamagedEvent = new UnityEvent();
        
        private IEnumerator Start()
        {
            while (Game._instance == null || Game.LocalPlayer == null)
            {
                yield return null;
            }
            if (Game.LocalPlayer.Health != this)
                UnitsManager.Instance.AddUnit(this);
            
            healthMax = health;
            enduranceMax = endurance;
        }
        
        public bool OwnCollider(Collider coll)
        {
            if (bodyPartsTransforms.Contains(coll.transform))
                return true;
            
            return false;
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
            if (hpToRegen <= 0) return;
            
            //Debug.Log("AddHealth " + hpToRegen);
            health = Mathf.Clamp(health + hpToRegen, 0, healthMax);
            if (Game.LocalPlayer.Health == this)
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
            if (IsImmortal)
                drainAmount = 0;
            
            if (health <= 0)
                return;
            health -= drainAmount;
            
            if (Game.LocalPlayer.Health == this)
            {
                PlayerUi.Instance.UpdateHealthBar();
            }

            if (health <= 0)
            {
                StartCoroutine(Death(ScoringActionType.NULL, null));
            }
        }

        public void Kill()
        {
            Damage(healthMax, DamageSource.Environment);
        }
        
        public void Damage(int damage, DamageSource source, ScoringActionType action = ScoringActionType.NULL, Transform killer = null)
        {
            if (health <= 0)
                return;

            if (IsImmortal || Game._instance.IsLevelGenerating)
                damage = 0;
            
            if (Shop.Instance.IsActive)
                return;
                
            if (Game.LocalPlayer.Health == this && Game.LocalPlayer.Inventory.HasTool(ToolType.OneTimeShield))
            {
                PlayerUi.Instance.RemoveShieldFeedback();
                Game.LocalPlayer.Inventory.RemoveTool(ToolType.OneTimeShield);
            }
            else
                health -= damage;

            
            OnDamagedEvent?.Invoke();
            
            if (Game.LocalPlayer.Health == this)
            {
                PlayerUi.Instance.UpdateHealthBar();
            }
            
            if (controlledMachine && controlledMachine.controllingHc)
                controlledMachine.controllingHc.Damage(Mathf.RoundToInt(damage * controlledMachine.DamageToControllingHcScaler), DamageSource.Environment);
                

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
        
            if (proceduralDamageShake && transformToShake)
                StartCoroutine(DamageShake());
            SetDamageState();
        }
        
        

        private IEnumerator DamageShake()
        {
            float t = 0f;
            var originalPos = transformToShake.localPosition;
            
            if (Game.LocalPlayer.Health == this)
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
            if (IsDead) 
                yield break;
            
            if (IsServer)
            {
                Debug.Log("DEATH on server start " + gameObject.name);
                DeathOnServer(action);
            }
            else
            {
                Debug.Log("DEATH on client start " + gameObject.name);
                RpcDeathOnServer(action);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        void RpcDeathOnServer(ScoringActionType action)
        {
            Debug.Log("DEATH RpcDeathOnServer " + gameObject.name);
            //DeathOnClient(action);
            DeathOnServer(action);
        }
        
        [Server]
        void DeathOnServer(ScoringActionType action)
        {
            RpcDeathOnClient(action);
            
            if (isPlayer == false)
                StartCoroutine(DestroyOnServer());
        }

        [Server]
        IEnumerator DestroyOnServer()
        {
            yield return new WaitForSeconds(10);
            ServerManager.Despawn(gameObject, DespawnType.Destroy);
        }

        
        [ObserversRpc(/*IncludeOwner = false*/)]
        void RpcDeathOnClient(ScoringActionType action)
        {
            //Debug.Log("DEATH RpcDeathOnClient " + gameObject.name);
            DeathOnClient(action);
        }

        void DeathOnClient(ScoringActionType action)
        {
            bool isLocalPlayer = Game.LocalPlayer.Health == this;
            health = 0;
            Debug.Log("DEATH DeathOnClient " + gameObject.name + "; isLocalPlayer + " + isLocalPlayer);
            IsDead = true;
            if (AiMovement)
                AiMovement.StopActivities();

            if (selfUnit)
                selfUnit.Death();
            
            if (HumanVisualController && HumanVisualController.gameObject.activeInHierarchy)
                HumanVisualController.Death();
        
            if (explosionOnDeath)
            {
                var explosion = Instantiate(explosionOnDeath, visibilityTrigger.transform.position, transform.rotation);
                explosion.Init(action);
            }

            if (selfUnit)
                selfUnit.SpawnLootOnDeath.SpawnLoot();


            if (isLocalPlayer)
            {
                GameManager.Instance.SetPlayerSleepTimeScale(false);
                Game.LocalPlayer.Death(null);
            }

            if (npcInteraction)
            {
                PhoneDialogueEvents.Instance.NpcDied(this);
                Destroy(npcInteraction.gameObject);
            }
            
            OnDeathEvent?.Invoke();

            UnitsManager.Instance.RemoveUnit(this);
            
            if (completeGameLevelOnDeath)
                ProgressionManager.Instance.LevelCompleted();
            
            if (destroyOnDeath)
            {
                OnDamagedEvent?.RemoveAllListeners();
                OnDeathEvent?.RemoveAllListeners();
                Destroy(gameObject);
            }
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
            
            UnitsManager.Instance.RemoveUnit(this);
            if (Game.LocalPlayer.CommanderControls.unitsInParty.Contains(this))
                Game.LocalPlayer.CommanderControls.unitsInParty.Remove(this);
        }

        public void Resurrect(bool fullHeal = true)
        {
            AddHealth(fullHeal ? healthMax : healthMax / 10);
            IsDead = false;
        }
    }
}