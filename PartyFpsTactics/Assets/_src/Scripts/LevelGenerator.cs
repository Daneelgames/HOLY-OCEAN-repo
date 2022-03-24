using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.PlayerSystem;
using Unity.AI.Navigation;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator Instance;
    public enum LevelType{Game,Narrative}

    public LevelType levelType = LevelType.Game;
    public List<Level> spawnedLevels = new List<Level>();
    public GameObject levelGoalSpawned;

    public Transform generatedBuildingFolder;
    [Header("SETTINGS")]
    public List<int> levelsHeights = new List<int>();
    
    public bool spawnWalls = true;
    public bool spawnLadders = true;
    public bool spawnAdditionalTiles = true;

    [Range(0,5)] public float offsetToThinWallsTargetDirection = 0;
    
    public GameObject levelGoalPrefab;
    public GameObject tilePrefab;
    public GameObject tileWallPrefab;
    public GameObject tileWallThinPrefab;
    public GameObject explosiveBarrelPrefab;
    public GrindRail grindRailsPrefab;
    public Cover coverPrefab;
    
    public Vector2 distanceToCutCeilingUnderStairsMinMax = new Vector2(1,5);
    public Vector2Int grindRailsMinMax = new Vector2Int(1, 2);
    public Vector2Int coversPerLevelMinMax = new Vector2Int(1, 10);
    public Vector2Int stairsDistanceMinMax = new Vector2Int(5, 10);
    public Vector2Int thinWallsPerLevelMinMax = new Vector2Int(1, 10);
    
    public LayerMask solidsUnitsLayerMask;
    public bool randomLevelRotation = false;
    public int explosiveBarrelsAmount = 2;

    [Header("SCALE IS SCALED BY 2 IN CODE")]
    public Vector2Int levelsPosMinMaxX = new Vector2Int(-10, 10);
    public Vector2Int levelsPosMinMaxZ = new Vector2Int(-10, 10);
    public Vector2Int levelsScaleMinMaxX = new Vector2Int(3, 10);
    public Vector2Int levelsScaleMinMaxZ = new Vector2Int(3, 10);
    [Space] [Header("NAVIGATION")] 
    public Transform navMeshesParent;
    public NavMeshSurface navMeshSurfacePrefab;
    public List<NavMeshSurface> navMeshSurfacesSpawned;
    public GameObject tileDestroyedParticles;

    public bool levelIsReady = false;
    private void Awake()
    {
        Instance = this;
    }

    IEnumerator Start()
    {
        Init();
        if (generatedBuildingFolder == null)
        {
            generatedBuildingFolder = new GameObject("GeneratedBuilding").transform;
            generatedBuildingFolder.position = Vector3.zero;
        }

        switch (levelType)
        {
            case LevelType.Game:
                StartCoroutine(GenerateLevel());
                break;
            case LevelType.Narrative:
                
                // choose here what narrative sequence to load?
                // and then set level ready
                
                yield return new WaitForSecondsRealtime(1);
                levelIsReady = true;
                break;
        }
    }

    void Init()
    {
        var currentLevel = ProgressionManager.Instance.CurrentLevel;
        levelsPosMinMaxX = currentLevel.levelsPosMinMaxX;
        levelsPosMinMaxZ = currentLevel.levelsPosMinMaxZ;
        levelsScaleMinMaxX = currentLevel.levelsScaleMinMaxX;
        levelsScaleMinMaxZ = currentLevel.levelsScaleMinMaxZ;
        levelGoalPrefab = currentLevel.levelGoalPrefab;
        tilePrefab = currentLevel.tilePrefab;
        tileWallPrefab = currentLevel.tileWallPrefab;
        tileWallThinPrefab = currentLevel.tileWallThinPrefab;
        levelsHeights = currentLevel.levelsHeights;
        explosiveBarrelsAmount = currentLevel.explosiveBarrelsAmount;
        explosiveBarrelPrefab = currentLevel.explosiveBarrelPrefab;
        coversPerLevelMinMax = currentLevel.coversPerLevelMinMax;
        grindRailsMinMax = currentLevel.grindRailsPerLevelMinMax;
        grindRailsPrefab = currentLevel.grindRailsPrefab;
        stairsDistanceMinMax = currentLevel.stairsDistanceMinMax;
        thinWallsPerLevelMinMax = currentLevel.thinWallsPerLevelMinMax;
        distanceToCutCeilingUnderStairsMinMax = currentLevel.distanceToCutCeilingUnderStairsMinMax;
        spawnWalls = currentLevel.spawnWalls;
        spawnLadders = currentLevel.spawnLadders;
        spawnAdditionalTiles = currentLevel.spawnAdditionalTiles;

    }

    IEnumerator GenerateLevel()
    {
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

        if (spawnLadders)
        {
            for (int i = 0; i < spawnedLevels.Count - 1; i++)
            {
                if (i != 0 && Random.value > 0.66f)
                {
                    yield return StartCoroutine(MakeLadders(i));
                }

                yield return StartCoroutine(MakeLadders(i));
            }
        }
        if (spawnAdditionalTiles)
            yield return StartCoroutine(SpawnCovers());
        
        for (int i = 0; i < spawnedLevels.Count; i++)
        {
            SpawnNavmesh(spawnedLevels[i]);
        }
        
        for (int i = 0; i < navMeshSurfacesSpawned.Count; i++)
        {
            navMeshSurfacesSpawned[i].BuildNavMesh();
        }

        StartCoroutine(UpdateNavMesh());
        yield return null;
        Respawner.Instance.Init();
        Player.Movement.rb.MovePosition(spawnedLevels[0].tilesInside[Random.Range(0, spawnedLevels[0].tilesInside.Count)].transform.position + Vector3.up);
        levelIsReady = true;
        
        SpawnGoalOnTop();
        
        yield return StartCoroutine(SpawnExplosiveBarrels());
        yield return StartCoroutine(SpawnGrindRails());
    }


    IEnumerator SpawnNewLevel(int levelIndex)
    {
        float levelY = 0;
        
        if (levelIndex > 0)
        {
            levelY = levelIndex * 5;
        }
        
        Vector3 levelPosition = new Vector3(Random.Range(levelsPosMinMaxX.x, levelsPosMinMaxX.y), levelY, Random.Range(levelsPosMinMaxZ.x, levelsPosMinMaxZ.y));
        
        Vector3Int levelSize = new Vector3Int(Random.Range(levelsScaleMinMaxX.x, levelsScaleMinMaxX.y) * 2,
            levelsHeights[levelIndex],Random.Range(levelsScaleMinMaxZ.x, levelsScaleMinMaxZ.y) * 2);
        
        Quaternion levelRotation = Quaternion.identity;
        if (randomLevelRotation)
            levelRotation = Quaternion.Euler(0, Random.Range(0,360), 0);

        yield return StartCoroutine(SpawnLevelTiles(levelIndex, levelPosition, levelSize, levelRotation));
        yield return StartCoroutine(SpawnLevelRooms());
        if (levelIndex == 0)
        {
            Player.Movement.rb.MovePosition(levelPosition);
        }
    }

    IEnumerator SpawnLevelTiles(int index, Vector3 pos, Vector3Int size, Quaternion rot)
    {
        Level newLevel = new Level();
        newLevel.position = pos;
        newLevel.size = size;

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
                    if (!spawnWalls)
                        continue;
                    
                    // SPAWN WALLS AROUND
                    newLevel.tilesWalls.Add(newTile);
                    for (int y = 1; y < levelsHeights[index]; y++)
                    {
                        var newWallTile = Instantiate(tileWallPrefab, newLevel.spawnedTransform);
                        newWallTile.transform.localRotation = Quaternion.identity;
                        
                        // ROTATE
                        if (x == size.x - 1)
                            newWallTile.transform.localEulerAngles = new Vector3(0, 180, 0);
                        if (z == 0)
                            newWallTile.transform.localEulerAngles = new Vector3(0, 270, 0);
                        if (z == size.z - 1)
                            newWallTile.transform.localEulerAngles = new Vector3(0, 90, 0);
                            
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
                        int r = Random.Range(1, 4);
                        
                        for (int i = 1; i <= r; i++)
                        {
                            var newAdditionalTile = Instantiate(tileWallPrefab, newLevel.spawnedTransform);
                            
                            if (i == 1 && r > 1)
                                ConstructCover(newAdditionalTile.gameObject);
                            
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
    
    IEnumerator MakeLadders(int i)
    {
        Level levelFrom = spawnedLevels[i];
        Level levelTo = spawnedLevels[i + 1];

        for (int j = levelFrom.tilesInside.Count - 1; j >= 0; j--)
        {
            if (levelFrom.tilesInside[j] == null)
                levelFrom.tilesInside.RemoveAt(j);
        }
        for (int j = levelTo.tilesInside.Count - 1; j >= 0; j--)
        {
            if (levelTo.tilesInside[j] == null)
                levelTo.tilesInside.RemoveAt(j);
        }
        
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

        yield return null;
        StartCoroutine(SpawnLadder(levelFromClosestTile.position, levelToClosestTile.position, true));
    }

    public IEnumerator SpawnLadder(Vector3 fromPosition, Vector3 toPosition, bool destroyTilesAround, int maxBridgeTiles = -1)
    {
        List<Transform> stairsTiles = new List<Transform>();
        
        // SPAWN BRIDGE
        float bridgeTilesAmount = Vector3.Distance(fromPosition, toPosition);

        if (maxBridgeTiles > 0)
            bridgeTilesAmount = Mathf.Clamp(bridgeTilesAmount, 1, maxBridgeTiles);
        for (int j = 0; j <= bridgeTilesAmount; j++)
        {
            Quaternion rot = Quaternion.identity;
            rot = Quaternion.LookRotation(toPosition - fromPosition);

            Vector3 pos = (fromPosition + (toPosition-fromPosition).normalized * j);
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
                stairsTiles.Add(newStairsTileHandle.transform);
            }
            stairsTiles.Add(newStairsTile.transform);
            
            if (destroyTilesAround)
            {
                var hit = Physics.OverlapSphere(newStairsTile.transform.position, Random.Range(distanceToCutCeilingUnderStairsMinMax.x, distanceToCutCeilingUnderStairsMinMax.y), 1 << 6);
                
                for (int i = 0; i < hit.Length; i++)
                {
                    if (hit[i].transform == null)
                        continue;

                    if (newStairsTile == null || stairsTiles.Contains(hit[i].transform) ||
                        hit[i].transform.position.y < newStairsTile.transform.position.y + 1)
                    {
                        continue;
                    }

                    var bodyPart = hit[i].transform.gameObject.GetComponent<BodyPart>();
                    bodyPart.DamageTile(bodyPart.localHealth);
                }
            }
            yield return new WaitForSeconds(0.1f);
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
                for (int k = availableTiles.Count - 1; k >= 0; k--)
                {
                     if (availableTiles[k] == null)
                         availableTiles.RemoveAt(k);
                }
                var tileForCover = availableTiles[Random.Range(0, availableTiles.Count)];
                    
                if (Physics.CheckBox(tileForCover.transform.position + Vector3.up * 3f, new Vector3(0.75f,2.5f,0.75f), Quaternion.identity , solidsUnitsLayerMask))
                {
                    Debug.Log("Skip cover");
                    availableTiles.Remove(tileForCover);
                    continue;
                }
                
                var newCover = Instantiate(coverPrefab, tileForCover.transform.position + Vector3.up * 0.5f, Quaternion.identity);
                newCover.transform.parent = generatedBuildingFolder;
                availableTiles.Remove(tileForCover);
            }
        }
    }

    void SpawnCover(Vector3 pos)
    {
        var newCover = Instantiate(coverPrefab, pos + Vector3.up * 0.5f, Quaternion.identity);
        newCover.transform.parent = generatedBuildingFolder;
    }

    void SpawnNavmesh(Level spawnedLevel)
    {
        var newNavMesh = Instantiate(navMeshSurfacePrefab, navMeshesParent);
        newNavMesh.transform.position = spawnedLevel.position;
        newNavMesh.transform.localScale = spawnedLevel.size;
        newNavMesh.size = spawnedLevel.size;
        newNavMesh.center = new Vector3(0, newNavMesh.size.y/ 2, 0);
        navMeshSurfacesSpawned.Add(newNavMesh);
    }
    
    IEnumerator SpawnExplosiveBarrels()
    {
        for (int i = 0; i < explosiveBarrelsAmount; i++)
        {
            var randomLevel = spawnedLevels[Random.Range(1, spawnedLevels.Count)];
            var randomTile = randomLevel.tilesInside[Random.Range(0, randomLevel.tilesInside.Count)];

            Vector3 pos = randomTile.transform.position + Vector3.up;
            randomLevel.tilesInside.Remove(randomTile);
            Instantiate(explosiveBarrelPrefab, pos, Quaternion.identity);
            yield return null;
        }
    }
    
    IEnumerator SpawnGrindRails()
    {
            for (int j = 0; j < Random.Range(grindRailsMinMax.x, grindRailsMinMax.y); j++)
            {
                var randomLevel = spawnedLevels[Random.Range(0,spawnedLevels.Count)];
                var randomTile = randomLevel.tilesInside[Random.Range(0, randomLevel.tilesInside.Count)];

                Vector3 pos = randomTile.transform.position + Vector3.up;
                randomLevel.tilesInside.Remove(randomTile);
                var grindRails = Instantiate(grindRailsPrefab, pos, Quaternion.identity);
                grindRails.GenerateNodes(true);
                yield return null;
            }
    }

    void SpawnGoalOnTop()
    {
        Vector3 spawnPosition = spawnedLevels[spawnedLevels.Count - 1].position + Vector3.up * 2;
        levelGoalSpawned = Instantiate(levelGoalPrefab, spawnPosition, Quaternion.identity);
    }

    public void TileDamaged(BodyPart tile)
    {
        if (tilesToDamage.Contains(tile.transform))
            return;
        
        StartCoroutine(TileDamagedCoroutine(tile.transform));
    }
    public void TileDamaged(Transform tile)
    {
        if (tilesToDamage.Contains(tile))
            return;
        
        StartCoroutine(TileDamagedCoroutine(tile));
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
        if (tile)
            tile.position = originalPosition;
    }
    public void DebrisParticles(Vector3 pos)
    {
        Instantiate(tileDestroyedParticles, pos, Quaternion.identity);
    }

    void AddNavMeshSurfaceToQueue(Vector3 pos)
    {
        float distance = 1000;
        NavMeshSurface closestNavMeshSurface = null;
        for (int i = 0; i < navMeshSurfacesSpawned.Count; i++)
        {
            float newDistance = Vector3.Distance(pos, navMeshSurfacesSpawned[i].transform.position);
            if (newDistance < distance)
            {
                distance = newDistance;
                closestNavMeshSurface = navMeshSurfacesSpawned[i];
            }
        }
        // add closest navmesh to navmesh queue
    }
    
    IEnumerator UpdateNavMesh()
    {
        while (true)
        {
            yield return null;
            
            for (int i = 0; i < navMeshSurfacesSpawned.Count; i++)
            {
                navMeshSurfacesSpawned[i].UpdateNavMesh(navMeshSurfacesSpawned[i].navMeshData);
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
        navMeshSurfacesSpawned.Add(bubble);
    }
    public void RemoveNavMeshBubble(NavMeshSurface bubble)
    {
        bubble.RemoveData();
        navMeshSurfacesSpawned.Remove(bubble);
    }

    public void ConstructCover(GameObject newCoverGo)
    {
        var newCover = newCoverGo.gameObject.AddComponent<Cover>();
        newCover.ConstructSpots();
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
    public Vector3Int size;
}

[Serializable]
public class Room
{
    public List<Transform> tilesToGrow = new List<Transform>();
    public List<Transform> tilesInside = new List<Transform>();
}