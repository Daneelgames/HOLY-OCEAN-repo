using System.Collections;
using System.Collections.Generic;
using MrPink.Units;
using UnityEngine;

namespace _src.Scripts.LevelGenerators
{
    public class RoomGenerator : MonoBehaviour
    {
        public static RoomGenerator Instance;

        void Awake()
        {
            if (Instance)
            {
                Destroy(Instance.gameObject);
            }
        
            Instance = this;
        }

        public IEnumerator GenerateRooms(List<Level> spawnedLevels)
        {
            yield return StartCoroutine(GenerateRoomsCoroutine(spawnedLevels));
        }

        IEnumerator GenerateRoomsCoroutine(List<Level> spawnedLevels)
        {
            for (int levelIndex = 0; levelIndex < spawnedLevels.Count; levelIndex++)
            {
                if (spawnedLevels[levelIndex].spawnedRooms.Count <= 0)
                    continue;
                
                var level = spawnedLevels[levelIndex];
                var minMax = ProgressionManager.Instance.levelDatas[ProgressionManager.Instance.currentLevelIndex].npcsPerMainBuildingRoomMinMax;
                int amount = Random.Range(minMax.x, minMax.y);
                
                for (int roomIndex = 0; roomIndex < amount; roomIndex++)
                {
                    // SPAWN OBJECT OF INTEREST
                    var room = spawnedLevels[levelIndex].spawnedRooms[roomIndex];
                    var roomTilesCoordsTemp = new List<Vector3Int>(room.coordsInside);
                    var randomTileCoords = roomTilesCoordsTemp[Random.Range(0, roomTilesCoordsTemp.Count)];
                    Vector3 worldSpawnPosition = new Vector3(randomTileCoords.x - level.size.x / 2, level.floorWorldHeight + 0.5f, randomTileCoords.z - level.size.z / 2);
                    
                    if (Random.value > 0.5f)
                        UnitsManager.Instance.SpawnNeutralUnit(worldSpawnPosition);
                    else
                    {
                        Instantiate(LevelGenerator.Instance.controlledMachinesInRooms[Random.Range(0, LevelGenerator.Instance.controlledMachinesInRooms.Count)],
                            worldSpawnPosition, Quaternion.identity);
                    }
                    
                    
                    
                    // THEN SPAWN 
                    yield return null;
                }
            }
        }
    }
}
