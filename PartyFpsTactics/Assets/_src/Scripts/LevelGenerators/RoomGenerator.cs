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

        public IEnumerator GenerateRooms(List<Level> spawnedLevels, bool singleFrame)
        {
            for (int levelIndex = 0; levelIndex < spawnedLevels.Count; levelIndex++)
            {
                Debug.Log("GenerateRoomsCoroutine; spawnedLevels[levelIndex] " + spawnedLevels[levelIndex].gameObject.name +"; levelIndex " + levelIndex);
                
                if (spawnedLevels[levelIndex].spawnRooms == false)
                    continue;
                
                if (spawnedLevels[levelIndex].spawnedRooms.Count <= 0)
                    continue;
                
                var level = spawnedLevels[levelIndex];
                
                // spawn unique npc and spawn controlled machines

                int t = 0;
                for (int i = 0; i < level.controlledMachinesToSpawn.Count; i++)
                {
                    var room = spawnedLevels[levelIndex].spawnedRooms[Random.Range(0,spawnedLevels[levelIndex].spawnedRooms.Count)];
                    var roomTilesCoordsTemp = new List<Vector3Int>(room.coordsInside);
                    var randomTileCoords = roomTilesCoordsTemp[Random.Range(0, roomTilesCoordsTemp.Count)];
                    Vector3 worldSpawnPosition = new Vector3(randomTileCoords.x - level.size.x / 2, level.floorWorldHeight + 0.5f, randomTileCoords.z - level.size.z / 2);

                    Instantiate(level.controlledMachinesToSpawn[Random.Range(0, level.controlledMachinesToSpawn.Count)], worldSpawnPosition, Quaternion.identity);
                    
                    if (singleFrame)
                        continue;
                    
                    t++;
                    if (t > 5)
                    {
                        t = 0;
                        yield return null;
                    }
                }
            }
        }
    }
}
