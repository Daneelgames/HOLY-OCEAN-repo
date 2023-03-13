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
         List<HealthController> mobsInGame = new List<HealthController>();
         List<HealthController> bossesInGame = new List<HealthController>();
        public List<HealthController> MobsInGame => mobsInGame;
        public List<HealthController> BossesInGame => bossesInGame;
        public float destroyUnitsDistance = 500;
        
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

        public void AddMob(HealthController hc)
        {
            mobsInGame.Add(hc);
        }
        public void AddBoss(HealthController hc)
        {
            bossesInGame.Add(hc);
        }
        public void RemoveUnit(HealthController hc)
        {
            if (mobsInGame.Contains(hc))
                mobsInGame.Remove(hc);
            if (bossesInGame.Contains(hc))
                bossesInGame.Remove(hc);
        }

        IEnumerator StreamUnits()
        {
            while (true)
            {
                yield return null;
                
                if (mobsInGame.Count <= 0)
                    continue;
                
                for (int i = mobsInGame.Count - 1; i >= 0; i--)
                {
                    yield return new WaitForSeconds(0.1f);

                    if (mobsInGame.Count <= i)
                        continue;

                    var unit = mobsInGame[i];   
                    if (unit.selfUnit == null || unit.selfUnit.DestroyOnDistance == false)
                        continue;
                    if (unit.health < 1)
                        continue;
                    var distance = Game._instance.DistanceToClosestPlayer(unit.transform.position);
                    if (distance.distance > destroyUnitsDistance)
                        unit.DestroyOnDistance();
                }
            }
        }


        public void ShowUnit(HealthController hc, bool show)
        {
            if (show == false) return;
            
            if (hc.health <= 0)
            {
                mobsInGame.Remove(hc);
            }
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
            for (int i = 0; i < mobsInGame.Count; i++)
            {
                if (i >= mobsInGame.Count || !mobsInGame[i].gameObject.activeInHierarchy)
                    continue;

                if (!(Vector3.Distance(explosionPosition, mobsInGame[i].transform.position + Vector3.up) <= distance)) continue;
                
                if (mobsInGame[i].playerMovement)
                {
                    continue;
                }

                if (mobsInGame[i].rb) // BARRELS
                {
                    mobsInGame[i].rb.AddForce((mobsInGame[i].visibilityTrigger.transform.position - explosionPosition).normalized *
                                               tileExplosionForceBarrels, ForceMode.VelocityChange);

                    mobsInGame[i].Damage(1, DamageSource.Player);

                    continue;
                }

                if (mobsInGame[i].DamageEndurance(enduranceDamage) <= 0)
                {
                    if (mobsInGame[i].HumanVisualController)
                    {
                        mobsInGame[i].HumanVisualController.ActivateRagdoll();
                        mobsInGame[i].HumanVisualController.ExplosionRagdoll(explosionPosition, force, distance);
                    }

                    if (mobsInGame[i].AiMovement)
                        mobsInGame[i].AiMovement.StopActivities();

                }
            }

            // BUMP PROPS
            bool propBumped = false;

            var closestBuilding = IslandSpawner.Instance.GetClosestTileBuilding(explosionPosition); 
            
            if (closestBuilding != null)
                for (int i = 0; i < closestBuilding.spawnedProps.Count; i++)
                {
                    if (Vector3.Distance(closestBuilding.spawnedProps[i].transform.position, explosionPosition) >
                        distance)
                        continue;

                    var rb = closestBuilding.spawnedProps[i].Rigidbody;

                    if (!rb) 
                        continue;
                
                    propBumped = true;
                    rb.AddExplosionForce(tileExplosionForceBarrels * 30, explosionPosition, distance);
                    closestBuilding.spawnedProps[i].tileAttack.dangerous = true;
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


        public void HealAllUnits()
        {
            foreach (var hc in mobsInGame)
            {
                if (hc == null || hc.health < 1)
                    continue;
                
                hc.AddHealth(hc.healthMax);
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

        public void KillAllMobs(bool realKill = false)
        {
            if (bossesInGame.Count > 0)
            {
                for (var index = bossesInGame.Count - 1; index >= 0; index--)
                {
                    var healthController = bossesInGame[index];
                    if (!healthController || healthController.IsDead)
                        continue;

                    if (realKill)
                        healthController.Kill();
                    else
                        healthController.DestroyOnDistance();
                }
            }
            if (mobsInGame.Count < 1)
                return;
            
            for (var index = mobsInGame.Count - 1; index >= 0; index--)
            {
                var healthController = mobsInGame[index];
                if (!healthController || healthController.IsDead)
                    continue;

                if (realKill)
                    healthController.Kill();
                else
                    healthController.DestroyOnDistance();
            }
        }
    }
}