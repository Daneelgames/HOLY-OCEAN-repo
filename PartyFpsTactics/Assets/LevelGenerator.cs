using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelGenerator : MonoBehaviour
{
    public GameObject tilePrefab;
    public List<Level> spawnedLevels = new List<Level>();

    public Transform generatedBuildingFolder;
    [Header("SETTINGS")]
    public List<int> levelsHeights = new List<int>();
    [Range(1,10)]
    public Vector2Int roomsPerLevelMinMax = new Vector2Int(1, 10);
    public bool randomLevelRotation = false;

    [Header("SCALE IS SCALED BY 2 IN CODE")]
    public Vector2Int levelsScaleMinMaxX = new Vector2Int(3, 10);
    public Vector2Int roomsScaleMinMaxY = new Vector2Int(2, 6);
    public Vector2Int roomsScaleMinMaxZ = new Vector2Int(3, 10);
    
    IEnumerator Start()
    {
        if (generatedBuildingFolder == null)
        {
            generatedBuildingFolder = new GameObject("GeneratedBuilding").transform;
            generatedBuildingFolder.position = Vector3.zero;
        }
        
        if (levelsHeights.Count == 0) // default 5 floors building
        {
            levelsHeights = new List<int>();
            for (int i = 0; i < 5; i++)
            {
                levelsHeights.Add(5);
            }
        }

        for (int i = 0; i < levelsHeights.Count; i++)
        {
            Debug.Log("Spawn New Level " + i);
            yield return StartCoroutine(SpawnNewLevel(i));
        }
        
        yield return StartCoroutine(ConnectRooms());
    }

    
    IEnumerator SpawnNewLevel(int levelIndex)
    {
        float levelY = 0;
        
        if (levelIndex > 0)
        {
            levelY = levelIndex * 5;
        }
        
        Vector3 levelPosition = new Vector3(0, levelY, 0);
        
        Vector3Int levelSize = new Vector3Int(Random.Range(levelsScaleMinMaxX.x, levelsScaleMinMaxX.y) * 2,
            levelsHeights[levelIndex],Random.Range(roomsScaleMinMaxZ.x, roomsScaleMinMaxZ.y) * 2);
        
        Quaternion levelRotation = Quaternion.identity;
        if (randomLevelRotation)
            levelRotation = Quaternion.Euler(0, Random.Range(0,360), 0);

        yield return StartCoroutine(SpawnLevelTiles(levelIndex, levelPosition, levelSize, levelRotation));
        if (levelIndex == 0)
        {
            PlayerMovement.Instance.rb.MovePosition(levelPosition);
        }
        yield break;
        
        if (!Physics.CheckBox(levelPosition, new Vector3(levelSize.x, levelSize.y, levelSize.z), levelRotation, 1 << 6))
        {
            yield return StartCoroutine(SpawnLevelTiles(levelIndex, levelPosition, levelSize, levelRotation));
            if (levelIndex == 0)
            {
                PlayerMovement.Instance.rb.MovePosition(levelPosition);
            }
            yield break;
        }
    }

    IEnumerator SpawnLevelTiles(int index, Vector3 pos, Vector3Int size, Quaternion rot)
    {
        Level newLevel = new Level();
        newLevel.position = pos;
        newLevel.size = size;
        newLevel.rotation = rot;

        GameObject newGameObject = new GameObject("Level " + index);
        newGameObject.transform.parent = generatedBuildingFolder;
        newLevel.spawnedTransform = newGameObject.transform;
        newGameObject.transform.position = pos;
        newGameObject.transform.rotation = rot;
        
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.z; z++)
            {
                var newTile = Instantiate(tilePrefab, newLevel.spawnedTransform);
                newTile.transform.localRotation = Quaternion.identity;
                newTile.transform.localPosition = new Vector3(x - size.x / 2, 0, z - size.z/2);
                newLevel.tiles.Add(newTile);

                if (x == 0 || x == size.x - 1 || z == 0 || z == size.z - 1) // SPAWN OUTSIDE WALLS
                {
                    for (int y = 1; y < levelsHeights[index]; y++)
                    {
                        var newWallTile = Instantiate(tilePrefab, newLevel.spawnedTransform);
                        newWallTile.transform.localRotation = Quaternion.identity;
                        newWallTile.transform.position = newTile.transform.position + Vector3.up * y;
                        newLevel.tiles.Add(newWallTile);
                    }
                }
            }
            yield return null;   
        }
        
        spawnedLevels.Add(newLevel);
    }

    IEnumerator ConnectRooms()
    {
        for (int i = 0; i < spawnedLevels.Count - 1; i++)
        {
            Level levelFrom = spawnedLevels[i];
            Level levelTo = spawnedLevels[i + 1];

            Transform roomFromClosestTile = levelFrom.tiles[0].transform;
            Transform roomToClosestTile = levelTo.tiles[0].transform;
            
            float distance = 10000;
            float newDistance = 0;
            
            for (int j = 0; j < levelFrom.tiles.Count; j++)
            {
                for (int k = 0; k < levelTo.tiles.Count; k++)
                {
                    if (j != 0 && k != 0 && j != levelFrom.tiles.Count-1 && k != levelTo.tiles.Count-1)
                        continue;
                    
                    var tile1 = levelFrom.tiles[j];
                    var tile2 = levelTo.tiles[k];
                    newDistance = Vector3.Distance(tile1.transform.position, tile2.transform.position);
                    if (newDistance < distance)
                    {
                        distance = newDistance;
                        roomFromClosestTile = tile1.transform;
                        roomToClosestTile = tile2.transform;
                    }
                }
            }

            float bridgeTilesAmount = distance;
            for (int j = 0; j <= bridgeTilesAmount; j++)
            {
                Quaternion rot = Quaternion.Lerp(roomFromClosestTile.rotation, roomToClosestTile.rotation, j/bridgeTilesAmount);
                var newTile = Instantiate(tilePrefab, 
                    roomFromClosestTile.position + (roomToClosestTile.position - roomFromClosestTile.position).normalized * j, rot);
                newTile.transform.parent = generatedBuildingFolder;
                yield return null;
            }
            yield return null;
        }
    }
}

[Serializable]
public class Level
{
    public List<GameObject> tiles = new List<GameObject>();
    public Transform spawnedTransform;
    public Vector3 position;
    public Vector3Int size;
    public Quaternion rotation;
}