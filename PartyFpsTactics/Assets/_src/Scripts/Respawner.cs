using System.Collections;
using System.Collections.Generic;
using MrPink;
using MrPink.Health;
using MrPink.PlayerSystem;
using MrPink.Units;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _src.Scripts
{
    public class Respawner : MonoBehaviour
    {
        public float corpseShredderY = -25;
        public List<Transform> redRespawns;
        public List<Transform> desertRespawns;
        public List<Transform> banditsRespawns;
        private List<HealthController> desertBanditsSpawned = new List<HealthController>();
        public float banditsSpawnCooldown = 60;
        public float banditsSpawnDistanceMin = 30;
        public float banditsSpawnDistanceMax = 200;
        public int banditsMaxAmount = 30;
        public List<Transform> playerRespawns;
        public List<Transform> blueRespawns;
        public int alliesAmount = 3;
        List<TileHealth> tilesForSpawns = new List<TileHealth>();

        public static Respawner Instance;
        public bool spawn = false;
        private void Awake()
        {
            Instance = this;
        }

        public void Init()
        {
            if (!spawn)
                return;
            StartCoroutine(SpawnStartEnemies());
        }

        IEnumerator SpawnStartEnemies()
        {
            tilesForSpawns = new List<TileHealth>();

            for (int i = 0; i < BuildingGenerator.Instance.spawnedBuildingLevels.Count; i++)
            {
                var level = BuildingGenerator.Instance.spawnedBuildingLevels[i];
                if (level.spawnUnits == false)
                    continue;
                
                tilesForSpawns.Clear();
                for (var index = level.tilesInside.Count - 1; index >= 0; index--)
                {
                    var tile = level.tilesInside[index];
                    if (tile == null)
                    {
                        level.tilesInside.RemoveAt(index);
                        continue;
                    }
                    tilesForSpawns.Add(tile);
                }

                for (int j = 0; j < level.unitsToSpawn.Count; j++)
                {
                    var randomTile = tilesForSpawns[Random.Range(0, tilesForSpawns.Count)];
                    UnitsManager.Instance.SpawnUnit(level.unitsToSpawn[j], randomTile.transform.position);
                }
                for (int j = 0; j < level.uniqueNpcToSpawn.Count; j++)
                {
                    var randomTile = tilesForSpawns[Random.Range(0, tilesForSpawns.Count)];
                    UnitsManager.Instance.SpawnUnit(level.uniqueNpcToSpawn[j], randomTile.transform.position);
                }
            }
            
            for (int i = 0; i < ProgressionManager.Instance.CurrentLevel.desertBeastsSpawnAmount; i++)
            {
                UnitsManager.Instance.SpawnDesertBeast(desertRespawns[Random.Range(0, desertRespawns.Count)].position);
                yield return null;
            }

            StartRespawningBandits(Game.Player.Position);
        }

        void StartRespawningBandits(Vector3 aroundPosition)
        {
            if (respawnDesertBanditsCoroutine != null)
                StopCoroutine(respawnDesertBanditsCoroutine);
            
            respawnDesertBanditsCoroutine = StartCoroutine(RespawnDesertBandits(aroundPosition));
        }

        private Coroutine respawnDesertBanditsCoroutine;
        IEnumerator RespawnDesertBandits(Vector3 aroundPosition)
        {
            Vector3 posToCheck = aroundPosition;
            while (true)
            {
                if (desertBanditsSpawned.Count > 0)
                {
                    for (int i = desertBanditsSpawned.Count - 1; i >= 0; i--)
                    {
                        if (desertBanditsSpawned[i] == null)
                        {
                            desertBanditsSpawned.RemoveAt(i);
                            continue;
                        }

                        if (Vector3.Distance(desertBanditsSpawned[i].transform.position, posToCheck) > 200)
                            Destroy(desertBanditsSpawned[i].gameObject);
                        
                        if (desertBanditsSpawned[i].health <= 0)
                            desertBanditsSpawned.RemoveAt(i);
                    }
                }
                
                List<Transform> spawns = new List<Transform>();
                for (int i = 0; i < banditsRespawns.Count; i++)
                {
                    float dist = Vector3.Distance(posToCheck, banditsRespawns[i].position);
                    if (dist < banditsSpawnDistanceMax && dist > banditsSpawnDistanceMin)
                    {
                        spawns.Add(banditsRespawns[i]);
                    }
                }

                if (spawns.Count > 0)
                {
                    for (int i = desertBanditsSpawned.Count; i < banditsMaxAmount; i++)
                    {
                        yield return null;
                        var bandit = UnitsManager.Instance.SpawnRedUnit(spawns[Random.Range(0, spawns.Count)].position);
                        desertBanditsSpawned.Add(bandit);
                    }
                }
                yield return new WaitForSeconds(banditsSpawnCooldown);
                posToCheck = Game.Player.Position;
            }
        }
        
        void Update()
        {
            if (Game.Player.Position.y < corpseShredderY)
            {
                GameManager.Instance.KillPlayer();
                GameManager.Instance.RespawnPlayer();
                return;
            }
            for (int i = UnitsManager.Instance.unitsInGame.Count - 1; i >= 0; i--)
            {
                if (i >= UnitsManager.Instance.unitsInGame.Count)
                    continue;
            
                var corpse = UnitsManager.Instance.unitsInGame[i];
                
                if (corpse.transform.position.y < corpseShredderY)
                {
                    if (corpse == Game.Player.Health)
                    {
                        GameManager.Instance.KillPlayer();
                        GameManager.Instance.RespawnPlayer();
                        return;
                    }
                    Destroy(corpse.gameObject);
                }
            }
        }

        public List<Transform> GetSpawnersInRange(List<Transform> spawners, Vector3 origin, float minRange, float maxRange)
        {
            List<Transform> temp = new List<Transform>(spawners);
            for (int i = temp.Count - 1; i >= 0; i--)
            {
                float dst = Vector3.Distance(origin, temp[i].position);
                if (dst < minRange || dst > maxRange)
                    temp.RemoveAt(i);
            }

            return temp;
        }

        public Vector3 MovePlayerToRandomRespawner()
        {
            if (desertBanditsSpawned.Count > 0)
            {
                for (int i = desertBanditsSpawned.Count - 1; i >= 0; i--)
                {
                    if (desertBanditsSpawned[i] != null)
                        Destroy(desertBanditsSpawned[i].gameObject);
                }
            }
            desertBanditsSpawned.Clear();

            var pos = playerRespawns[Random.Range(0, playerRespawns.Count)].position;
            Game.Player.Movement.TeleportToPosition(pos);
            StartRespawningBandits(pos);
            return pos;
        }
    }
}
