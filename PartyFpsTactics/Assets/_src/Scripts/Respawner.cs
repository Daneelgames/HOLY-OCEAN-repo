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
        List<TileHealth> tilesForSpawns = new List<TileHealth>();

        public static Respawner Instance;
        public bool spawn = false;
        private void Awake()
        {
            Instance = this;
        }

        public void SpawnEnemiesInBuilding(BuildingGenerator.Building building)
        {
            if (!spawn)
                return;
            StartCoroutine(SpawnEnemiesInBuildingCoroutine(building));
        }

        IEnumerator SpawnEnemiesInBuildingCoroutine(BuildingGenerator.Building building)
        {
            tilesForSpawns = new List<TileHealth>();

            for (int i = 0; i < building.spawnedBuildingLevels.Count; i++)
            {
                var level = building.spawnedBuildingLevels[i];
                /*if (level.spawnUnits == false)
                    continue;*/
                
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
                    yield return null;
                }
                for (int j = 0; j < level.uniqueNpcToSpawn.Count; j++)
                {
                    var randomTile = tilesForSpawns[Random.Range(0, tilesForSpawns.Count)];
                    UnitsManager.Instance.SpawnUnit(level.uniqueNpcToSpawn[j], randomTile.transform.position);
                    yield return null;
                }
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
            
            if (UnitsManager.Instance.HcInGame.Count < 1)
                return;
            
            for (int i = UnitsManager.Instance.HcInGame.Count - 1; i >= 0; i--)
            {
                if (i >= UnitsManager.Instance.HcInGame.Count)
                    continue;
            
                var corpse = UnitsManager.Instance.HcInGame[i];
                
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

    }
}
