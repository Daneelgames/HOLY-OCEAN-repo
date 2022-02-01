using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator Instance;
    public GameObject tilePrefab;
    public GameObject tileWallPrefab;
    public Cover coverPrefab;
    public List<Level> spawnedLevels = new List<Level>();

    public Transform generatedBuildingFolder;
    [Header("SETTINGS")]
    public List<int> levelsHeights = new List<int>();
    public Vector2Int coversPerLevelMinMax = new Vector2Int(1, 10);
    public Vector2Int stairsDistanceMinMax = new Vector2Int(5, 10);
    [Range(1, 3)] public float distanceToCutCeilingUnderStairs = 1;
    public Vector2Int roomsPerLevelMinMax = new Vector2Int(1, 10);
    public bool randomLevelRotation = false;

    [Header("SCALE IS SCALED BY 2 IN CODE")]
    public Vector2Int levelsScaleMinMaxX = new Vector2Int(3, 10);
    public Vector2Int levelsScaleMinMaxZ = new Vector2Int(3, 10);

    public NavMeshSurface navMeshSurface;

    private void Awake()
    {
        Instance = this;
    }

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
        
        yield return StartCoroutine(MakeStairs());
        yield return StartCoroutine(SpawnCovers());
        
        PlayerMovement.Instance.rb.MovePosition(spawnedLevels[0].tilesInside[Random.Range(0, spawnedLevels[0].tilesInside.Count)].transform.position + Vector3.up);
        navMeshSurface.BuildNavMesh();
        Respawner.Instance.Init();
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
            levelsHeights[levelIndex],Random.Range(levelsScaleMinMaxZ.x, levelsScaleMinMaxZ.y) * 2);
        
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

                if (x == 0 || x == size.x - 1 || z == 0 || z == size.z - 1) 
                {
                    // SPAWN WALLS AROUND
                    newLevel.tilesWallsAround.Add(newTile);
                    for (int y = 1; y < levelsHeights[index]; y++)
                    {
                        var newWallTile = Instantiate(tileWallPrefab, newLevel.spawnedTransform);
                        newWallTile.transform.localRotation = Quaternion.identity;
                        newWallTile.transform.position = newTile.transform.position + Vector3.up * y;
                        newLevel.tilesWallsAround.Add(newWallTile);
                    }
                }
                else
                {
                    // TILES INSIDE BUILDING
                    newLevel.tilesInside.Add(newTile);
                }
            }
            yield return null;   
        }
        
        spawnedLevels.Add(newLevel);
    }
    
    IEnumerator MakeStairs()
    {

        for (int i = 0; i < spawnedLevels.Count - 1; i++)
        {
            Level levelFrom = spawnedLevels[i];
            Level levelTo = spawnedLevels[i + 1];
            
            Transform levelFromClosestTile = levelFrom.tilesInside[Random.Range(0, levelFrom.tilesInside.Count)].transform;
            Transform levelToClosestTile = levelTo.tilesInside[levelTo.tilesInside.Count/2].transform;

            float distance = 10000;
            for (int j = 0; j < levelTo.tilesInside.Count; j++)
            {
                float newDistance = Vector3.Distance(levelFromClosestTile.position,
                    levelTo.tilesInside[j].transform.position);
                if (newDistance < distance)
                {
                    distance = newDistance;
                    levelToClosestTile = levelTo.tilesInside[j].transform;
                }
            }

            for (int j = 0; j < levelTo.tilesInside.Count; j++)
            {
                var tile = levelTo.tilesInside[j].transform;
                if (tile == levelToClosestTile)
                    continue;

                if (!Mathf.Approximately(tile.transform.position.x, levelToClosestTile.position.x) && !Mathf.Approximately(tile.transform.position.z, levelToClosestTile.position.z))
                {
                    continue;
                }
                
                float dst = Vector3.Distance(tile.position, levelToClosestTile.position);

                if (dst > stairsDistanceMinMax.x && dst < stairsDistanceMinMax.y)
                {
                    levelToClosestTile = tile;
                    break;
                }
            }

            List<Transform> stairsTiles = new List<Transform>();

            float bridgeTilesAmount = Vector3.Distance(levelFromClosestTile.position, levelToClosestTile.position);
            for (int j = 0; j <= bridgeTilesAmount; j++)
            {
                Quaternion rot = Quaternion.identity;
                rot = Quaternion.LookRotation(levelToClosestTile.position - levelFromClosestTile.position);

                Vector3 pos = (levelFromClosestTile.position + (levelToClosestTile.position - levelFromClosestTile.position).normalized * j);
                //pos = new Vector3(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
                var newTile = Instantiate(tilePrefab, pos, rot);
                newTile.transform.parent = generatedBuildingFolder;
                stairsTiles.Add(newTile.transform);
                yield return null;
            }

            // remove top floor tiles
            for (int j = 0; j < stairsTiles.Count; j++)
            {
                Vector3 pos1 = new Vector3(stairsTiles[j].position.x, levelTo.tilesInside[0].transform.position.y, stairsTiles[j].position.z);
                for (int k = levelTo.tilesInside.Count - 1; k >= 0; k--)
                {
                    if (levelTo.tilesInside[k] == levelToClosestTile)
                        continue;
                    Vector3 pos2 = levelTo.tilesInside[k].transform.position;
                    if (Vector3.Distance(pos1, pos2) < distanceToCutCeilingUnderStairs)
                    {
                        Destroy(levelTo.tilesInside[k]);
                        levelTo.tilesInside.RemoveAt(k);
                    }
                } 
                
                for (int k = levelTo.tilesWallsAround.Count - 1; k >= 0; k--)
                {
                    if (levelTo.tilesWallsAround[k] == levelToClosestTile)
                        continue;
                    
                    pos1 = new Vector3(pos1.x, levelTo.tilesWallsAround[k].transform.position.y, pos1.z);
                    Vector3 pos2 = levelTo.tilesWallsAround[k].transform.position;
                    if (Vector3.Distance(pos1, pos2) < distanceToCutCeilingUnderStairs)
                    {
                        Destroy(levelTo.tilesWallsAround[k]);
                        levelTo.tilesWallsAround.RemoveAt(k);
                    }
                }
                for (int k = levelFrom.tilesWallsAround.Count - 1; k >= 0; k--)
                {
                    pos1 = new Vector3(pos1.x, levelFrom.tilesWallsAround[k].transform.position.y, pos1.z);
                    Vector3 pos2 = levelFrom.tilesWallsAround[k].transform.position;
                    if (Vector3.Distance(pos1, pos2) < distanceToCutCeilingUnderStairs)
                    {
                        Destroy(levelFrom.tilesWallsAround[k]);
                        levelFrom.tilesWallsAround.RemoveAt(k);
                    }
                }
            }

            yield return null;
        }
    }

    IEnumerator SpawnCovers()
    {
        for (int i = 0; i < spawnedLevels.Count; i++)
        {
            List<GameObject> availableTiles = new List<GameObject>(spawnedLevels[i].tilesInside);
            for (int j = 0; j < Random.Range(coversPerLevelMinMax.x, coversPerLevelMinMax.y); j++)
            {
                var tileForCover = availableTiles[Random.Range(0, availableTiles.Count)];
                Instantiate(coverPrefab, tileForCover.transform.position, Quaternion.identity);
                availableTiles.Remove(tileForCover);
            }
            yield return null;
        }
    }
}

[Serializable]
public class Level
{
    public List<GameObject> tilesInside = new List<GameObject>();
    public List<GameObject> tilesWallsAround = new List<GameObject>();
    public Transform spawnedTransform;
    public Vector3 position;
    public Vector3Int size;
    public Quaternion rotation;
}