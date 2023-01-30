using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Cysharp.Threading.Tasks;
using FishNet.Object;
using MrPink.Health;
using MrPink.PlayerSystem;
using Sirenix.OdinInspector;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace MrPink.Units
{
    public class UnitsManager : MonoBehaviour
    {
        public static UnitsManager Instance;
         List<HealthController> hcInGame = new List<HealthController>();
        public List<HealthController> HcInGame => hcInGame;
        [Header("STREAMING")] 
        public float streamingDistance = 200;
        public float streamingDistanceMin = 20;
        public int maxUnitsToShow = 30;
        int currentShowAmount = 0;
        
        [Space]
        
        public int defaultInduranceDamage = 100;
    
        public float tileExplosionDistance = 3;
        public float tileExplosionForce = 100;
        public float tileExplosionForceBarrels = 50;
        public float tileExplosionForcePlayer = 100;

        public PhysicMaterial corpsesMaterial;
        
        [BoxGroup("UNITS PREFABS")] public List<HealthController> bossUnitPrefabs;
        [BoxGroup("UNITS PREFABS")] public List<HealthController> redTeamUnitPrefabs;
        [BoxGroup("UNITS PREFABS")] public List<HealthController> blueTeamUnitPrefabs;
        [BoxGroup("UNITS PREFABS")] public List<HealthController> neutralUnitPrefabs;
        [BoxGroup("UNITS PREFABS")] public List<HealthController> desertBeastsPrefabs;
    
        // TODO use real queue
        private readonly List<BasicHealth> _bodyPartsQueueToKill = new List<BasicHealth>();
        private readonly List<BasicHealth> _bodyPartsQueueToKillCombo = new List<BasicHealth>();

        private Transform _spawnRoot;
        public Transform SpawnRoot => _spawnRoot;

        public HealthController GetRandomRedUnit => redTeamUnitPrefabs[Random.Range(0, redTeamUnitPrefabs.Count)];
        public HealthController GetRandomBossUnit => bossUnitPrefabs[Random.Range(0, bossUnitPrefabs.Count)];
    
        private void Awake()
        {
            Instance = this;

            _spawnRoot = new GameObject("Unit Spawn Root").transform;
        }

        private void Start()
        {
            StartCoroutine(UniTask.ToCoroutine(BodyPartsKillQueue));
            StartCoroutine(StreamUnits());
        }

        public void AddUnit(HealthController hc)
        {
            hcInGame.Add(hc);
        }
        public void RemoveUnit(HealthController hc)
        {
            if (hcInGame.Contains(hc))
                hcInGame.Remove(hc);
        }

        IEnumerator StreamUnits()
        {
            while (true)
            {
                yield return null;
                
                if (hcInGame.Count <= 0)
                    continue;
                
                for (int i = hcInGame.Count - 1; i >= 0; i--)
                {
                    yield return null;

                    if (hcInGame.Count <= i)
                        continue;

                    var unit = hcInGame[i];
                    ShowUnit(unit, true);
                }
            }
        }


        public void ShowUnit(HealthController hc, bool show)
        {
            if (show == false) return;
            
            /*
            if (show)
                currentShowAmount++;
            else
            {
                if (PhoneDialogueEvents.Instance.currentTalknigNpc == hc)
                    return;
                currentShowAmount--;
            }*/
            
            if (hc.health <= 0)
            {
                hcInGame.Remove(hc);
                //Destroy(hc.gameObject);
                return;
            }
            //hc.gameObject.SetActive(show);
        }
        
        
        public Vector3 SamplePos(Vector3 pos)
        {
            return pos;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(pos, out hit, 10, NavMesh.AllAreas))
            {
                pos = hit.position;
            }

            return pos;
        }

        
        public void RagdollTileExplosion(Vector3 explosionPosition, float distance = -1, float force = -1,
            float playerForce = -1, ScoringActionType action = ScoringActionType.NULL, int enduranceDamage = -1)
        {
            NoiseSystem.Instance.DefaultNoise(explosionPosition);
            //Debug.Log("RAGDOLL TILE EXPLOSION; pos " + explosionPosition);
            if (distance < 0)
                distance = tileExplosionDistance;

            if (force < 0)
                force = tileExplosionForce;

            if (playerForce < 0)
                playerForce = tileExplosionForcePlayer;

            if (enduranceDamage < 0)
                enduranceDamage = defaultInduranceDamage;
            
            // BUMP ENEMIES
            for (int i = 0; i < hcInGame.Count; i++)
            {
                if (i >= hcInGame.Count || !hcInGame[i].gameObject.activeInHierarchy)
                    continue;

                if (!(Vector3.Distance(explosionPosition, hcInGame[i].transform.position + Vector3.up) <= distance)) continue;
                
                if (hcInGame[i].playerMovement)
                {
                    continue;
                    hcInGame[i].playerMovement.rb.AddForce((hcInGame[i].visibilityTrigger.transform.position - explosionPosition).normalized *
                                                              playerForce, ForceMode.VelocityChange);
                    continue;
                }

                if (hcInGame[i].rb) // BARRELS
                {
                    hcInGame[i].rb.AddForce((hcInGame[i].visibilityTrigger.transform.position - explosionPosition).normalized *
                                               tileExplosionForceBarrels, ForceMode.VelocityChange);

                    hcInGame[i].Damage(1, DamageSource.Player);
                    if (action != ScoringActionType.NULL)
                        ScoringSystem.Instance.RegisterAction(ScoringActionType.BarrelBumped, 3);

                    continue;
                }

                if (hcInGame[i].DamageEndurance(enduranceDamage) <= 0)
                {
                    if (hcInGame[i].HumanVisualController)
                    {
                        if (hcInGame[i].health > 0 && action != ScoringActionType.NULL)
                            ScoringSystem.Instance.RegisterAction(ScoringActionType.EnemyBumped, 2);
                        hcInGame[i].HumanVisualController.ActivateRagdoll();
                        hcInGame[i].HumanVisualController.ExplosionRagdoll(explosionPosition, force, distance);
                    }

                    if (hcInGame[i].AiMovement)
                        hcInGame[i].AiMovement.StopActivities();

                }
            }

            // BUMP PROPS
            bool propBumped = false;
            
            if (BuildingGenerator.Instance != null)
                for (int i = 0; i < BuildingGenerator.Instance.spawnedProps.Count; i++)
                {
                    if (Vector3.Distance(BuildingGenerator.Instance.spawnedProps[i].transform.position, explosionPosition) >
                        distance)
                        continue;

                    var rb = BuildingGenerator.Instance.spawnedProps[i].Rigidbody;

                    if (!rb) 
                        continue;
                
                    propBumped = true;
                    rb.AddExplosionForce(tileExplosionForceBarrels * 30, explosionPosition, distance);
                    BuildingGenerator.Instance.spawnedProps[i].tileAttack.dangerous = true;
                }

            if (propBumped && action != ScoringActionType.NULL)
                ScoringSystem.Instance.RegisterAction(ScoringActionType.PropBumped, 2);

        }

        public void AddHealthEntityToQueue(BasicHealth part, ScoringActionType action)
        {
            if (action != ScoringActionType.NULL)
                _bodyPartsQueueToKillCombo.Add(part);
            else
                _bodyPartsQueueToKill.Add(part);
        }

        public void MoveUnitsToRespawnPoints(bool destroyDead, bool healAlive)
        {
            for (int i = 0; i < hcInGame.Count; i++)
            {
                var unit = hcInGame[i];
                if (!unit)
                    continue;

                if (unit.health <= 0)
                {
                 if (destroyDead)
                     Destroy(unit.gameObject);
                
                 continue;
                }
                
                if (unit.health > 0)
                {
                    if (unit.health > 0 && unit.selfUnit && unit.selfUnit.UnitMovement)
                        unit.selfUnit.UnitMovement.TeleportToRespawnPosition();
                    
                    if (healAlive)
                        unit.AddHealth(unit.healthMax);
                }

            }
        }
        
        private async UniTask BodyPartsKillQueue()
        {
            int handledInFrame = 0;
            while (true)
            {
                handledInFrame = await HandleKillQueue(_bodyPartsQueueToKillCombo, DamageSource.Player, handledInFrame);
                await HandleKillQueue(_bodyPartsQueueToKill, DamageSource.Environment, handledInFrame);
            
                await UniTask.DelayFrame(1);
                handledInFrame = 0;
            }
        }

        private static async UniTask<int> HandleKillQueue(List<BasicHealth> healthToKillQueue, DamageSource damageSource, int handledInFrame)
        {
            if (healthToKillQueue.Count <= 0)
                return handledInFrame;

            for (int i = healthToKillQueue.Count - 1; i >= 0; i--)
            {
                if (healthToKillQueue[i] != null)
                    healthToKillQueue[i].Kill(damageSource);
                healthToKillQueue.RemoveAt(i);
                
                handledInFrame++;
                if (handledInFrame > 3)
                {
                    handledInFrame = 0;
                    await UniTask.DelayFrame(1);
                }
            }

            return handledInFrame;
        }
    
    }
}