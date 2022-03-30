using System.Collections;
using System.Collections.Generic;
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

        public void GenerateRooms(List<Level> spawnedLevels)
        {
            StartCoroutine(GenerateRoomsCoroutine(spawnedLevels));
        }

        IEnumerator GenerateRoomsCoroutine(List<Level> spawnedLevels)
        {
            for (int levelIndex = 0; levelIndex < spawnedLevels.Count; levelIndex++)
            {
                var level = spawnedLevels[levelIndex];
                for (int roomIndex = 0; roomIndex < spawnedLevels[levelIndex].spawnedRooms.Count; roomIndex++)
                {
                    // SPAWN OBJECT OF INTEREST
                    var room = spawnedLevels[levelIndex].spawnedRooms[roomIndex];
                    var roomTilesCoordsTemp = new List<Vector3Int>(room.coordsInside);
                    var randomTileCoords = roomTilesCoordsTemp[Random.Range(0, roomTilesCoordsTemp.Count)];
                    Vector3 worldSpawnPosition = new Vector3(randomTileCoords.x - level.size.x / 2, level.floorWorldHeight + 0.5f, randomTileCoords.z - level.size.z / 2);
                    UnitsManager.Instance.SpawnNeutralUnit(worldSpawnPosition);
                    
                    // THEN SPAWN 
                    yield return null;
                }
                yield return null;
            }
        }
    }
}
