using System;
using System.Collections;
using System.Collections.Generic;
using _src.Scripts;
using MrPink;
using MrPink.Health;
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
    public TileHealth tilePrefab;
    public TileHealth tileWallPrefab;
    public TileHealth tileWallThinPrefab;
    public List<TileHealth> tileWallThinColorPrefabs;
    public GameObject explosiveBarrelPrefab;
    public GrindRail grindRailsPrefab;
    public List<TileHealth> propsPrefabs;
    public List<InteractiveObject> lootToSpawnAround;
    

    public Vector2 distanceToCutCeilingUnderStairsMinMax = new Vector2(1,5);
    public Vector2Int grindRailsMinMax = new Vector2Int(1, 2);
    public Vector2Int propsPerLevelMinMax = new Vector2Int(1, 10);
    public Vector2Int lootPerLevelMinMax = new Vector2Int(1, 10);
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
    public PhysicMaterial tilePhysicsMaterial;
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
        propsPerLevelMinMax = currentLevel.propsPerLevelMinMax;
        lootPerLevelMinMax = currentLevel.lootPerLevelMinMax;
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
        /*
        if (spawnAdditionalTiles)
            yield return StartCoroutine(SpawnCovers());*/
        
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
        yield return SpawnLoot();
        //yield return StartCoroutine(SpawnGrindRails());
    }


    IEnumerator SpawnNewLevel(int levelIndex)
    {
        float levelY = 0;

        for (int i = 0; i < levelsHeights.Count; i++)
        {
            if (i == levelIndex)
            {
                levelY++;
                break;
            }

            levelY += levelsHeights[i];
        }
        
        Vector3 levelPosition = new Vector3(Random.Range(levelsPosMinMaxX.x, levelsPosMinMaxX.y), levelY, Random.Range(levelsPosMinMaxZ.x, levelsPosMinMaxZ.y));
        
        Vector3Int levelSize = new Vector3Int(Random.Range(levelsScaleMinMaxX.x, levelsScaleMinMaxX.y) * 2,
            levelsHeights[levelIndex],Random.Range(levelsScaleMinMaxZ.x, levelsScaleMinMaxZ.y) * 2);
        
        Quaternion levelRotation = Quaternion.identity;
        if (randomLevelRotation)
            levelRotation = Quaternion.Euler(0, Random.Range(0,360), 0);

        yield return StartCoroutine(SpawnBaseTiles(levelIndex, levelPosition, levelSize, levelRotation));
        //yield return StartCoroutine(SpawnInsideWallsTiles());
        if (levelIndex == 0)
        {
            Player.Movement.rb.MovePosition(levelPosition);
        }
    }

    IEnumerator SpawnBaseTiles(int index, Vector3 pos, Vector3Int size, Quaternion rot)
    {
        Level newLevel = new Level();
        newLevel.position = pos;
        newLevel.size = size;

        GameObject newLevelGameObject = new GameObject("Level " + index);
        newLevelGameObject.transform.parent = generatedBuildingFolder;
        newLevel.spawnedTransform = newLevelGameObject.transform;
        newLevelGameObject.transform.position = pos;
        newLevelGameObject.transform.rotation = rot;

        newLevel.roomTilesMatrix = new TileHealth[size.x,size.y,size.z];
        bool hasRoof = index == levelsHeights.Count - 1;
        

        List<Vector3Int> availableStarPositionsForThinWalls = new List<Vector3Int>();
        
        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.z; z++)
            {
                // FLOOR
                var newFloorTile = Instantiate(tilePrefab, newLevel.spawnedTransform);
                newFloorTile.transform.localRotation = Quaternion.identity;
                newFloorTile.transform.localPosition = new Vector3(x - size.x / 2, 0, z - size.z/2);
                newFloorTile.SetTileRoomCoordinates(new Vector3Int(x,0,z), newLevel);
                newLevel.roomTilesMatrix[x, 0, z] = newFloorTile;
                
                if (hasRoof)
                {
                    // CEILING ON TOP LEVEL
                    var newCeilingTile = Instantiate(tilePrefab, newLevel.spawnedTransform);
                    newCeilingTile.gameObject.name = "CeilingTile coords: " + x + ", " + (size.y - 1) + ", " + z;
                    newCeilingTile.transform.localRotation = Quaternion.identity;
                    newCeilingTile.transform.localPosition = new Vector3(x - size.x / 2, size.y - 1, z - size.z / 2);
                    newCeilingTile.SetTileRoomCoordinates(new Vector3Int(x, size.y - 1, z), newLevel);
                    newLevel.roomTilesMatrix[x, size.y - 1, z] = newCeilingTile;
                }
                
                
                // SPAWN BUILDING'S OUTSIDE WALLS 
                if (x == 0 || x == size.x - 1 || z == 0 || z == size.z - 1) 
                {
                    if (!spawnWalls)
                        continue;
                    
                    newLevel.tilesWalls.Add(newFloorTile);
                    for (int y = 1; y < size.y; y++)
                    {
                        var newWallTile = Instantiate(tileWallPrefab, newLevel.spawnedTransform);
                        newWallTile.transform.localRotation = Quaternion.identity;
                        newWallTile.gameObject.name = "Outside Wall Tile. Coords: " + x +", " + y + ", " + z;
                        newWallTile.SetTileRoomCoordinates(new Vector3Int(x,y,z), newLevel);
                        
                        // ROTATE
                        if (x == size.x - 1)
                            newWallTile.transform.localEulerAngles = new Vector3(0, 180, 0);
                        if (z == 0)
                            newWallTile.transform.localEulerAngles = new Vector3(0, 270, 0);
                        if (z == size.z - 1)
                            newWallTile.transform.localEulerAngles = new Vector3(0, 90, 0);
                            
                        newWallTile.transform.position = newFloorTile.transform.position + Vector3.up * y;
                        newLevel.roomTilesMatrix[x, y, z] = newWallTile;
                        newLevel.tilesWalls.Add(newWallTile);
                        
                        if (y != 1 || x == 0 && z == 0 || x == 0 && z == size.z-1 || x == size.x-1 && z == 0 || x == size.x - 1 && z == size.z-1)
                            continue;
                        
                        availableStarPositionsForThinWalls.Add(new Vector3Int(x,1,z));
                    }
                }
                else // TILES INSIDE ROOMS
                {
                    newLevel.tilesInside.Add(newFloorTile);
                    
                    if (Random.value > 0.95f)
                    {
                        // SPAWN PROPS TILES ON FLOOR
                        
                        var newAdditionalTile = Instantiate(propsPrefabs[Random.Range(0, propsPrefabs.Count)], newLevel.spawnedTransform);
                            
                        ConstructCover(newAdditionalTile.gameObject);
                            
                        newAdditionalTile.transform.localEulerAngles = new Vector3(0, Random.Range(0,360), 0);
                        newAdditionalTile.transform.localPosition = newFloorTile.transform.localPosition + Vector3.up * 0.5f;
                        newAdditionalTile.SetTileRoomCoordinates(new Vector3Int(x,1,z), newLevel);
                        newLevel.roomTilesMatrix[x, 1, z] = newAdditionalTile;
                        newLevel.tilesWalls.Add(newAdditionalTile);
                        
                        /*
                        int r = Random.Range(1, 4);
                        r = 1;
                        for (int i = 1; i <= r; i++)
                        {
                            if (newLevel.roomTilesMatrix[x, i, z] != null)
                                continue;
                            
                            var newAdditionalTile = Instantiate(propsPrefabs[Random.Range(0, propsPrefabs.Count)], newLevel.spawnedTransform);
                            
                            if (i == 1 && r > 1)
                                ConstructCover(newAdditionalTile.gameObject);
                            
                            newAdditionalTile.transform.localRotation = newFloorTile.transform.localRotation;
                            newAdditionalTile.transform.localPosition = newFloorTile.transform.localPosition + Vector3.up * i;
                            newAdditionalTile.SetTileRoomCoordinates(new Vector3Int(x,i,z), newLevel);
                            newLevel.roomTilesMatrix[x, i, z] = newAdditionalTile;
                            newLevel.tilesWalls.Add(newAdditionalTile);
                        }
                        */
                    }
                }
            }
            yield return null;   
        }

        yield return StartCoroutine(SpawnInsideWallsOnLevel(availableStarPositionsForThinWalls, newLevel, hasRoof));
        
        spawnedLevels.Add(newLevel);
    }
    
    IEnumerator SpawnInsideWallsOnLevel(List<Vector3Int> availableStarPositionsForThinWalls, Level level, bool hasRoof)
    {
        List<Vector2Int> RoomsOccupiedTilesPositions = new List<Vector2Int>(); // this will make sure rooms dont intersect
        
        for (int i = 0; i < Random.Range(1, 4); i++) // ROOMS AMOUNT
        {
            // SPAWN INNER ROOMS
            int leftSidePosition = Random.Range(1, level.size.x - 1);
            int rightSidePosition = 0;
            int backSidePosition = Random.Range(1, level.size.z - 1);
            int frontSidePosition = 0;

            if (leftSidePosition < level.size.x / 2)
            {
                rightSidePosition = leftSidePosition + Random.Range(2, level.size.x / 2);
            }
            else
            {
                var tempPos = leftSidePosition - Random.Range(2, level.size.x / 2);
                rightSidePosition = leftSidePosition;
                leftSidePosition = tempPos;
            }

            if (backSidePosition < level.size.z / 2)
            {
                frontSidePosition = backSidePosition + Random.Range(2, level.size.z / 2);
            }
            else
            {
                var tempPos = backSidePosition - Random.Range(2, level.size.z / 2);
                frontSidePosition = backSidePosition;
                backSidePosition = tempPos;
            }

            /*
            leftSidePosition = Mathf.Clamp(leftSidePosition, 1, level.size.x - 1);
            frontSidePosition = Mathf.Clamp(frontSidePosition, 1, level.size.y - 1);
            rightSidePosition = Mathf.Clamp(rightSidePosition, 1, level.size.x - 1);
            backSidePosition = Mathf.Clamp(backSidePosition, 1, level.size.y - 1);
            */

            var roomPrefab = tileWallThinColorPrefabs[Random.Range(0, tileWallThinColorPrefabs.Count)];
            var newRoom = new Room();
            for (int x = leftSidePosition; x <= rightSidePosition; x++)
            {
                for (int z = backSidePosition; z <= frontSidePosition; z++)
                {
                    if (RoomsOccupiedTilesPositions.Contains(new Vector2Int(x,z)))
                        continue;
                    
                    if (x != leftSidePosition && x != rightSidePosition && z != backSidePosition &&
                        z != frontSidePosition)
                    {
                        // IF NOT A TILE FOR A WALL,
                        // MARK THIS EMPTY SPACE AS A ROOM
                        newRoom.coordsInside.Add(new Vector3Int(x, 0, z));
                        RoomsOccupiedTilesPositions.Add(new Vector2Int(x,z));
                        continue;
                    }
                    
                    // NOW SPAWN WALLS AROUND THE ROOM
                    
                    
                    RoomsOccupiedTilesPositions.Add(new Vector2Int(x,z));
                    
                    int buildWallUntillY = level.size.y;
                    if (hasRoof)
                        buildWallUntillY = level.size.y - 1;

                    for (int y = 1; y < buildWallUntillY; y++)
                    {
                        if (level.roomTilesMatrix[x, y, z] != null)
                            continue;
                        
                        var newRoomWallTile = Instantiate(roomPrefab, level.spawnedTransform);
                        newRoomWallTile.transform.localRotation = Quaternion.identity;
                        newRoomWallTile.transform.localPosition = new Vector3(x, y, z) - new Vector3(level.size.x / 2, 0, level.size.z / 2);
                        level.roomTilesMatrix[x, y, z] = newRoomWallTile;

                        var coords = new Vector3Int(x, y, z);
                        newRoomWallTile.SetTileRoomCoordinates(coords, level);
                    }
                }
            }
            
            level.spawnedRooms.Add(newRoom);
        }
        // SPAWN INVISIBLE BLOCKERS FOR RANDOM WALLS
        int[,,] invisibleWallBlockers = new int[level.size.x,level.size.y,level.size.z]; // 0 is free, 1 is block
        Vector3Int roomSize = new Vector3Int(level.size.x / 5, level.size.y, level.size.z / 5);
        Vector3Int roomLocalCoords = new Vector3Int(Random.Range(0, level.size.x / 2), 0, Random.Range(0, level.size.z / 2));
        for (int x = 0; x < roomSize.x; x++)
        {
            for (int z = 0; z < roomSize.z; z++)
            {
                for (int y = 0; y < roomSize.y; y++)
                {
                    invisibleWallBlockers[roomLocalCoords.x + x, y, roomLocalCoords.z + z] = 1;
                }
            }
        }
        // RANDOM WALLS
        int wallsAmount = Random.Range(thinWallsPerLevelMinMax.x, thinWallsPerLevelMinMax.y);
        for (int i = 0; i < wallsAmount; i++)
        {
            var currentWallCoord = availableStarPositionsForThinWalls[Random.Range(0, availableStarPositionsForThinWalls.Count)];
            var prevWallCoord = currentWallCoord;
            availableStarPositionsForThinWalls.Remove(currentWallCoord);
            // пустить крота по y==1

            List<Vector3Int> positionsInThinWall = new List<Vector3Int>(); 
            positionsInThinWall.Add(currentWallCoord);
            int posIndex = 0;
            while (true)
            {
                List<Vector3Int> nextAvailablePositions = new List<Vector3Int>(); 
                if (currentWallCoord.x - 1 >= 0 && level.roomTilesMatrix[currentWallCoord.x - 1, currentWallCoord.y, currentWallCoord.z] == null)
                {
                    var newCoord = new Vector3Int(currentWallCoord.x - 1, currentWallCoord.y, currentWallCoord.z);
                    bool canAdd = true;

                    if (posIndex > 0)
                        canAdd = !HasNeighbourTiles(newCoord, level, currentWallCoord, prevWallCoord);

                    if (invisibleWallBlockers[newCoord.x, 0, newCoord.z] == 1)
                        canAdd = false;
                    
                    if (canAdd)
                        nextAvailablePositions.Add(newCoord);
                }
                if (currentWallCoord.z + 1 < level.size.z && level.roomTilesMatrix[currentWallCoord.x, currentWallCoord.y, currentWallCoord.z + 1] == null)
                {
                    var newCoord = new Vector3Int(currentWallCoord.x, currentWallCoord.y, currentWallCoord.z + 1);
                    bool canAdd = true;
                    
                    if (posIndex > 0)
                        canAdd = !HasNeighbourTiles(newCoord, level, currentWallCoord, prevWallCoord);
                    
                    if (invisibleWallBlockers[newCoord.x, 0, newCoord.z] == 1)
                        canAdd = false;

                    if (canAdd)
                        nextAvailablePositions.Add(newCoord);
                }
                if (currentWallCoord.x + 1 < level.size.x && level.roomTilesMatrix[currentWallCoord.x + 1, currentWallCoord.y, currentWallCoord.z] == null)
                {
                    var newCoord = new Vector3Int(currentWallCoord.x + 1, currentWallCoord.y, currentWallCoord.z);
                    bool canAdd = true;
                    if (posIndex > 0)
                        canAdd = !HasNeighbourTiles(newCoord, level, currentWallCoord, prevWallCoord);
                    
                    if (invisibleWallBlockers[newCoord.x, 0, newCoord.z] == 1)
                        canAdd = false;

                    if (canAdd)
                        nextAvailablePositions.Add(newCoord);
                }
                if (currentWallCoord.z - 1 >= 0 && level.roomTilesMatrix[currentWallCoord.x, currentWallCoord.y, currentWallCoord.z - 1] == null)
                {
                    var newCoord = new Vector3Int(currentWallCoord.x, currentWallCoord.y, currentWallCoord.z - 1);
                    bool canAdd = true;
                    if (posIndex > 0)
                        canAdd = !HasNeighbourTiles(newCoord, level, currentWallCoord, prevWallCoord);
                    
                    if (invisibleWallBlockers[newCoord.x, 0, newCoord.z] == 1)
                        canAdd = false;

                    if (canAdd)
                        nextAvailablePositions.Add(newCoord);
                }

                posIndex++;
                if (nextAvailablePositions.Count <= 0)
                {
                    break;
                }
                var nextPos = nextAvailablePositions[Random.Range(0, nextAvailablePositions.Count)];
                prevWallCoord = currentWallCoord;
                currentWallCoord = nextPos;

                int buildWallUntillY = level.size.y;
                if (hasRoof)
                    buildWallUntillY = level.size.y - 1;
                
                for (int y = 1; y < buildWallUntillY;  y++)
                {
                    var newWallTile = Instantiate(tileWallThinPrefab, level.spawnedTransform);
                    newWallTile.transform.localRotation = Quaternion.identity;
                    newWallTile.transform.localPosition =  new Vector3(nextPos.x - level.size.x / 2, y, nextPos.z - level.size.z/2);
                    level.roomTilesMatrix[nextPos.x, y, nextPos.z] = newWallTile;
                    
                    Debug.Log("thin walls. level.roomTilesMatrix[" + nextPos.x +", " + y +", " + nextPos.z +"]; " + level.roomTilesMatrix[nextPos.x, y, nextPos.z].name +"; newWallTile is " + newWallTile);
                    var coords = new Vector3Int(nextPos.x, y, nextPos.z);
                    newWallTile.SetTileRoomCoordinates(coords, level);
                }
                yield return null;
            }
        }
    }

    bool HasNeighbourTiles(Vector3Int tilePos, Level level, Vector3Int ignorePosAsNeighbour, Vector3Int ignorePrevPosAsNeighbour)
    {        
        if (tilePos.x - 1 >= 0 && level.roomTilesMatrix[tilePos.x - 1, tilePos.y, tilePos.z] != null)
        {
            var v = new Vector3Int(tilePos.x - 1, tilePos.y, tilePos.z);
            if (ignorePosAsNeighbour != v && ignorePrevPosAsNeighbour != v)
                return true;
        }
         
        if (tilePos.x - 1 >= 0 && tilePos.z + 1 < level.size.z && level.roomTilesMatrix[tilePos.x - 1, tilePos.y, tilePos.z + 1] != null)
        {
            var v = new Vector3Int(tilePos.x - 1, tilePos.y, tilePos.z + 1);
            if (ignorePosAsNeighbour != v && ignorePrevPosAsNeighbour != v)
                return true;
        }
        
        if (tilePos.z + 1 < level.size.z && level.roomTilesMatrix[tilePos.x, tilePos.y, tilePos.z + 1] != null)
        {
            var v = new Vector3Int(tilePos.x, tilePos.y, tilePos.z + 1);
            if (ignorePosAsNeighbour != v && ignorePrevPosAsNeighbour != v)
                return true;
        }
        if (tilePos.z + 1 < level.size.z && tilePos.x + 1 < level.size.x && level.roomTilesMatrix[tilePos.x + 1, tilePos.y, tilePos.z + 1] != null)
        {
            var v = new Vector3Int(tilePos.x + 1, tilePos.y, tilePos.z + 1);
            if (ignorePosAsNeighbour != v && ignorePrevPosAsNeighbour != v)
                return true;
        }
        if (tilePos.x + 1 < level.size.x && level.roomTilesMatrix[tilePos.x + 1, tilePos.y, tilePos.z] != null)
        {
            var v = new Vector3Int(tilePos.x + 1, tilePos.y, tilePos.z);
            if (ignorePosAsNeighbour != v && ignorePrevPosAsNeighbour != v)
                return true;
        }
        if (tilePos.z - 1 >= 0 && tilePos.x + 1 < level.size.x && level.roomTilesMatrix[tilePos.x + 1, tilePos.y, tilePos.z - 1] != null)
        {
            var v = new Vector3Int(tilePos.x + 1, tilePos.y, tilePos.z - 1);
            if (ignorePosAsNeighbour !=v && ignorePrevPosAsNeighbour != v)
                return true;
        }
        if (tilePos.z - 1 >= 0 && level.roomTilesMatrix[tilePos.x, tilePos.y, tilePos.z - 1] != null)
        {
            var v = new Vector3Int(tilePos.x, tilePos.y, tilePos.z - 1);
            if (ignorePosAsNeighbour != v && ignorePrevPosAsNeighbour != v)
                return true;
        }
        if (tilePos.x - 1 >= 0 && tilePos.z - 1 >= 0 && level.roomTilesMatrix[tilePos.x - 1, tilePos.y, tilePos.z - 1] != null)
        {
            var v = new Vector3Int(tilePos.x - 1, tilePos.y, tilePos.z - 1);
            if (ignorePosAsNeighbour != v && ignorePrevPosAsNeighbour != v)
                return true;
        }
        
        return false;
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
            float newDistance = Vector3.Distance(new Vector3(levelFromClosestTile.position.x, 0, levelFromClosestTile.position.z),
                new Vector3(levelTo.tilesInside[j].transform.position.x, 0, levelTo.tilesInside[j].transform.position.z));
            if (newDistance >= stairsDistanceMinMax.x && newDistance <= stairsDistanceMinMax.y && newDistance < distance)
            {
                distance = newDistance;
                levelToClosestTile = levelTo.tilesInside[j].transform;
            }
        }

        /*
        for (int j = 0; j < levelTo.tilesInside.Count; j++)
        {
            var tile = levelTo.tilesInside[j].transform;
            if (tile == levelToClosestTile)
                continue;

            if (!Mathf.Approximately(tile.transform.position.x, levelFromClosestTile.position.x) && 
                !Mathf.Approximately(tile.transform.position.z, levelFromClosestTile.position.z))
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
        */

        yield return StartCoroutine(SpawnLadder(levelFromClosestTile.position, levelToClosestTile.position, true, levelFrom.spawnedTransform, 20, levelFromClosestTile, levelToClosestTile));
    }

    public IEnumerator SpawnLadder(Vector3 fromPosition, Vector3 toPosition, bool destroyTilesAround, Transform parent, int maxBridgeTiles = -1, Transform startTile = null, Transform targetTile = null)
    {
        List<Transform> stairsTiles = new List<Transform>();
        
        // SPAWN BRIDGE
        float bridgeTilesAmount = Vector3.Distance(fromPosition, toPosition);
        bridgeTilesAmount = Mathf.CeilToInt(bridgeTilesAmount);

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
            newStairsTile.transform.parent = parent;
            
            // ПОРУЧНИ
            for (int k = 0; k < 2; k++)
            {
                var newStairsTileHandle = Instantiate(tilePrefab, newStairsTile.transform.position, newStairsTile.transform.rotation);
                newStairsTileHandle.transform.parent = newStairsTile.transform;
                float x = 0.546f;
                if (k == 1)
                    x *= -1;
                
                newStairsTileHandle.transform.localPosition = new Vector3(x, 0.898f, 0);
                newStairsTileHandle.transform.localScale = new Vector3(0.3f, 1f, 1);
                stairsTiles.Add(newStairsTileHandle.transform);
            }
            stairsTiles.Add(newStairsTile.transform);
            
            if (destroyTilesAround)
            {
                var hit = Physics.OverlapSphere(newStairsTile.transform.position + Vector3.up, Random.Range(distanceToCutCeilingUnderStairsMinMax.x, distanceToCutCeilingUnderStairsMinMax.y), 1 << 6);
                
                for (int i = 0; i < hit.Length; i++)
                {
                    if (hit[i].transform == null)
                        continue;
                    if (hit[i].transform == targetTile)
                        continue;
                    if (hit[i].transform == startTile)
                        continue;

                    if (newStairsTile == null || stairsTiles.Contains(hit[i].transform) ||
                        hit[i].transform.position.y < newStairsTile.transform.position.y + 1)
                    {
                        continue;
                    }

                    var bodyPart = hit[i].transform.gameObject.GetComponent<TileHealth>();
                    bodyPart.DestroyTileFromGenerator();
                }
            }
        }
        yield return null;
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

    IEnumerator SpawnLoot()
    {
        for (int i = 0; i < spawnedLevels.Count; i++)
        {
            int amount = Random.Range(lootPerLevelMinMax.x, lootPerLevelMinMax.y);
            
            // get all available tiles
            List<TileHealth> tilesForSpawn = new List<TileHealth>();
            for (int x = 0; x < spawnedLevels[i].size.x; x++)
            {
                for (int y = 0; y < spawnedLevels[i].size.y; y++)
                {
                    for (int z = 0; z < spawnedLevels[i].size.z; z++)
                    {
                        if (spawnedLevels[i].roomTilesMatrix[x, y, z] != null)
                        {
                            tilesForSpawn.Add(spawnedLevels[i].roomTilesMatrix[x, y, z]);
                        }
                    }
                }
            }
            
            for (int j = 0; j < amount; j++)
            {
                // choose tile to spawn on
                var randomTile = tilesForSpawn[Random.Range(0, tilesForSpawn.Count)];
                while (randomTile == null)
                {
                    for (int k = tilesForSpawn.Count - 1; k >= 0; k--)
                    {
                        if (k >= tilesForSpawn.Count)
                            continue;

                        if (tilesForSpawn[k] == null)
                            tilesForSpawn.RemoveAt(k);
                    }

                    if (tilesForSpawn.Count <= 0)
                        break;
                    
                    randomTile = tilesForSpawn[Random.Range(0, tilesForSpawn.Count)];
                    yield return null;
                }

                if (randomTile == null)
                    break;
                
                Vector3 randomOffset = Vector3.forward * 0.5f;
                float r = Random.value;
                if (r < 0.1)
                    randomOffset = Vector3.down * 0.5f;
                else if (r < 0.2f)
                    randomOffset = Vector3.left * 0.5f;
                else if (r < 0.3f)
                    randomOffset = Vector3.right * 0.5f;
                else if (r < 0.4f)
                    randomOffset = Vector3.forward * 0.5f;
                else if (r < 0.5f)
                    randomOffset = Vector3.back * 0.5f;
                else
                    randomOffset = Vector3.up * 0.5f;
                Vector3 spawnPos = randomTile.transform.position + randomOffset; 
                var newLoot = Instantiate(lootToSpawnAround[Random.Range(0, lootToSpawnAround.Count)], spawnPos, Quaternion.Euler(Random.Range(0,360),Random.Range(0,360),Random.Range(0,360)));
            }
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

    public void TileDamaged(TileHealth tile)
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

    public void TileDestroyed(Level room, Vector3Int destroyedTileCoords)
    {
        int x = destroyedTileCoords.x;
        int y = destroyedTileCoords.y;
        int z = destroyedTileCoords.z;
        
        /*
        Debug.Log("Tile Destroyed, destroyedTileCoords: " + destroyedTileCoords);
        Debug.Log("TileDestroyed. room.roomTilesMatrix[" + x + ", " + y + ", " + z +"]; " + room.roomTilesMatrix[x, y, z].name);*/
        room.roomTilesMatrix[x, y, z] = null;
        
        for (int YYY = 1; YYY < room.size.y; YYY++)
        {
            //Debug.Log("0; x " + x + "; YYY " + YYY +"; z " + z);
            if (room.roomTilesMatrix[x, YYY, z] != null)
            {
                //Debug.Log("1");
                if (room.roomTilesMatrix[x, YYY - 1, z] != null)
                {
                    //Debug.Log("2");
                    if (YYY-2 >= 0 && room.roomTilesMatrix[x,YYY-2,z] != null)
                        continue;
                }
                if (YYY + 1 < room.size.y && room.roomTilesMatrix[x, YYY + 1, z] != null)
                {
                    //Debug.Log("3");
                    if (YYY + 2 < room.size.y && room.roomTilesMatrix[x,YYY+2,z] != null)
                        continue;
                }

                //Debug.Log("Tile Destroyed AddRigidbody");
                room.roomTilesMatrix[x, YYY, z].AddRigidbody(100, tilePhysicsMaterial);
            }
        }
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
    public TileHealth[,,] roomTilesMatrix;
    public List<TileHealth> tilesInside = new List<TileHealth>();
    public List<TileHealth> tilesWalls = new List<TileHealth>();
    public Transform spawnedTransform;
    public Vector3 position;
    public Vector3Int size;
}

[Serializable]
public class Room
{
    public List<Vector3Int> coordsInside = new List<Vector3Int>();
}
