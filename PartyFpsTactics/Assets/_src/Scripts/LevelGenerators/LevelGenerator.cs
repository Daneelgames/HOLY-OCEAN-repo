using System;
using System.Collections;
using System.Collections.Generic;
using _src.Scripts;
using _src.Scripts.LevelGenerators;
using MrPink;
using MrPink.Health;
using MrPink.PlayerSystem;
using MrPink.Units;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator Instance;
    public enum LevelType{Game,Narrative}

    public LevelType levelType = LevelType.Game;
    public List<Level> spawnedMainBuildingLevels = new List<Level>();
    public List<Level> spawnedAdditionalLevels = new List<Level>();
    public List<TileHealth> spawnedProps = new List<TileHealth>();
    public GameObject levelGoalSpawned;

    public Transform generatedBuildingFolder;
    public Transform disconnectedTilesFolder;
    [Header("SETTINGS")]
    public List<int> mainBuildingLevelsHeights = new List<int>();

    public int additionalSmallBuildingsAmount = 3;
    public Vector2Int additionalBuildingsScaleMinMaxX = new Vector2Int(4, 10);
    public Vector2Int additionalBuildingsScaleMinMaxZ = new Vector2Int(4, 10);
    public Vector2Int additionalBuildingsScaleMinMaxY = new Vector2Int(4, 10);
    public bool spawnWalls = true;
    public bool spawnLadders = true;
    public LayerMask allSolidsLayerMask;

    public BillboardGenerator billboardGeneratorPrefab;
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

    private bool _isLevelReady = false;
    public bool IsLevelReady
    {
        get => _isLevelReady;
        set
        {
            _isLevelReady = value;
            Game.Flags.IsPlayerInputBlocked = !_isLevelReady;
        }
    }

    [UnityEngine.Tooltip("More == buildings levels are more stable")]
    public int islandSupportsScalerToClash = 20;
    private int mainBuildingEntranceSide = 0;
    
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
        if (disconnectedTilesFolder == null)
        {
            disconnectedTilesFolder = new GameObject("DisconnectedTiles").transform;
            disconnectedTilesFolder.position = Vector3.zero;
        }

        switch (levelType)
        {
            case LevelType.Game:
                StartCoroutine(GenerateProcLevel());
                break;
            case LevelType.Narrative:
                
                // choose here what narrative sequence to load?
                // and then set level ready
                
                yield return new WaitForSecondsRealtime(1);
                IsLevelReady = true;
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
        additionalSmallBuildingsAmount = currentLevel.additionalSmallBuildingsAmount;
        additionalBuildingsScaleMinMaxX = currentLevel.additionalBuildingsScaleMinMaxX;
        additionalBuildingsScaleMinMaxY = currentLevel.additionalBuildingsScaleMinMaxY;
        additionalBuildingsScaleMinMaxZ = currentLevel.additionalBuildingsScaleMinMaxZ;
        
        levelGoalPrefab = currentLevel.levelGoalPrefab;
        tilePrefab = currentLevel.tilePrefab;
        tileWallPrefab = currentLevel.tileWallPrefab;
        tileWallThinPrefab = currentLevel.tileWallThinPrefab;
        mainBuildingLevelsHeights = currentLevel.levelsHeights;
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
    }

    public void AddProp(TileHealth prop)
    {
        spawnedProps.Add(prop);
    }
    public void RemoveProp(TileHealth prop)
    {
        spawnedProps.Remove(prop);
    }

    IEnumerator GenerateProcLevel()
    {
        Game.Player.Movement.gameObject.SetActive(false);
        Game.Player.Interactor.cam.gameObject.SetActive(false);
        
        // road is already generated
        //yield return StartCoroutine(RoadGenerator.Instance.GenerateRoadCoroutine());

        yield return null;
        PartyController.Instance.Init(RoadGenerator.Instance.GetPlayerPosOnRoadEnd());
        
        
        // засунуть игрока в машину
        // переместить машину на последний кусок дороги
        
        if (mainBuildingLevelsHeights.Count == 0) // default 5 floors building
        {
            mainBuildingLevelsHeights = new List<int>();
            for (int i = 0; i < 5; i++)
            {
                mainBuildingLevelsHeights.Add(5);
            }
        }

        for (int i = 0; i < mainBuildingLevelsHeights.Count; i++)
        {
            yield return StartCoroutine(SpawnNewBuildingLevel(i));
        }

        for (int i = 0; i < additionalSmallBuildingsAmount; i++)
        {
            yield return StartCoroutine(SpawnNewAdditionalLevel(0));
        }

        yield return StartCoroutine(MakeLadderOnEntrance(spawnedMainBuildingLevels[0]));

        for (int i = 0; i < spawnedAdditionalLevels.Count; i++)
        {
            yield return StartCoroutine(MakeLadderOnEntrance(spawnedAdditionalLevels[i]));
        }
        
        if (spawnLadders)
        {
            for (int i = 0; i < spawnedMainBuildingLevels.Count - 1; i++)
            {
                if (i != 0 && Random.value > 0.66f)
                {
                    yield return StartCoroutine(MakeLaddersBetweenLevels(i));
                }

                yield return StartCoroutine(MakeLaddersBetweenLevels(i));
            }
        }
        
        for (int i = 0; i < spawnedMainBuildingLevels.Count; i++)
        {
            SpawnNavmesh(spawnedMainBuildingLevels[i]);
        }
        
        for (int i = 0; i < spawnedAdditionalLevels.Count; i++)
        {
            SpawnNavmesh(spawnedAdditionalLevels[i]);
        }

        for (int i = 0; i < spawnedMainBuildingLevels.Count; i++)
        {
            spawnedMainBuildingLevels[i].Init();
        }
        for (int i = 0; i < spawnedAdditionalLevels.Count; i++)
        {
            spawnedAdditionalLevels[i].Init();
        }
        for (int i = 0; i < navMeshSurfacesSpawned.Count; i++)
        {
            navMeshSurfacesSpawned[i].BuildNavMesh();
            yield return null;
        }

        StartCoroutine(UpdateNavMesh());
        yield return null;
        Respawner.Instance.Init();
        IsLevelReady = true;
        
        // GOALS
        yield return StartCoroutine(RoomGenerator.Instance.GenerateRooms(spawnedMainBuildingLevels));
        yield return StartCoroutine(SpawnGoals());
        
        yield return StartCoroutine(SpawnExplosiveBarrels());
        yield return SpawnLoot();
        SpawnBillboard();

        //yield return StartCoroutine(SpawnGrindRails());
        
        LevelEventsOnConditions.Instance.Init(ProgressionManager.Instance.CurrentLevel);
        yield break;
    }


    IEnumerator SpawnNewBuildingLevel(int levelIndex)
    {
        float levelY = 0;

        for (int i = 0; i < mainBuildingLevelsHeights.Count; i++)
        {
            if (i == levelIndex)
            {
                //levelY++;
                break;
            }

            levelY += mainBuildingLevelsHeights[i];
        }
        
        Vector3 levelPosition = new Vector3(Random.Range(levelsPosMinMaxX.x, levelsPosMinMaxX.y), levelY, Random.Range(levelsPosMinMaxZ.x, levelsPosMinMaxZ.y));
        /*if (Physics.Raycast(levelPosition + Vector3.up * 1000, Vector3.down, out var hit, Mathf.Infinity,
            GameManager.Instance.AllSolidsMask))
        {
            levelPosition = new Vector3(levelPosition.x, levelPosition.y + hit.point.y, levelPosition.z);
        }*/
        Vector3Int levelSize = new Vector3Int(Random.Range(levelsScaleMinMaxX.x, levelsScaleMinMaxX.y) * 2,
            mainBuildingLevelsHeights[levelIndex],Random.Range(levelsScaleMinMaxZ.x, levelsScaleMinMaxZ.y) * 2);
        
        Quaternion levelRotation = Quaternion.identity;
        if (randomLevelRotation)
            levelRotation = Quaternion.Euler(0, Random.Range(0,360), 0);

        yield return StartCoroutine(SpawnBaseTiles(levelIndex, levelPosition, levelSize, levelRotation, true));
    }
    IEnumerator SpawnNewAdditionalLevel(int levelIndex)
    {
        float levelY = 0;

        Vector3Int levelSize = new Vector3Int(Random.Range(additionalBuildingsScaleMinMaxX.x, additionalBuildingsScaleMinMaxX.y) * 2,
            Random.Range(additionalBuildingsScaleMinMaxY.x, additionalBuildingsScaleMinMaxY.y),Random.Range(additionalBuildingsScaleMinMaxZ.x, additionalBuildingsScaleMinMaxZ.y) * 2);
       
        Vector3 levelPosition =  RandomPosForAdditionalLevel(levelSize);
        int globalInterations = 10;
        bool found = false;
        for (int i = 0; i < globalInterations; i++)
        {
            Vector3 posForLevel = RandomPosForAdditionalLevel(levelSize);

            int tries = 50;
            while (tries > 0)
            {
                if (!Physics.CheckBox(posForLevel + Vector3.up * Mathf.RoundToInt(levelSize.y / 2) + Vector3.up * 2, levelSize + Vector3Int.one, Quaternion.identity, allSolidsLayerMask))
                {
                    levelPosition = posForLevel;
                    found = true;
                    break;
                }

                tries--;
            }
            
            if (found)
                break;
            
            yield return null;
            levelSize = new Vector3Int(Random.Range(additionalBuildingsScaleMinMaxX.x, additionalBuildingsScaleMinMaxX.y) * 2,
                Random.Range(additionalBuildingsScaleMinMaxY.x, additionalBuildingsScaleMinMaxY.y),Random.Range(additionalBuildingsScaleMinMaxZ.x, additionalBuildingsScaleMinMaxZ.y) * 2);
        }
        
        /*
        if (Physics.Raycast(levelPosition + Vector3.up * 1000, Vector3.down, out var hit, Mathf.Infinity,
            GameManager.Instance.AllSolidsMask))
        {
            levelPosition = new Vector3(levelPosition.x, levelPosition.y + hit.point.y, levelPosition.z);
        }
        */
        
        Quaternion levelRotation = Quaternion.identity;
        if (randomLevelRotation)
            levelRotation = Quaternion.Euler(0, Random.Range(0,360), 0);

        yield return StartCoroutine(SpawnBaseTiles(levelIndex, levelPosition, levelSize, levelRotation, false));
    }

    Vector3 RandomPosForAdditionalLevel(Vector3Int additionalLevelSize)
    {
        int x = 0;
        int y = 0;
        int z = 0;
        
        int side = Random.Range(0, 4);
        switch (side)
        {
            case 0: // LEFT
                x = Random.Range(-levelsScaleMinMaxX.y - additionalLevelSize.x * 2 - additionalLevelSize.x, - levelsScaleMinMaxX.y - additionalLevelSize.x * 2);
                z = Mathf.RoundToInt(Random.Range(-levelsScaleMinMaxZ.y - additionalLevelSize.z * 2, levelsScaleMinMaxZ.y + additionalLevelSize.z * 2));
                break;
            case 1: // FWD
                x = Random.Range(-levelsScaleMinMaxX.y - additionalLevelSize.x * 2, levelsScaleMinMaxX.y + additionalLevelSize.x * 2);
                z = Mathf.RoundToInt(Random.Range(levelsScaleMinMaxZ.y + additionalLevelSize.z * 2, levelsScaleMinMaxZ.y + additionalLevelSize.z * 2 + additionalLevelSize.z));
                break;
            case 2: // RIGHT
                x = Random.Range(levelsScaleMinMaxX.y + additionalLevelSize.x * 2, levelsScaleMinMaxX.y + additionalLevelSize.x * 2 + additionalLevelSize.x);
                z = Mathf.RoundToInt(Random.Range(-levelsScaleMinMaxZ.y - additionalLevelSize.z * 2, levelsScaleMinMaxZ.y + additionalLevelSize.z * 2));
                break;
            case 3: // BACK
                x = Random.Range(-levelsScaleMinMaxX.y - additionalLevelSize.x * 2, levelsScaleMinMaxX.y + additionalLevelSize.x * 2);
                z = Mathf.RoundToInt(Random.Range(-levelsScaleMinMaxZ.y - additionalLevelSize.z * 2, -levelsScaleMinMaxZ.y - additionalLevelSize.z * 2 - additionalLevelSize.z));
                break;
        }
        return new Vector3(x, y, z);
    }

    IEnumerator SpawnBaseTiles(int groundLevelIndex, Vector3 pos, Vector3Int size, Quaternion rot, bool mainBuilding)
    {
        GameObject newLevelGameObject = new GameObject();
        Level newLevel = newLevelGameObject.AddComponent<Level>();
        
        if (mainBuilding)
        {
            spawnedMainBuildingLevels.Add(newLevel);

            if (groundLevelIndex == 0)
                newLevel.levelCanClash = false;
        }
        else
        {
            spawnedAdditionalLevels.Add(newLevel);
            newLevel.levelCanClash = false;
        }
        
        newLevel.position = pos;
        newLevel.size = size;
        if (mainBuilding)
        {
            newLevelGameObject.name = "Building Level " + groundLevelIndex;
            newLevelGameObject.transform.parent = generatedBuildingFolder;
        }
        else
        {
            newLevelGameObject.name = "Additional Level";   
        }
        newLevel.spawnedTransform = newLevelGameObject.transform;
        newLevelGameObject.transform.position = pos;
        newLevelGameObject.transform.rotation = rot;
        newLevel.floorWorldHeight = pos.y + 0.5f;

        newLevel.roomTilesMatrix = new TileHealth[size.x,size.y,size.z];
        bool hasRoof = groundLevelIndex == mainBuildingLevelsHeights.Count - 1;

        if (!mainBuilding)
            hasRoof = true;

        int spaceBetweenWindows = Random.Range(2, size.x);
        int currentSpaceBetweenWindows = spaceBetweenWindows;
        Vector2Int windowStartEndY = new Vector2Int(Random.Range(1, size.y/2 -1 ), Random.Range(size.y / 2 + 1, size.y -1 ));
        
        List<Vector3Int> availableStarPositionsForThinWalls = new List<Vector3Int>();

        for (int x = 0; x < size.x; x++)
        {
            for (int z = 0; z < size.z; z++)
            {
                // FLOOR
                var newFloorTile = Instantiate(tilePrefab, newLevel.spawnedTransform);
                newFloorTile.floorLevelTile = true;
                newFloorTile.gameObject.name = "Floor Tile. Coords: " + x +", " + 0 + ", " + z;
                newFloorTile.transform.localRotation = Quaternion.identity;
                newFloorTile.transform.localPosition = new Vector3(x - size.x / 2, 0, z - size.z/2);
                newFloorTile.SetTileRoomCoordinates(new Vector3Int(x,0,z), newLevel);
                newLevel.roomTilesMatrix[x, 0, z] = newFloorTile;
                //newLevel.tilesWalls.Add(newFloorTile);
                newLevel.allTiles.Add(newFloorTile);
                newLevel.tilesFloor.Add(newFloorTile);

                if (groundLevelIndex == 0) // only on very first floor
                {
                    var newFloorObstacle = new GameObject("FloorObstacle");
                    newFloorObstacle.transform.parent = newFloorTile.transform;
                    newFloorObstacle.transform.localPosition = new Vector3(0, -0.5f, 0);
                    var obst = newFloorObstacle.AddComponent<NavMeshObstacle>();
                    obst.carving = true;
                }
                else // on every other floor
                {
                    // TRY TO FIND SUPPORTER TILE
                    if (LevelgenTransforms.SetSupporterTile(spawnedMainBuildingLevels, newFloorTile) == null)
                    {
                        // IF NO SUPPORTER TILE UNDER CORNER TILES -
                        // SPAWN UNDESTRACTIBLE SUPPORT

                        if (x == 0 && z == 0 || x == size.x - 1 && z == 0 || x == 0 && z == size.z - 1 ||
                            x == size.x - 1 && z == size.z - 1)
                        {
                            GameObject newSupport = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            newSupport.name = "STRONG SUPPORT";
                            newSupport.transform.parent = newLevelGameObject.transform;
                            newSupport.transform.localPosition = newFloorTile.transform.localPosition;
                            newSupport.transform.localScale = new Vector3(0.9f, 1000, 0.9f);
                            newSupport.transform.localPosition += Vector3.down * 500.5f;
                            var mesh = newSupport.GetComponent<MeshRenderer>();
                            mesh.material = GameManager.Instance.rockDefaultMaterial;
                            newSupport.layer = 12;
                            var obstacle = newSupport.AddComponent<NavMeshObstacle>();
                            obstacle.carving = true;
                            var tileHealth = newSupport.AddComponent<TileHealth>();
                            tileHealth.ImmuneToDamage = true;
                            newFloorTile.supporterTile = tileHealth;
                            tileHealth.supportedTile = newFloorTile;
                        }
                    }
                }
                
                // SPAWN BUILDING'S OUTSIDE WALLS 
                
                
                if (x == 0 || x == size.x - 1 || z == 0 || z == size.z - 1) 
                {
                    if (!spawnWalls)
                        continue;

                    bool windowHere = false;

                    if (currentSpaceBetweenWindows <= 0)
                    {
                        windowHere = true;
                        currentSpaceBetweenWindows = spaceBetweenWindows;
                    }
                    
                    currentSpaceBetweenWindows--;
                    for (int y = 1; y < size.y; y++)
                    {
                        bool cornerTile = x == 0 && z == 0 || x == 0 && z == size.z - 1 ||
                                          x == size.x - 1 && z == 0 || x == size.x - 1 && z == size.z - 1;

                        if (cornerTile)
                            windowHere = false;
                        
                        if (windowHere)
                        {
                            if (y > windowStartEndY.x && y < windowStartEndY.y)
                                continue;
                        }
                        
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
                        newLevel.allTiles.Add(newWallTile);

                        if (groundLevelIndex < mainBuildingLevelsHeights.Count - 1 && y == size.y - 1)
                            newLevel.tilesTop.Add(newWallTile);
                        
                        if (cornerTile || y != 1)
                            continue;
                        
                        StartCoroutine(ConstructCover(newWallTile.gameObject, 3));
                        if (!windowHere)
                            availableStarPositionsForThinWalls.Add(new Vector3Int(x,1,z));
                    }
                }
                else // TILES INSIDE 
                {
                    newLevel.tilesInside.Add(newFloorTile);
                    newLevel.allTiles.Add(newFloorTile);
                    
                    if (hasRoof)
                    {
                        // CEILING ON TOP LEVEL
                        var newCeilingTile = Instantiate(tilePrefab, newLevel.spawnedTransform);
                        newCeilingTile.gameObject.name = "CeilingTile coords: " + x + ", " + (size.y - 1) + ", " + z;
                        newCeilingTile.transform.localRotation = Quaternion.identity;
                        newCeilingTile.transform.localPosition = new Vector3(x - size.x / 2, size.y - 1, z - size.z / 2);
                        newCeilingTile.SetTileRoomCoordinates(new Vector3Int(x, size.y - 1, z), newLevel);
                        newCeilingTile.ceilingLevelTile = true;
                        newLevel.roomTilesMatrix[x, size.y - 1, z] = newCeilingTile;
                        newLevel.allTiles.Add(newCeilingTile);
                        //newLevel.tilesInside.Add(newCeilingTile);
                        newLevel.tilesTop.Add(newFloorTile);
                    }

                    if (Random.value > 0.95f)
                    {
                        // SPAWN PROPS TILES ON FLOOR
                        
                        var newAdditionalTile = Instantiate(propsPrefabs[Random.Range(0, propsPrefabs.Count)], newLevel.spawnedTransform);
                            
                        StartCoroutine(ConstructCover(newAdditionalTile.gameObject, 0));
                            
                        newAdditionalTile.transform.localEulerAngles = new Vector3(0, Random.Range(0,360), 0);
                        newAdditionalTile.transform.localPosition = newFloorTile.transform.localPosition + Vector3.up * 0.5f;
                        newAdditionalTile.SetTileRoomCoordinates(new Vector3Int(x,1,z), newLevel);
                        newLevel.roomTilesMatrix[x, 1, z] = newAdditionalTile;
                        /*newLevel.tilesInside.Add(newAdditionalTile);
                        newLevel.allTiles.Add(newAdditionalTile);*/
                    }
                }
            }
            yield return null;   
        }

        yield return StartCoroutine(SpawnInsideWallsOnLevel(availableStarPositionsForThinWalls, newLevel, hasRoof));
    }
    
    IEnumerator SpawnInsideWallsOnLevel(List<Vector3Int> availableStarPositionsForThinWalls, Level level, bool hasRoof)
    {
        if (availableStarPositionsForThinWalls.Count <= 0)
            yield break;
        
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

            var thinColorPrefab = tileWallThinColorPrefabs[Random.Range(0, tileWallThinColorPrefabs.Count)];
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
                        
                        var newRoomWallTile = Instantiate(thinColorPrefab, level.spawnedTransform);
                        newRoomWallTile.transform.localRotation = Quaternion.identity;
                        newRoomWallTile.transform.localPosition = new Vector3(x, y, z) - new Vector3(level.size.x / 2, 0, level.size.z / 2);
                        level.roomTilesMatrix[x, y, z] = newRoomWallTile;
                        //level.tilesInside.Add(newRoomWallTile);
                        level.allTiles.Add(newRoomWallTile);
                        if (y == buildWallUntillY - 1)
                            level.tilesTop.Add(newRoomWallTile);

                        var coords = new Vector3Int(x, y, z);
                        newRoomWallTile.SetTileRoomCoordinates(coords, level);
                        
                        if (y != 1)
                            continue;
                        
                        StartCoroutine(ConstructCover(newRoomWallTile.gameObject, 3));
                    }
                }
            }
            
            level.spawnedRooms.Add(newRoom);
            yield return null;
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
                    level.allTiles.Add(newWallTile);
                    if (y == buildWallUntillY - 1)
                        level.tilesTop.Add(newWallTile);
                    
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


    IEnumerator MakeLaddersBetweenLevels(int i)
    {
        Level levelFrom = spawnedMainBuildingLevels[i];
        Level levelTo = spawnedMainBuildingLevels[i + 1];

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

        yield return StartCoroutine(SpawnLadder(levelFromClosestTile.position, levelToClosestTile.position, true, levelFrom.spawnedTransform, 20, levelFromClosestTile, levelToClosestTile));
    }

    IEnumerator MakeLadderOnEntrance(Level level)
    {
        Vector3 fromPosition = Vector3.zero;
        Vector3 toPosition = Vector3.zero;
        Transform targetTileToConnect = null;

        int randomSide = Random.Range(0, 4); // 0 - left, 1 - front, 2 - right, 3 - back
        Vector3 offsetVector = Vector3.zero;

        if (spawnedMainBuildingLevels[0] == level)
            mainBuildingEntranceSide = randomSide;
        
        
        switch (randomSide)
        {
            case 0: // LEFT
                var tile = level.roomTilesMatrix[0, 0, Random.Range(0, level.size.z)];
                targetTileToConnect = tile.transform;
                offsetVector = Vector3.left;
                break;
            case 1: // FRONT
                var tileF = level.roomTilesMatrix[Random.Range(0, level.size.x), 0, level.size.z - 1];
                targetTileToConnect = tileF.transform;
                offsetVector = Vector3.forward;
                break;
            case 2: // RIGHT
                var tileR = level.roomTilesMatrix[level.size.x - 1, 0, Random.Range(0, level.size.z)];
                targetTileToConnect = tileR.transform;
                offsetVector = Vector3.right;
                break;
            case 3: // BACK
                var tileB = level.roomTilesMatrix[Random.Range(0, level.size.x), 0, 0];
                targetTileToConnect = tileB.transform;
                offsetVector = Vector3.back;
                break;
        }

        fromPosition = new Vector3(targetTileToConnect.position.x, 0, targetTileToConnect.position.z) + offsetVector * 5;  
        toPosition = targetTileToConnect.position;
        
        yield return StartCoroutine(SpawnLadder(fromPosition, toPosition, true, level.spawnedTransform, 20, null, targetTileToConnect));
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
                var hit = Physics.OverlapSphere(newStairsTile.transform.position + Vector3.up, Random.Range(distanceToCutCeilingUnderStairsMinMax.x, distanceToCutCeilingUnderStairsMinMax.y), allSolidsLayerMask);
                
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
                    if (bodyPart)
                        bodyPart.DestroyTileFromGenerator();
                }
            }
            yield return null;
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
            var randomLevel = spawnedMainBuildingLevels[Random.Range(1, spawnedMainBuildingLevels.Count)];
            var randomTile = randomLevel.tilesInside[Random.Range(0, randomLevel.tilesInside.Count)];

            Vector3 pos = randomTile.transform.position + Vector3.up;
            randomLevel.tilesInside.Remove(randomTile);
            Instantiate(explosiveBarrelPrefab, pos, Quaternion.identity);
            yield return null;
        }
    }

    IEnumerator SpawnLoot()
    {
        for (int i = 0; i < spawnedMainBuildingLevels.Count; i++)
        {
            int amount = Random.Range(lootPerLevelMinMax.x, lootPerLevelMinMax.y);
            
            // get all available tiles
            List<TileHealth> tilesForSpawn = new List<TileHealth>();
            
            if (i == 0) // first floor, spread stuff through all additionalBuidlings
            {
                amount += additionalSmallBuildingsAmount;
                for (int j = 0; j < spawnedAdditionalLevels.Count; j++)
                {
                    for (int x = 0; x < spawnedAdditionalLevels[i].size.x; x++)
                    {
                        for (int y = 0; y < spawnedAdditionalLevels[i].size.y; y++)
                        {
                            for (int z = 0; z < spawnedAdditionalLevels[i].size.z; z++)
                            {
                                if (spawnedAdditionalLevels[i].roomTilesMatrix[x, y, z] != null)
                                {
                                    tilesForSpawn.Add(spawnedAdditionalLevels[i].roomTilesMatrix[x, y, z]);
                                }
                            }
                        }
                    }   
                }
            }
            
            for (int x = 0; x < spawnedMainBuildingLevels[i].size.x; x++)
            {
                for (int y = 0; y < spawnedMainBuildingLevels[i].size.y; y++)
                {
                    for (int z = 0; z < spawnedMainBuildingLevels[i].size.z; z++)
                    {
                        if (spawnedMainBuildingLevels[i].roomTilesMatrix[x, y, z] != null)
                        {
                            tilesForSpawn.Add(spawnedMainBuildingLevels[i].roomTilesMatrix[x, y, z]);
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

    void SpawnBillboard()
    {
        // fix later
        return;
        
        if (spawnedMainBuildingLevels.Count <= 3)
            return;
        
        var newBillboard = Instantiate(billboardGeneratorPrefab);
        var randomLevel = spawnedMainBuildingLevels[Random.Range(2, spawnedMainBuildingLevels.Count - 1)];
        float yRot = 0;
        int wallSize = 0;
        Vector3 billboardPos = Vector3.zero;
        
        switch (mainBuildingEntranceSide)
        {
            case 0: // LEFT
                billboardPos = randomLevel.position + Vector3.left + Vector3.left + Vector3.left * randomLevel.size.x / 2;
                yRot = 270;
                wallSize = randomLevel.size.z;
                break;
            case 1: // FWD
                billboardPos = randomLevel.position + Vector3.forward * randomLevel.size.z / 2;
                yRot = 0;
                wallSize = randomLevel.size.x;
                break;
            case 2: // RIGHT
                billboardPos = randomLevel.position + Vector3.right * randomLevel.size.z / 2;
                yRot = 90;
                wallSize = randomLevel.size.z;
                break;
            case 3: // BACK
                billboardPos = randomLevel.position + Vector3.back + Vector3.back + Vector3.back * randomLevel.size.x / 2 ;
                yRot = 180;
                wallSize = randomLevel.size.x;
                break;
        }
        newBillboard.GenerateBillboard(wallSize, billboardPos,yRot);
    }
    
    IEnumerator SpawnGrindRails() // соединять главное здание с дополнительными
    {
        for (int j = 0; j < Random.Range(grindRailsMinMax.x, grindRailsMinMax.y); j++)
        {
            var randomLevel = spawnedMainBuildingLevels[Random.Range(0,spawnedMainBuildingLevels.Count)];
            var randomTile = randomLevel.tilesInside[Random.Range(0, randomLevel.tilesInside.Count)];

            Vector3 pos = randomTile.transform.position + Vector3.up;
            randomLevel.tilesInside.Remove(randomTile);
            var grindRails = Instantiate(grindRailsPrefab, pos, Quaternion.identity);
            grindRails.GenerateNodes(true);
            yield return null;
        }
    }

    IEnumerator SpawnGoals()
    {
        Vector3 spawnPosition = spawnedMainBuildingLevels[spawnedMainBuildingLevels.Count - 1].position + Vector3.up * 2;
        levelGoalSpawned = Instantiate(levelGoalPrefab, spawnPosition, Quaternion.identity);
        for (int i = 0; i < spawnedMainBuildingLevels.Count; i++)
        {
            for (int j = 0; j < spawnedMainBuildingLevels[i].spawnedRooms.Count; j++)
            {
                var room = spawnedMainBuildingLevels[i].spawnedRooms[j];

                yield return null;                
            }

            yield return null;
        }
    }

    public void TileDamagedFeedback(TileHealth tile)
    {
        if (tilesToDamage.Contains(tile.transform))
            return;
        
        StartCoroutine(TileDamagedFeedbackCoroutine(tile.transform));
    }
    public void TileDamagedFeedback(Transform tile)
    {
        if (tilesToDamage.Contains(tile))
            return;
        
        StartCoroutine(TileDamagedFeedbackCoroutine(tile));
    }

    public void TileDestroyed(Level level, Vector3Int destroyedTileCoords)
    {
        int x = destroyedTileCoords.x;
        int y = destroyedTileCoords.y;
        int z = destroyedTileCoords.z;
        
        /*
        Debug.Log("Tile Destroyed, destroyedTileCoords: " + destroyedTileCoords);
        Debug.Log("TileDestroyed. room.roomTilesMatrix[" + x + ", " + y + ", " + z +"]; " + room.roomTilesMatrix[x, y, z].name);*/
        level.roomTilesMatrix[x, y, z] = null;
        
        // check neighbours
        for (int YYY = 1; YYY < level.size.y; YYY++)
        {
            //Debug.Log("0; x " + x + "; YYY " + YYY +"; z " + z);
            var tile = level.roomTilesMatrix[x, YYY, z];
            if (tile != null)
            {
                if (tile.ceilingLevelTile)
                    continue;
                //Debug.Log("1");
                if (level.roomTilesMatrix[x, YYY - 1, z] != null)
                {
                        continue;
                }
                if (YYY + 1 < level.size.y && level.roomTilesMatrix[x, YYY + 1, z] != null)
                {
                        continue;
                }

                //Debug.Log("Tile Destroyed AddRigidbody");
                UnitsManager.Instance.RagdollTileExplosion(tile.transform.position);
                tile.ActivateRigidbody(100, tilePhysicsMaterial);
                level.roomTilesMatrix[x, YYY, z] = null;
            }
        }
    }

    //IEnumerator CheckTilesToFall
    
    private List<Transform> tilesToDamage = new List<Transform>();

    IEnumerator TileDamagedFeedbackCoroutine(Transform tile)
    {
        if (tile.gameObject.isStatic)
            yield break;
        
        float t = 0;
        tilesToDamage.Add(tile);
        Vector3 originalLocalPosition = tile.localPosition;
        Quaternion originalLocalRot = tile.localRotation;
        
        while (t < 0.5f)
        {
            if (tile == null)
                yield break;
            
            t += Time.deltaTime;
            tile.localPosition = originalLocalPosition + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f),
                Random.Range(-0.1f, 0.1f));
            yield return null;
        }

        if (tile)
        {
            tile.localPosition = originalLocalPosition;
            tile.localRotation = originalLocalRot;
            tilesToDamage.Remove(tile);
        }
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
                
                yield return new WaitForSeconds(1f);
            }
        }
    }
    

    IEnumerator ConstructCover(GameObject newCoverGo, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        if (newCoverGo == null)
            yield break; 
                
        var newCover = newCoverGo.gameObject.AddComponent<Cover>();
        newCover.ConstructSpots();
    }

    public void AddToDisconnectedTilesFolder(Transform t)
    {
        t.parent = disconnectedTilesFolder;
    }
}

[Serializable]
public class Room
{
    public List<Vector3Int> coordsInside = new List<Vector3Int>();
}

public static class LevelgenTransforms
{
    public static TileHealth SetSupporterTile(List<Level> levels, TileHealth tile)
    {
        if (tile == null)
            return null;

        if (Physics.Raycast(tile.transform.position, Vector3.down, out var hit, 1,  GameManager.Instance.AllSolidsMask))
        {
            TileHealth supporter = hit.transform.gameObject.GetComponent<TileHealth>();
            if (supporter && supporter != tile)
            {
                tile.supporterTile = supporter;
                supporter.supportedTile = tile;
            }
        }
        
        return null;
    }

    
    // OBSOLETE
    public static Vector3Int ConvertTilePositionToLocalLevelCoords(Level levelTop, Level levelBottom)
    {
        int tileSize = 1;
        
        // найти дистанции по отдельным осям между нулевыми тайлами обоих этажей
        var topTilePosition = GetZeroPosition(levelTop, tileSize);
        var bottomTilePosition = GetZeroPosition(levelBottom, tileSize);
        
        int x = GetOffsetOfTiles(topTilePosition.x, bottomTilePosition.x, 1);
        int y = levelBottom.size.y;
        int z = GetOffsetOfTiles(topTilePosition.z, bottomTilePosition.z, 1);

        return new Vector3Int(x, y, z);
    }

    private static Vector3 GetZeroPosition(Level level, int tileSize)
        => level.position - level.size/2*tileSize;
    
    private static int GetOffsetOfTiles(float positionA, float positionB, float size)
        => Mathf.RoundToInt(Mathf.Abs(positionB - positionA) / size);
}