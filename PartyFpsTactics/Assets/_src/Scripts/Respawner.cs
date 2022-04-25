using System.Collections;
using System.Collections.Generic;
using MrPink;
using MrPink.Health;
using MrPink.PlayerSystem;
using MrPink.Units;
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
        float banditsSpawnCooldown = 60;
        int maxAmount = 30;
        public List<Transform> playerRespawns;
        public Vector2Int enemiesPerRoomMinMax = new Vector2Int(3,10);
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
            
            
            // create enemy spawns
            tilesForSpawns = new List<TileHealth>();
            enemiesPerRoomMinMax = ProgressionManager.Instance.levelDatas[ProgressionManager.Instance.currentLevelIndex].enemiesPerRoomMinMax;

            for (int i = 0; i < LevelGenerator.Instance.spawnedMainBuildingLevels.Count; i++)
            {
                tilesForSpawns.Clear();
                for (var index = LevelGenerator.Instance.spawnedMainBuildingLevels[i].tilesInside.Count - 1; index >= 0; index--)
                {
                    var tile = LevelGenerator.Instance.spawnedMainBuildingLevels[i].tilesInside[index];
                    if (tile == null)
                    {
                        LevelGenerator.Instance.spawnedMainBuildingLevels[i].tilesInside.RemoveAt(index);
                        continue;
                    }
                    tilesForSpawns.Add(tile);
                }

                if (i == 0)
                {
                    for (int j = 0; j <  LevelGenerator.Instance.spawnedAdditionalLevels.Count; j++)
                    {
                        for (var index = LevelGenerator.Instance.spawnedAdditionalLevels[j].tilesInside.Count - 1; index >= 0; index--)
                        {
                            var tile = LevelGenerator.Instance.spawnedAdditionalLevels[j].tilesInside[index];
                            if (tile == null)
                            {
                                LevelGenerator.Instance.spawnedAdditionalLevels[j].tilesInside.RemoveAt(index);
                                continue;
                            }
                            tilesForSpawns.Add(tile);
                        }
                    }
                }


                int enemiesAmount = Random.Range(enemiesPerRoomMinMax.x, enemiesPerRoomMinMax.y);
                if (i == 0)
                    enemiesAmount += LevelGenerator.Instance.additionalSmallBuildingsAmount;
                for (int j = 0; j < enemiesAmount; j++)
                {
                    var randomTile = tilesForSpawns[Random.Range(0, tilesForSpawns.Count)];
                    var newSpawnPoint = new GameObject("RedSpawnPoint");
                    newSpawnPoint.transform.parent = transform;
                    redRespawns.Add(newSpawnPoint.transform);
                
                    UnitsManager.Instance.SpawnRedUnit(randomTile.transform.position);
                }
            }

            int additionalNpcAmount = Random.Range(ProgressionManager.Instance
                .levelDatas[ProgressionManager.Instance.currentLevelIndex].npcsPerMainBuildingRoomMinMax.x, 
                ProgressionManager.Instance.levelDatas[ProgressionManager.Instance.currentLevelIndex].npcsPerMainBuildingRoomMinMax.y);
            for (int i = 0; i < additionalNpcAmount; i++)
            {
                var tiles = LevelGenerator.Instance
                    .spawnedAdditionalLevels[Random.Range(0, LevelGenerator.Instance.spawnedAdditionalLevels.Count)]
                    .tilesInside;
                var randomTIle = tiles[Random.Range(0, tiles.Count)];
                UnitsManager.Instance.SpawnNeutralUnit(randomTIle.transform.position);
            }
            
            
            for (int j = 0; j < alliesAmount; j++)
            {
                var randomTile = tilesForSpawns[Random.Range(0, tilesForSpawns.Count)];
                UnitsManager.Instance.SpawnBlueUnit(randomTile.transform.position);   
            }

            for (int i = 0; i < ProgressionManager.Instance.CurrentLevel.desertBeastsSpawnAmount; i++)
            {
                UnitsManager.Instance.SpawnDesertBeast(desertRespawns[Random.Range(0, desertRespawns.Count)].position);
            }

            StartCoroutine(RespawnDesertBandits());
        }

        IEnumerator RespawnDesertBandits()
        {
            while (true)
            {
                if (desertBanditsSpawned.Count > 0)
                {
                    for (int i = desertBanditsSpawned.Count - 1; i >= 0; i--)
                    {
                        if (desertBanditsSpawned[i] == null || desertBanditsSpawned[i].health <= 0)
                            desertBanditsSpawned.RemoveAt(i);
                    }
                }
                
                List<Transform> spawns = new List<Transform>();
                for (int i = 0; i < banditsRespawns.Count; i++)
                {
                    float dist = Vector3.Distance(Game.Player.Position, banditsRespawns[i].position);
                    if (dist < 400 && dist > 50)
                    {
                        spawns.Add(banditsRespawns[i]);
                    }
                }

                if (spawns.Count > 0)
                {
                    for (int i = desertBanditsSpawned.Count; i < maxAmount; i++)
                    {
                        var bandit = UnitsManager.Instance.SpawnRedUnit(spawns[Random.Range(0, spawns.Count)].position);
                        desertBanditsSpawned.Add(bandit);
                    }
                }
                yield return new WaitForSeconds(banditsSpawnCooldown);
            }
        }
        
        void Update()
        {
            if (Game.Player.Position.y < corpseShredderY)
            {
                GameManager.Instance.StartProcScene();
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

        public void MovePlayerToRandomRespawner()
        {
            Game.Player.Movement.TeleportToPosition(playerRespawns[Random.Range(0, playerRespawns.Count)].position);
        }
    }
}
