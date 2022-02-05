using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
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
    public GameObject tileWallThinPrefab;
    public Cover coverPrefab;
    public List<Level> spawnedLevels = new List<Level>();

    public Transform generatedBuildingFolder;
    [Header("SETTINGS")]
    public List<int> levelsHeights = new List<int>();
    [Range(0,5)] public float offsetToThinWallsTargetDirection = 0;
    public Vector2Int coversPerLevelMinMax = new Vector2Int(1, 10);
    public Vector2Int stairsDistanceMinMax = new Vector2Int(5, 10);
    [Range(1, 3)] public float distanceToCutCeilingUnderStairs = 1;
    public Vector2Int thinWallsPerLevelMinMax = new Vector2Int(1, 10);
    public LayerMask solidsUnitsLayerMask;
    public bool randomLevelRotation = false;

    [Header("SCALE IS SCALED BY 2 IN CODE")]
    public Vector2Int levelsScaleMinMaxX = new Vector2Int(3, 10);
    public Vector2Int levelsScaleMinMaxZ = new Vector2Int(3, 10);
    [Space]
    public List<NavMeshSurface> navMeshSurfaces;
    public GameObject tileDestroyedParticles;
    private List<Vector3> navMeshChangedPositionQuere = new List<Vector3>();

    public bool levelIsReady = false;
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
            yield return StartCoroutine(SpawnNewLevel(i));
        }

        for (int i = 0; i < spawnedLevels.Count - 1; i++)
        {
            if (i != 0 && Random.value > 0.66f)
            {
                yield return StartCoroutine(MakeStairs(i));
            }   
            
            yield return StartCoroutine(MakeStairs(i));
        }
        yield return StartCoroutine(SpawnCovers());
        
        
        for (int i = 0; i < navMeshSurfaces.Count; i++)
        {
            navMeshSurfaces[i].BuildNavMesh();
        }

        StartCoroutine(UpdateNavMesh());
        yield return null;
        Respawner.Instance.Init();
        PlayerMovement.Instance.rb.MovePosition(spawnedLevels[0].tilesInside[Random.Range(0, spawnedLevels[0].tilesInside.Count)].transform.position + Vector3.up);
        levelIsReady = true;
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
        yield return StartCoroutine(SpawnLevelRooms());
        if (levelIndex == 0)
        {
            PlayerMovement.Instance.rb.MovePosition(levelPosition);
        }
    }

    IEnumerator SpawnLevelTiles(int index, Vector3 pos, Vector3Int size, Quaternion rot)
    {
        Level newLevel = new Level();
        newLevel.position = pos;

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
                    newLevel.tilesWalls.Add(newTile);
                    for (int y = 1; y < levelsHeights[index]; y++)
                    {
                        var newWallTile = Instantiate(tileWallPrefab, newLevel.spawnedTransform);
                        newWallTile.transform.localRotation = Quaternion.identity;
                        newWallTile.transform.position = newTile.transform.position + Vector3.up * y;
                        newLevel.tilesWalls.Add(newWallTile);
                    }
                }
                else
                {
                    // TILES INSIDE BUILDING
                    newLevel.tilesInside.Add(newTile);
                    
                    if (Random.value > 0.95f)
                    { //ADDITIONAL TILES
                        int r = Random.Range(1, 3);
                        for (int i = 1; i <= r; i++)
                        {
                            var newAdditionalTile = Instantiate(tileWallPrefab, newLevel.spawnedTransform);
                            newAdditionalTile.transform.localRotation = newTile.transform.localRotation;
                            newAdditionalTile.transform.localPosition = newTile.transform.localPosition + Vector3.up * i;
                            newLevel.tilesWalls.Add(newAdditionalTile);
                        }
                    }
                }
            }
            yield return null;   
        }
        
        spawnedLevels.Add(newLevel);
    }

    [ContextMenu("ToggleRoomsTiles")]
    public void ToggleRoomsTiles()
    {
        var room = spawnedLevels[0].spawnedRooms[Random.Range(0, spawnedLevels[0].spawnedRooms.Count)];
        for (int i = 0; i < room.tilesInside.Count; i++)
        {
            room.tilesInside[i].gameObject.SetActive(!room.tilesInside[i].gameObject.activeInHierarchy);
        }
    }
    
    IEnumerator SpawnLevelRooms()
    {
        // RANDOM WALLS METHOD 
        for (int i = 0; i < spawnedLevels.Count; i++)
        {
            for (int j = 0; j < Random.Range(thinWallsPerLevelMinMax.x, thinWallsPerLevelMinMax.y); j++)
            {
                var randomWallTile = spawnedLevels[i].tilesWalls[Random.Range(0, spawnedLevels[i].tilesWalls.Count)];
                Vector3 raycastDirection = Vector3.forward;
                Vector3 origin = new Vector3(randomWallTile.transform.position.x, spawnedLevels[i].position.y, randomWallTile.transform.position.z);

                Vector3 dir = Vector3.forward;
                float r = Random.value;
                if (r < 0.25f)
                    dir = Vector3.right;
                else if ( r < 0.5)
                    dir = Vector3.left;
                else if (r < 0.75f)
                    dir = Vector3.back;
                raycastDirection = (origin + dir + new Vector3(Random.Range(-offsetToThinWallsTargetDirection,offsetToThinWallsTargetDirection), 0, Random.Range(-offsetToThinWallsTargetDirection,offsetToThinWallsTargetDirection))) - origin;
                raycastDirection.Normalize();
                
                // origin + raycastDirection will prevent from raycasting itself
                if (Physics.Raycast(origin + Vector3.up + raycastDirection, 
                    raycastDirection, out var hit, 1000, 1 << 6))
                {
                    if (hit.distance < 5)
                        continue;
                    
                    Transform newWall = new GameObject("Thin Wall").transform;
                    newWall.parent = generatedBuildingFolder;
                    float doorPos = Random.Range(1, hit.distance - 2);
                    bool spawned = false;
                    for (float k = 0.5f; k < hit.distance + 1f; k++)
                    {
                        for (int l = 0; l < levelsHeights[i]-1; l++)
                        {
                            if (!spawned && k >= doorPos && l <= 2)
                                continue;
                            
                            var pos = origin + Vector3.up + raycastDirection * k + Vector3.up * l;
                            var rot = Quaternion.LookRotation(raycastDirection, Vector3.up);
                            var newTileWallThin = Instantiate(tileWallThinPrefab, pos, rot);
                            newTileWallThin.transform.parent = newWall;
                            spawnedLevels[i].tilesWalls.Add(newTileWallThin);
                        }
                        if (!spawned && k >= doorPos)
                        {
                            spawned = true;
                        }
                    }

                    yield return null;
                }
            }
        }
        yield break;
        
        // ROOMS METHOD
        // FIND ROOMS START TILES
        for (int i = 0; i < spawnedLevels.Count; i++)
        {
            spawnedLevels[i].spawnedRooms = new List<Room>();
            var freeLevelTiles = new List<GameObject>(spawnedLevels[i].tilesInside);
            int roomsAmount = Random.Range(thinWallsPerLevelMinMax.x, thinWallsPerLevelMinMax.y);
            for (int j = 0; j < roomsAmount; j++)
            {
                var newRoom = new Room();
                var randomTile = freeLevelTiles[Random.Range(0, freeLevelTiles.Count)].transform;
                
                spawnedLevels[i].spawnedRooms.Add(newRoom);
                
                freeLevelTiles.Remove(randomTile.gameObject);
                
                newRoom.tilesToGrow = new List<Transform>();
                newRoom.tilesToGrow.Add(randomTile);
            }
            
            bool noMoreRoom = false;
            while (!noMoreRoom)
            {
                noMoreRoom = true;
                for (int j = 0; j < spawnedLevels[i].spawnedRooms.Count; j++)
                {
                    var room = spawnedLevels[i].spawnedRooms[j];
                    var tempTilesToGrow = new List<Transform>();
                    for (int k = room.tilesToGrow.Count - 1; k >= 0; k--)
                    {
                        int amountOfNewNeighbours = 0;
                        for (int l = freeLevelTiles.Count - 1; l >= 0; l--)
                        {
                            if (amountOfNewNeighbours >= 3)
                                break;
                        
                            var freeTile = freeLevelTiles[l]; 
                            if (Vector3.Distance(room.tilesToGrow[k].transform.position,
                                freeTile.transform.position) < 2)
                            {
                                tempTilesToGrow.Add(freeTile.transform);
                                //room.tilesToGrow.Add(freeTile.transform);
                                freeLevelTiles.Remove(freeTile);
                                amountOfNewNeighbours++;
                                noMoreRoom = false;
                                yield return null;
                            }
                        }   
                        room.tilesInside.Add(room.tilesToGrow[k]);
                        room.tilesToGrow.RemoveAt(k);
                    }

                    for (int k = 0; k < tempTilesToGrow.Count; k++)
                    {
                        room.tilesToGrow.Add(tempTilesToGrow[k]);
                    }
                }
            }
            yield return null;
            continue;
            
            for (int j = 0; j < spawnedLevels[i].spawnedRooms.Count; j++)
            {
                var tiles = new List<Transform>(spawnedLevels[i].spawnedRooms[j].tilesInside);
                for (int k = 0; k < tiles.Count; k++)
                {
                    Vector3 isTileInFrontPos = tiles[k].transform.position + Vector3.forward;
                    Vector3 isTileInRightPos = tiles[k].transform.position + Vector3.right;
                    Vector3 isTileInBackPos = tiles[k].transform.position + Vector3.back;
                    Vector3 isTileInLeftPos = tiles[k].transform.position + Vector3.left;
                    
                    for (int l = 0; l < tiles.Count; l++)
                    {
                        yield return null;
                    }
                }
            }
        }
    }
    
    IEnumerator MakeStairs(int i)
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

        // SPAWN BRIDGE
        float bridgeTilesAmount = Vector3.Distance(levelFromClosestTile.position, levelToClosestTile.position);
        for (int j = 0; j <= bridgeTilesAmount; j++)
        {
            Quaternion rot = Quaternion.identity;
            rot = Quaternion.LookRotation(levelToClosestTile.position - levelFromClosestTile.position);

            Vector3 pos = (levelFromClosestTile.position + (levelToClosestTile.position - levelFromClosestTile.position).normalized * j);
            var newStairsTile = Instantiate(tilePrefab, pos, rot);
            
            var transformLocalScale = newStairsTile.transform.localScale;
            transformLocalScale.x = 1.5f;
            newStairsTile.transform.localScale = transformLocalScale;
            newStairsTile.transform.parent = generatedBuildingFolder;
            for (int k = 0; k < 2; k++)
            {
                var newStairsTileHandle = Instantiate(tilePrefab, newStairsTile.transform.position, newStairsTile.transform.rotation);
                newStairsTileHandle.transform.parent = newStairsTile.transform;
                float x = 0.546f;
                if (k == 1)
                    x *= -1;
                
                newStairsTileHandle.transform.localPosition = new Vector3(x, 0.898f, 0);
                newStairsTileHandle.transform.localScale = new Vector3(0.1f, 1f, 1);
            }
            stairsTiles.Add(newStairsTile.transform);
            yield return null;
        }

        // REMOVE TOP TILES AROUND
        for (int j = 0; j < stairsTiles.Count-1; j++)
        {
            Vector3 pos1 = new Vector3(stairsTiles[j].position.x, levelTo.tilesInside[0].transform.position.y, stairsTiles[j].position.z);
            for (int k = levelTo.tilesInside.Count - 1; k >= 0; k--)
            {
                if (levelTo.tilesInside[k].transform == levelToClosestTile)
                    continue;
                Vector3 pos2 = levelTo.tilesInside[k].transform.position;
                if (Vector3.Distance(pos1, pos2) < distanceToCutCeilingUnderStairs)
                {
                    Destroy(levelTo.tilesInside[k]);
                    levelTo.tilesInside.RemoveAt(k);
                }
            } 
            
            for (int k = levelTo.tilesWalls.Count - 1; k >= 0; k--)
            {
                if (levelTo.tilesWalls[k].transform == levelToClosestTile)
                    continue;
                
                pos1 = new Vector3(pos1.x, levelTo.tilesWalls[k].transform.position.y, pos1.z);
                Vector3 pos2 = levelTo.tilesWalls[k].transform.position;
                if (Vector3.Distance(pos1, pos2) < distanceToCutCeilingUnderStairs)
                {
                    Destroy(levelTo.tilesWalls[k]);
                    levelTo.tilesWalls.RemoveAt(k);
                }
            }
            for (int k = levelFrom.tilesWalls.Count - 1; k >= 0; k--)
            {
                pos1 = new Vector3(pos1.x, levelFrom.tilesWalls[k].transform.position.y, pos1.z);
                Vector3 pos2 = levelFrom.tilesWalls[k].transform.position;
                if (Vector3.Distance(pos1, pos2) < distanceToCutCeilingUnderStairs)
                {
                    Destroy(levelFrom.tilesWalls[k]);
                    levelFrom.tilesWalls.RemoveAt(k);
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
            for (int j = availableTiles.Count - 1; j >= 0; j--)
            {
                if (j >= availableTiles.Count)
                {
                    continue;
                }
                
                if (availableTiles[i] == null)
                    availableTiles.RemoveAt(i);
            }
            
            for (int j = 0; j < Random.Range(coversPerLevelMinMax.x, coversPerLevelMinMax.y); j++)
            {
                yield return null;
                var tileForCover = availableTiles[Random.Range(0, availableTiles.Count)];
                
                if (Physics.CheckBox(tileForCover.transform.position + Vector3.up * 2.5f, new Vector3(0.9f,4.75f,0.9f), Quaternion.identity , solidsUnitsLayerMask))
                    continue;
                
                var newCover = Instantiate(coverPrefab, tileForCover.transform.position + Vector3.up * 0.5f, Quaternion.identity);
                newCover.transform.parent = generatedBuildingFolder;
                availableTiles.Remove(tileForCover);
            }
        }
    }

    public void TileDamaged(BodyPart tile)
    {
        if (tilesToDamage.Contains(tile.transform))
            return;
        
        StartCoroutine(TileDamagedCoroutine(tile.transform));
    }

    private List<Transform> tilesToDamage = new List<Transform>();

    IEnumerator TileDamagedCoroutine(Transform tile)
    {
        float t = 0;
        tilesToDamage.Add(tile);
        Vector3 originalPosition = tile.position;
        while (t < 0.5f)
        {
            if (tile == null)
                yield break;
            
            t += Time.deltaTime;
            tile.position = originalPosition + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f),
                Random.Range(-0.1f, 0.1f));
            yield return null;
        }

        tilesToDamage.Remove(tile);
        tile.position = originalPosition + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f),
            Random.Range(-0.1f, 0.1f));;
    }
    public void TileDestroyed(BodyPart tile)
    {
        navMeshChangedPositionQuere.Add(tile.transform.position);
        Instantiate(tileDestroyedParticles, tile.transform.position, Quaternion.identity);
        //AddNavMeshSurfaceToQueue(tile.transform.position);
    }

    void AddNavMeshSurfaceToQueue(Vector3 pos)
    {
        float distance = 1000;
        NavMeshSurface closestNavMeshSurface = null;
        for (int i = 0; i < navMeshSurfaces.Count; i++)
        {
            float newDistance = Vector3.Distance(pos, navMeshSurfaces[i].transform.position);
            if (newDistance < distance)
            {
                distance = newDistance;
                closestNavMeshSurface = navMeshSurfaces[i];
            }
        }
        // add closest navmesh to navmesh queue
    }
    
    IEnumerator UpdateNavMesh()
    {
        while (true)
        {
            yield return null;
            
            for (int i = 0; i < navMeshSurfaces.Count; i++)
            {
                navMeshSurfaces[i].UpdateNavMesh(navMeshSurfaces[i].navMeshData);
                yield return new WaitForSecondsRealtime(0.5f);
            }
            
            continue;
            /*
            float distance = 10000;
            NavMeshSurface closestSurface = null;
            for (int i = 0; i < navMeshSurfaces.Count; i++)
            {
                float newDistance = Vector3.Distance(navMeshSurfaces[i].transform.position, pos);
                if (newDistance < distance)
                {
                    distance = newDistance;
                    closestSurface = navMeshSurfaces[i];
                }
            }   
            closestSurface.UpdateNavMesh(closestSurface.navMeshData);
            */   
        }
    }
    
    public void AddNavMeshBubble(NavMeshSurface bubble)
    {
        navMeshSurfaces.Add(bubble);
    }
    public void RemoveNavMeshBubble(NavMeshSurface bubble)
    {
        bubble.RemoveData();
        navMeshSurfaces.Remove(bubble);
    }
}

[Serializable]
public class Level
{
    public List<Room> spawnedRooms = new List<Room>();
    public List<GameObject> tilesInside = new List<GameObject>();
    public List<GameObject> tilesWalls = new List<GameObject>();
    public Transform spawnedTransform;
    public Vector3 position;
}

[Serializable]
public class Room
{
    public List<Transform> tilesToGrow = new List<Transform>();
    public List<Transform> tilesInside = new List<Transform>();
}