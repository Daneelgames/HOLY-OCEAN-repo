using System;
using System.Collections;
using System.Collections.Generic;
using _src.Scripts;
using _src.Scripts.LevelGenerators;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using MrPink;
using MrPink.Health;
using MrPink.PlayerSystem;
using MrPink.Units;
using Sirenix.OdinInspector;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class BuildingGenerator : NetworkBehaviour
{
    public List<Building> spawnedBuildings = new List<Building>();
    [HideInInspector]
    public List<TileHealth> spawnedProps = new List<TileHealth>();

    public Transform generatedBuildingFolder;
    public Transform disconnectedTilesFolder;

    [Header("GENERATE IN SINGLE FRAME OR OVER TIME")]
    [SerializeField]private bool singleFrame = false;
    [Header("SETTINGS")]
    //public List<int> mainBuildingLevelsHeights = new List<int>();
    public List<BuildingSettings> buildingsToSpawnSettings = new List<BuildingSettings>();
    
    public bool spawnGoals = true;
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
    public List<TileHealth> PropsPrefabs => propsPrefabs;
    public List<InteractiveObject> lootToSpawnAround;

    public Vector2 distanceToCutCeilingUnderStairsMinMax = new Vector2(1,5);
    public Vector2Int grindRailsMinMax = new Vector2Int(1, 2);
    public Vector2Int lootPerLevelMinMax = new Vector2Int(1, 10);
    public Vector2Int stairsDistanceMinMax = new Vector2Int(5, 10);
    public Vector2Int thinWallsPerLevelMinMax = new Vector2Int(1, 10);
    
    public bool randomLevelRotation = false;
    public int explosiveBarrelsAmount = 2;

    [Space] [Header("NAVIGATION")] 
    public Transform navMeshesParent;
    public NavMeshSurface navMeshSurfacePrefab;
    public List<NavMeshSurface> navMeshSurfacesSpawned;
    public float updateNavMeshDistance = 100;
    public GameObject tileDestroyedParticles;
    public PhysicMaterial tilePhysicsMaterial;

    private bool generated = false;
    public bool Generated => generated;
    
    [Serializable]
    public class Building
    {
        public Vector3 worldPos;
        public List<Level> spawnedBuildingLevels = new List<Level>();
        public int localEntranceSide;
    }

    [Tooltip("More == buildings levels are more stable")]
    public int islandSupportsScalerToClash = 20;

    private Island ownIsland;


    public override void OnStartClient()
    {
        base.OnStartClient();

        IslandSpawner.Instance.AddTileBuilding(this);
    }

    private void OnDestroy()
    {
        IslandSpawner.Instance.RemoveTileBuilding(this);
    }

    private int currentSeed;
    public void InitOnClient(int seed, Island island)
    {
        ownIsland = island;
        currentSeed = seed;
        Random.InitState(currentSeed);
        Debug.Log("BUILDING GENERATOR START GENERATING WITH SEED " + currentSeed +"; Random.state " + Random.state + "; hashCode " + Random.state.GetHashCode());

        #region depricated - get building data from ProgressionManager

        /*
        var currentLevel = ProgressionManager.Instance.CurrentLevel;

        for (int i = 0; i < buildingsToSpawnSettings.Count; i++)
        {
            buildingsToSpawnSettings[i].levelsSettings = new List<BuildingSettings.LevelSetting>(currentLevel.BuildingSettings.levelsSettings);
            buildingsToSpawnSettings[i].spawnLadders = currentLevel.BuildingSettings.spawnLadders;
            buildingsToSpawnSettings[i].spawnLoot = currentLevel.BuildingSettings.spawnLoot;
            buildingsToSpawnSettings[i].spawnProps = currentLevel.BuildingSettings.spawnProps;
            buildingsToSpawnSettings[i].spawnRooms = currentLevel.BuildingSettings.spawnRooms;
            buildingsToSpawnSettings[i].spawnUnits = currentLevel.BuildingSettings.spawnUnits;
            buildingsToSpawnSettings[i].spawnNavMesh = currentLevel.BuildingSettings.spawnNavMesh;
            buildingsToSpawnSettings[i].levelsScaleMinMaxX = currentLevel.BuildingSettings.levelsScaleMinMaxX;
            buildingsToSpawnSettings[i].levelsScaleMinMaxZ = currentLevel.BuildingSettings.levelsScaleMinMaxZ;

            currentLevel = ProgressionManager.Instance.RandomLevel;

        }
        //buildingsToSpawnSettings[0] = currentLevel.BuildingSettings;
        
        levelGoalPrefab = currentLevel.levelGoalPrefab;
        tilePrefab = currentLevel.tilePrefab;
        tileWallPrefab = currentLevel.tileWallPrefab;
        tileWallThinPrefab = currentLevel.tileWallThinPrefab;
        explosiveBarrelsAmount = currentLevel.explosiveBarrelsAmount;
        explosiveBarrelPrefab = currentLevel.explosiveBarrelPrefab;
        lootPerLevelMinMax = currentLevel.lootPerLevelMinMax;
        grindRailsMinMax = currentLevel.grindRailsPerLevelMinMax;
        grindRailsPrefab = currentLevel.grindRailsPrefab;
        stairsDistanceMinMax = currentLevel.stairsDistanceMinMax;
        thinWallsPerLevelMinMax = currentLevel.thinWallsPerLevelMinMax;
        distanceToCutCeilingUnderStairsMinMax = currentLevel.distanceToCutCeilingUnderStairsMinMax;
        spawnWalls = currentLevel.spawnWalls;
        spawnLadders = currentLevel.spawnLadders;
        */


        #endregion
        
        if (generatedBuildingFolder == null)
        {
            generatedBuildingFolder = new GameObject("GeneratedBuilding").transform;
        }
        generatedBuildingFolder.position = Vector3.zero;
        generatedBuildingFolder.parent = transform;
        if (disconnectedTilesFolder == null)
        {
            disconnectedTilesFolder = new GameObject("DisconnectedTiles").transform;
        }
        disconnectedTilesFolder.position = Vector3.zero;
        disconnectedTilesFolder.parent = transform;

        //StartCoroutine(PartyController.Instance.Init());


        if (buildingsToSpawnSettings.Count > 0 && buildingsToSpawnSettings[0].BuildingOriginTransform)
            StartCoroutine(SpawnBuilding(buildingsToSpawnSettings[0].BuildingOriginTransform.position));
    }

    public void AddProp(TileHealth prop)
    {
        if (spawnedProps.Contains(prop))
            return;

        prop.transform.parent = generatedBuildingFolder;
        spawnedProps.Add(prop);
    }
    public void RemoveProp(TileHealth prop)
    {
        if (!spawnedProps.Contains(prop))
            return;
        spawnedProps.Remove(prop);
    }

    IEnumerator SpawnBuilding(Vector3 buildingPos)
    {
        if (GameManager.Instance.GetLevelType != GameManager.LevelType.Game)
        {
            Debug.LogError("DONT SPAWN BUILDING ON THIS LEVEL TYPE");
            yield break;
        }

        if (spawnedBuildings.Count > buildingsToSpawnSettings.Count)
            yield break;

        Building newBuilding = new Building();
        newBuilding.worldPos = buildingPos;
        spawnedBuildings.Add(newBuilding);
        
        Game._instance.SetLevelGeneratingFeedback(true);
        
        StartCoroutine(SpawnBuilding(newBuilding));
    }

    IEnumerator SpawnBuilding(Building building)
    {
        generated = false;
        //var buildingSettings = buildingsToSpawnSettings[Random.Range(0, buildingsToSpawnSettings.Count)];
        var buildingSettings = buildingsToSpawnSettings[0];
        buildingSettings.BuildingOriginTransform.position = building.worldPos;
        for (int j = 0; j < buildingSettings.levelsSettings.Count; j++)
        {
            yield return StartCoroutine(SpawnNewBuildingLevel(building, j, buildingSettings));
        }
        
        
        for (int i = 0; i < building.spawnedBuildingLevels.Count; i++)
        {
            if (building.spawnedBuildingLevels[i].firstFloor)
                yield return StartCoroutine(MakeLadderOnEntrance(building, building.spawnedBuildingLevels[0]));   
                
            if (i != 0 && i%2 == 1) // if not first and if even
            {
                yield return StartCoroutine(MakeLaddersBetweenLevels(building, i));
            }

            yield return StartCoroutine(MakeLaddersBetweenLevels(building, i));
            
            SpawnNavmesh(building.spawnedBuildingLevels[i]);
            
            building.spawnedBuildingLevels[i].Init();
        }

        var roomsGenerator = gameObject.GetComponent<RoomGenerator>();
        if (roomsGenerator)
            yield return StartCoroutine(roomsGenerator.GenerateRooms(building.spawnedBuildingLevels, singleFrame));
        
        DestroyTilesForLadders();
        
        if (base.IsHost)
        {
            if (spawnGoals)
                yield return StartCoroutine(SpawnGoalsOnServer(building));
            
            StartCoroutine(UpdateNavMesh());
        }

        ownIsland.BuildingGenerated();
        generated = true;
        
        if (base.IsHost)
        {
            yield return StartCoroutine(SpawnPropsOnServer(building));
            yield return StartCoroutine(SpawnExplosiveBarrelsOnServer(building));
        }
        
        Game._instance.SetLevelGeneratingFeedback(false);
        
        //yield return SpawnLoot(building);
        
        //ContentPlacer.Instance.SpawnEnemiesInBuilding(building);
    }


    IEnumerator SpawnNewBuildingLevel(Building building, int levelIndexInBuilding,  BuildingSettings buildingSettings)
    {
        var levelHeights = buildingSettings.levelsSettings;
        var buildingOrigin = buildingSettings.BuildingOriginTransform;
        
        float levelY = 0;
        levelY = buildingOrigin.position.y;
        
        for (int i = 0; i < levelHeights.Count; i++)
        {
            if (i == levelIndexInBuilding)
            {
                //levelY++;
                break;
            }

            levelY += levelHeights[i].levelHeight;
        }
        
        Debug.Log("BUILDING GENERATOR CURRENT state " + Random.state + "; hashCode " + Random.state.GetHashCode());
        
        Random.InitState(currentSeed);
        var x = Random.Range(buildingSettings.offsetPosMinMaxX.x - levelIndexInBuilding,
            buildingSettings.offsetPosMinMaxX.y + levelIndexInBuilding);
        Random.InitState(currentSeed + levelIndexInBuilding);
        var z = Random.Range(buildingSettings.offsetPosMinMaxZ.x - levelIndexInBuilding,
            buildingSettings.offsetPosMinMaxZ.y + levelIndexInBuilding);
        Vector3 levelPosition = new Vector3(buildingOrigin.position.x + x, levelY,
            buildingOrigin.position.z + z);
        
        Random.InitState(currentSeed);
        var randomSizeX = Mathf.RoundToInt(Random.Range(buildingSettings.levelsScaleMinMaxX.x * 1f, buildingSettings.levelsScaleMinMaxX.y * 1f));
        Random.InitState(currentSeed + levelIndexInBuilding);
        var randomSizeZ = Mathf.RoundToInt(Random.Range(buildingSettings.levelsScaleMinMaxZ.x * 1f, buildingSettings.levelsScaleMinMaxZ.y * 1f));
        
        Random.InitState(currentSeed);
        randomSizeX += Random.Range(-randomSizeX / 5 * levelIndexInBuilding, randomSizeX / 5 * levelIndexInBuilding);
        Random.InitState(currentSeed + levelIndexInBuilding);
        randomSizeZ += Random.Range(-randomSizeZ / 5 * levelIndexInBuilding, randomSizeZ / 5 * levelIndexInBuilding);
        
        Vector3Int levelSize = new Vector3Int( randomSizeX * 2, levelHeights[levelIndexInBuilding].levelHeight,randomSizeZ * 2);
        Debug.Log("BUILDING GENERATOR LEVEL SIZE " + levelSize + "; randomSizeX " + randomSizeX  +"; randomSizeZ " + randomSizeZ + "; buildingSettings.levelsScaleMinMaxX " + buildingSettings.levelsScaleMinMaxX + "; buildingSettings.levelsScaleMinMaxZ" + buildingSettings.levelsScaleMinMaxZ);
        Quaternion levelRotation = Quaternion.identity;
        if (randomLevelRotation)
        {
            Random.InitState(currentSeed + levelIndexInBuilding);
            levelRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        }

        ClearPlaceForBuilding(levelPosition, levelRotation, levelSize);
        
        yield return StartCoroutine(SpawnBaseTiles(building, levelIndexInBuilding, levelPosition, levelSize, levelRotation, levelHeights, buildingSettings));
    }

    void ClearPlaceForLadder(Vector3 startPos, Vector3 endPos)
    {
        var levelCutDummy = GameObject.CreatePrimitive(PrimitiveType.Cube);
        
        levelCutDummy.transform.position = Vector3.Lerp(startPos, endPos, 0.5f) + Vector3.up;
        levelCutDummy.transform.rotation = Quaternion.LookRotation((endPos - startPos).normalized);
        levelCutDummy.transform.localScale = new Vector3(2,2,Vector3.Distance(startPos,endPos));
        levelCutDummy.layer = 2; // ignore raycast
        var collider = levelCutDummy.GetComponent<Collider>();
        collider.isTrigger = true;
        ownIsland.AddRoomCutter(levelCutDummy);
    }
    void ClearPlaceForBuilding(Vector3 levelPos, Quaternion levelRot, Vector3Int levelSize)
    {
        var levelCutDummy = GameObject.CreatePrimitive(PrimitiveType.Cube);
        
        levelCutDummy.transform.position = levelPos + Vector3.up * levelSize.y/2;
        levelCutDummy.transform.rotation = levelRot;
        levelCutDummy.transform.localScale = levelSize;
        levelCutDummy.layer = 2; // ignore raycast
        var collider = levelCutDummy.GetComponent<Collider>();
        collider.isTrigger = true;
        ownIsland.AddRoomCutter(levelCutDummy);
    }
    
    //. this method spawns building level "bounds"
    IEnumerator SpawnBaseTiles(Building building, int levelIndexInBuilding, Vector3 pos, Vector3Int size, Quaternion rot, List<BuildingSettings.LevelSetting> levelSettings, BuildingSettings buildingSettings)
    {
        GameObject newLevelGameObject = new GameObject();
        Level newLevel = newLevelGameObject.AddComponent<Level>();
        newLevel.SetBuildingSettings(buildingSettings, levelIndexInBuilding);
        building.spawnedBuildingLevels.Add(newLevel);

        if (levelIndexInBuilding == 0)
            newLevel.firstFloor = true;
        
        newLevel.position = pos;
        newLevel.size = size;
        
        newLevelGameObject.name = "Building Level " + levelIndexInBuilding;
        newLevelGameObject.transform.parent = generatedBuildingFolder;
        newLevel.spawnedTransform = newLevelGameObject.transform;
        newLevelGameObject.transform.position = pos;
        newLevelGameObject.transform.rotation = rot;
        newLevel.floorWorldHeight = pos.y + 0.5f;

        newLevel.roomTilesMatrix = new TileHealth[size.x,size.y,size.z];
        bool hasRoof = true;

        Random.InitState(currentSeed);
        var offset = Random.Range(-levelIndexInBuilding, levelIndexInBuilding);
        Random.InitState(currentSeed);
        int spaceBetweenWindows = Random.Range(2, size.x + offset);
        int currentSpaceBetweenWindows = spaceBetweenWindows;
        Random.InitState(currentSeed);
        var xxx = Random.Range(1, size.y / 2 - 1);
        Random.InitState(currentSeed + levelIndexInBuilding);
        var yyy = Random.Range(size.y / 2 + 1, size.y -1 );
        Vector2Int windowStartEndY = new Vector2Int(xxx, yyy);
        
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

                if (levelIndexInBuilding == 0) // only on very first floor
                {
                    var newFloorObstacle = new GameObject("FloorObstacle");
                    newFloorObstacle.transform.parent = newFloorTile.transform;
                    newFloorObstacle.transform.localPosition = new Vector3(0, -0.5f, 0);
                    var obst = newFloorObstacle.AddComponent<NavMeshObstacle>();
                    obst.carving = true;
                }
                else if (buildingSettings.spawnSupports) // on every other floor
                {
                    // TRY TO FIND SUPPORTER TILE
                    if (LevelgenTransforms.SetSupporterTile(building.spawnedBuildingLevels, newFloorTile) == null)
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
                
                if (x == 0 || x == size.x - 1 || z == 0 || z == size.z - 1) // OUTER WALLS
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

                        if (levelIndexInBuilding < levelSettings.Count - 1 && y == size.y - 1)
                            newLevel.tilesTop.Add(newWallTile);
                        
                        if (cornerTile || y != 1)
                            continue;
                        
                        StartCoroutine(ConstructCover(newWallTile.gameObject, 3));
                        if (!windowHere)
                            availableStarPositionsForThinWalls.Add(new Vector3Int(x,1,z));
                    }
                }
                else // TILES INSIDE: GROUND AND CEILING
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
                        newLevel.tilesTop.Add(newFloorTile);
                    }
                    
                }
                //yield return null; 
            }
            if (singleFrame == false)
                yield return null;   
        }

        /*
        Debug.LogError("SPAWN INSIDE WALLS IS TURNED OFF FOR NOW");
        yield break;
        */
        if (buildingSettings.spawnRooms)
            yield return StartCoroutine(SpawnInsideWallsOnLevel(availableStarPositionsForThinWalls, newLevel, hasRoof, levelIndexInBuilding));
    }
    
    IEnumerator SpawnInsideWallsOnLevel(List<Vector3Int> availableStarPositionsForThinWalls, Level level, bool hasRoof, int levelIndex)
    {
        if (availableStarPositionsForThinWalls.Count <= 0)
            yield break;
        
        List<Vector2Int> RoomsOccupiedTilesPositions = new List<Vector2Int>(); // this will make sure rooms dont intersect
        
        
        int max = level.size.x / 10;
        for (int i = 0; i < max; i++) // ROOMS AMOUNT
        {
            Random.InitState(currentSeed + levelIndex);
            int buildWallUntillY = level.size.y - Mathf.Clamp(Random.Range(0, level.size.x / level.size.y), 0, level.size.y);
            
            // SPAWN INNER ROOMS
            Random.InitState(currentSeed + levelIndex);
            int leftSidePosition = Random.Range(1, level.size.x - 1);
            int rightSidePosition = 0;
            Random.InitState(currentSeed + levelIndex);
            int backSidePosition = Random.Range(1, level.size.z - 1);
            int frontSidePosition = 0;

            if (leftSidePosition < level.size.x / 2)
            {
                Random.InitState(currentSeed);
                rightSidePosition = leftSidePosition + Random.Range(2, level.size.x / 2);
            }
            else
            {
                Random.InitState(currentSeed + levelIndex);
                var tempPos = leftSidePosition - Random.Range(2, level.size.x / 2);
                rightSidePosition = leftSidePosition;
                leftSidePosition = tempPos;
            }

            if (backSidePosition < level.size.z / 2)
            {
                Random.InitState(currentSeed);
                frontSidePosition = backSidePosition + Random.Range(2, level.size.z / 2);
            }
            else
            {
                Random.InitState(currentSeed + levelIndex);
                var tempPos = backSidePosition - Random.Range(2, level.size.z / 2);
                frontSidePosition = backSidePosition;
                backSidePosition = tempPos;
            }

            Random.InitState(currentSeed + levelIndex);
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
                    //yield return null;
                }
                yield return null;
            }
            
            level.spawnedRooms.Add(newRoom);
            yield return null;
        }
        // SPAWN INVISIBLE BLOCKERS FOR RANDOM WALLS
        int[,,] invisibleWallBlockers = new int[level.size.x,level.size.y,level.size.z]; // 0 is free, 1 is block
        Vector3Int roomSize = new Vector3Int(level.size.x / 5, level.size.y, level.size.z / 5);
        Random.InitState(currentSeed);
        var xxx = Random.Range(0, level.size.x / 2);
        Random.InitState(currentSeed);
        var yyy = Random.Range(0, level.size.z / 2);
        Vector3Int roomLocalCoords = new Vector3Int(xxx, 0, yyy);
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
        Random.InitState(currentSeed + levelIndex);
        int wallsAmount = Random.Range(thinWallsPerLevelMinMax.x, thinWallsPerLevelMinMax.y);
        for (int i = 0; i < wallsAmount; i++)
        {
            Random.InitState(currentSeed + levelIndex);
            var currentWallCoord = availableStarPositionsForThinWalls[Random.Range(0, availableStarPositionsForThinWalls.Count)];
            var prevWallCoord = currentWallCoord;
            availableStarPositionsForThinWalls.Remove(currentWallCoord);
            // пустить крота по y==1

            List<Vector3Int> positionsInThinWall = new List<Vector3Int>(); 
            positionsInThinWall.Add(currentWallCoord);
            Random.InitState(currentSeed);
            float thinWallHeightScaler = Random.Range(0.2f, 1f);
            int posIndex = 0;

            int t = 0;
            
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
                Random.InitState(currentSeed + levelIndex);
                var nextPos = nextAvailablePositions[Random.Range(0, nextAvailablePositions.Count)];
                prevWallCoord = currentWallCoord;
                currentWallCoord = nextPos;

                int buildWallUntillY = level.size.y;
                if (hasRoof)
                    buildWallUntillY = level.size.y - 1;
                else
                    buildWallUntillY = Mathf.RoundToInt(buildWallUntillY * thinWallHeightScaler);
                
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

                t++;
                if (t > 10)
                {
                    t = 0;
                    yield return null;
                }
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


    IEnumerator MakeLaddersBetweenLevels(Building building, int i)
    {
        if (i - 1 < 0)
            yield break;
        
        Level levelFrom = building.spawnedBuildingLevels[i - 1];
        Level levelTo = building.spawnedBuildingLevels[i];
        
        if (!levelFrom.spawnLadders || !levelTo.spawnLadders || levelTo.firstFloor)
        {
            Debug.Log("DONT SPAWN LADDERS");
            yield break;
        }
        
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
        
        Random.InitState(currentSeed);
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

        
        ClearPlaceForLadder(levelFromClosestTile.position, levelToClosestTile.position);
        yield return StartCoroutine(SpawnLadder(levelFromClosestTile.position, levelToClosestTile.position, true, levelFrom.spawnedTransform, 100, levelFromClosestTile, levelToClosestTile, singleFrame));
    }

    IEnumerator MakeLadderOnEntrance(Building building, Level level)
    {
        Vector3 fromPosition = Vector3.zero;
        Vector3 toPosition = Vector3.zero;
        Transform targetTileToConnect = null;

        Random.InitState(currentSeed);
        int randomSide = Random.Range(0, 4); // 0 - left, 1 - front, 2 - right, 3 - back
        Vector3 offsetVector = Vector3.zero;

        building.localEntranceSide = randomSide;
        
        switch (randomSide)
        {
            case 0: // LEFT
                Random.InitState(currentSeed);
                var tile = level.roomTilesMatrix[0, 0, Random.Range(0, level.size.z)];
                targetTileToConnect = tile.transform;
                offsetVector = -tile.transform.right;
                break;
            case 1: // FRONT
                Random.InitState(currentSeed);
                var tileF = level.roomTilesMatrix[Random.Range(0, level.size.x), 0, level.size.z - 1];
                targetTileToConnect = tileF.transform;
                offsetVector = tileF.transform.forward;
                break;
            case 2: // RIGHT
                Random.InitState(currentSeed);
                var tileR = level.roomTilesMatrix[level.size.x - 1, 0, Random.Range(0, level.size.z)];
                targetTileToConnect = tileR.transform;
                offsetVector = tileR.transform.right;
                break;
            case 3: // BACK
                Random.InitState(currentSeed);
                var tileB = level.roomTilesMatrix[Random.Range(0, level.size.x), 0, 0];
                targetTileToConnect = tileB.transform;
                offsetVector = -tileB.transform.forward;
                break;
        }

        fromPosition = new Vector3(targetTileToConnect.position.x, 0, targetTileToConnect.position.z) + offsetVector * 5;  
        toPosition = targetTileToConnect.position - Vector3.up;
        
        ClearPlaceForLadder(fromPosition, toPosition);
        yield return StartCoroutine(SpawnLadder(fromPosition, toPosition, true, level.spawnedTransform, 20, null, targetTileToConnect, singleFrame));
    }
    
    public IEnumerator SpawnLadder(Vector3 fromPosition, Vector3 toPosition, bool destroyTilesAround, Transform parent, int maxBridgeTiles = -1, Transform startTile = null, Transform targetTile = null, bool singleFrame = false)
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
                Random.InitState(currentSeed);
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
                    {
                        tilesToDestroyForLadders.Add(bodyPart);
                        //bodyPart.DestroyTileFromGenerator();
                    }
                }
            }
            if (singleFrame == false)
                yield return null;
        }
    }

    private List<TileHealth> tilesToDestroyForLadders = new List<TileHealth>();
    void DestroyTilesForLadders()
    {
        foreach (var bodyPart in tilesToDestroyForLadders)
        {
            bodyPart.DestroyTileFromGenerator();
        }
    }
    
    void SpawnNavmesh(Level spawnedLevel)
    {
        if (!spawnedLevel.spawnNavMesh)
            return;
        
        var newNavMesh = Instantiate(navMeshSurfacePrefab, navMeshesParent);
        newNavMesh.transform.position = spawnedLevel.position;
        newNavMesh.transform.localScale = spawnedLevel.size;
        newNavMesh.transform.rotation = spawnedLevel.transform.rotation;
        newNavMesh.size = spawnedLevel.size;
        newNavMesh.center = new Vector3(0, newNavMesh.size.y/ 2, 0);
        navMeshSurfacesSpawned.Add(newNavMesh);
    }
    
    [Server]
    IEnumerator SpawnExplosiveBarrelsOnServer(Building building)
    {
        for (int i = 0; i < explosiveBarrelsAmount; i++)
        {
            var randomLevel = building.spawnedBuildingLevels[Random.Range(1, building.spawnedBuildingLevels.Count)];
            var randomTile = randomLevel.tilesInside[Random.Range(0, randomLevel.tilesInside.Count)];
            if (randomTile == null)
            {
                continue;
            }
            Vector3 pos = randomTile.transform.position + Vector3.up;
            randomLevel.tilesInside.Remove(randomTile);
            if (explosiveBarrelPrefab)
            {
                var barrel = Instantiate(explosiveBarrelPrefab, pos, Quaternion.identity);
                ServerManager.Spawn(barrel);
            }
            yield return null;
        }
    }
    [Server]
    IEnumerator SpawnPropsOnServer(Building building)
    {
        for (int i = 0; i < building.spawnedBuildingLevels.Count; i++)
        {
            var level = building.spawnedBuildingLevels[i];

            int propsAmount = Random.Range(level.size.x / 3, level.size.y);
            for (int j = 0; j < propsAmount; j++)
            {
                var randomTile = level.tilesInside[Random.Range(0, level.tilesInside.Count)];
                if (randomTile == null)
                    continue;
                
                Vector3 pos = randomTile.transform.position + Vector3.up/2;
                level.tilesInside.Remove(randomTile);
                var prop = Instantiate(propsPrefabs[Random.Range(0, propsPrefabs.Count)], pos, Quaternion.identity);
                ServerManager.Spawn(prop.gameObject);
                yield return null;   
            }
        }
    }

    IEnumerator SpawnLoot(Building building)
    {
        yield break;
        for (int i = 0; i < building.spawnedBuildingLevels.Count; i++)
        {
            if (!building.spawnedBuildingLevels[i].spawnLoot)
                continue;
            
            Random.InitState(currentSeed);
            int amount = Random.Range(lootPerLevelMinMax.x, lootPerLevelMinMax.y);
            
            // get all available tiles
            List<TileHealth> tilesForSpawn = new List<TileHealth>();
            
            for (int x = 0; x < building.spawnedBuildingLevels[i].size.x; x++)
            {
                for (int y = 0; y < building.spawnedBuildingLevels[i].size.y; y++)
                {
                    for (int z = 0; z < building.spawnedBuildingLevels[i].size.z; z++)
                    {
                        if (building.spawnedBuildingLevels[i].roomTilesMatrix[x, y, z] != null)
                        {
                            tilesForSpawn.Add(building.spawnedBuildingLevels[i].roomTilesMatrix[x, y, z]);
                        }
                    }
                }
            }   
            
            for (int j = 0; j < amount; j++)
            {
                // choose tile to spawn on
                Random.InitState(currentSeed);
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
                    
                    Random.InitState(currentSeed);
                    randomTile = tilesForSpawn[Random.Range(0, tilesForSpawn.Count)];
                    yield return null;
                }

                if (randomTile == null)
                    break;
                
                Vector3 randomOffset = Vector3.forward * 0.5f;
                Random.InitState(currentSeed);
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
                Random.InitState(currentSeed);
                var prefab = lootToSpawnAround[Random.Range(0, lootToSpawnAround.Count)]; 
                Random.InitState(currentSeed);
                float xxx = Random.Range(0, 360); 
                Random.InitState(currentSeed);
                float yyy = Random.Range(0, 360); 
                Random.InitState(currentSeed);
                float zzz = Random.Range(0, 360);
                var newLoot = Instantiate(prefab, spawnPos, Quaternion.Euler(xxx,yyy,zzz));
            }
            yield return null;
        }
    }

    void SpawnBillboard(Building building)
    {
        // fix later
        return;
        
        if (building.spawnedBuildingLevels.Count <= 3)
            return;
        
        var newBillboard = Instantiate(billboardGeneratorPrefab);
        var randomLevel = building.spawnedBuildingLevels[Random.Range(2, building.spawnedBuildingLevels.Count - 1)];
        float yRot = 0;
        int wallSize = 0;
        Vector3 billboardPos = Vector3.zero;
        
        switch (building.localEntranceSide)
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
    
    IEnumerator SpawnGrindRails(Building building) // соединять главное здание с дополнительными
    {
        for (int j = 0; j < Random.Range(grindRailsMinMax.x, grindRailsMinMax.y); j++)
        {
            var randomLevel = building.spawnedBuildingLevels[Random.Range(0,building.spawnedBuildingLevels.Count)];
            var randomTile = randomLevel.tilesInside[Random.Range(0, randomLevel.tilesInside.Count)];

            Vector3 pos = randomTile.transform.position + Vector3.up;
            randomLevel.tilesInside.Remove(randomTile);
            var grindRails = Instantiate(grindRailsPrefab, pos, Quaternion.identity);
            grindRails.GenerateNodes(true);
            yield return null;
        }
    }


    [Serializable]
    enum DolaSpawnWhere
    {
        TopFloor, BottomFloor, Random
    }

    [SerializeField] private DolaSpawnWhere _dolaSpawnWhere = DolaSpawnWhere.TopFloor;
    [Server]
    IEnumerator SpawnGoalsOnServer(Building building)
    {
        // spawn tools on every floor 
        for (int index = 0; index < building.spawnedBuildingLevels.Count; index++)
        {
            var level = building.spawnedBuildingLevels[index];
            var tiles = level.tilesInside;
            Vector3 spawnPosition = tiles[Random.Range(0, tiles.Count)].transform.position + Vector3.up/2;
            var levelGoalSpawned = Instantiate(ContentPlacer.Instance.GetToolForSpawnOnLevel().gameObject, spawnPosition, Quaternion.identity);
            ServerManager.Spawn(levelGoalSpawned);
        }
        for (int i = 0; i < building.spawnedBuildingLevels.Count; i++)
        {
            for (int j = 0; j < building.spawnedBuildingLevels[i].spawnedRooms.Count; j++)
            {
                var room = building.spawnedBuildingLevels[i].spawnedRooms[j];

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
                InteractableEventsManager.Instance.ExplosionNearInteractables(tile.transform.position);
                GameVoxelModifier.Instance?.DestructionInWorld(tile.transform.position);
                tile.ActivateRigidbody(100, tilePhysicsMaterial);
                level.roomTilesMatrix[x, YYY, z] = null;
            }
        }
    }

    //IEnumerator CheckTilesToFall
    
    private List<Transform> tilesToDamage = new List<Transform>();

    IEnumerator TileDamagedFeedbackCoroutine(Transform tile)
    {
        //Debug.LogWarning("TILES SHAKE DAMAGE FEEDBACK IS DISABLED");
        
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
        var debris = Instantiate(tileDestroyedParticles, pos, Quaternion.identity);
        debris.transform.parent = transform;
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
        while (Game._instance == null || Game.LocalPlayer == null)
        {
            yield return null;
        }
        for (int i = 0; i < navMeshSurfacesSpawned.Count; i++)
        {
            yield return new WaitForSecondsRealtime(0.25f);
            navMeshSurfacesSpawned[i].BuildNavMesh();
        }
        yield break;
        while (true)
        {
            yield return null;
            
            for (int i = 0; i < navMeshSurfacesSpawned.Count; i++)
            {
                yield return new WaitForSecondsRealtime(1f);
                
                if (Game._instance == null || Game.LocalPlayer == null)
                    continue;
                if (Vector3.Distance(Game.LocalPlayer.Position, navMeshSurfacesSpawned[i].transform.position) < updateNavMeshDistance)
                {
                    if (navMeshSurfacesSpawned[i].navMeshData)
                        navMeshSurfacesSpawned[i].UpdateNavMesh(navMeshSurfacesSpawned[i].navMeshData);
                    else
                    {
                        navMeshSurfacesSpawned[i].BuildNavMesh();
                    }
                }
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

    private void OnDrawGizmos()
    {
        for (int i = 0; i < buildingsToSpawnSettings.Count; i++)
        {
            var building = buildingsToSpawnSettings[i];
            float lastHeight = 0;
            for (int j = 0; j < building.levelsSettings.Count; j++)
            {
                // yellow is min, red is full
                Vector3 offsetVert = Vector3.up * building.levelsSettings[j].levelHeight / 2 + Vector3.up * lastHeight;
                
                Gizmos.color = Color.yellow;
                Gizmos.DrawCube(building.BuildingOriginTransform.position + offsetVert, new Vector3(building.levelsScaleMinMaxX.x * 2, building.levelsSettings[j].levelHeight, building.levelsScaleMinMaxZ.x * 2));
                
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(building.BuildingOriginTransform.position + offsetVert, new Vector3(building.levelsScaleMinMaxX.y * 2, building.levelsSettings[j].levelHeight, building.levelsScaleMinMaxZ.y * 2));
                lastHeight += building.levelsSettings[j].levelHeight;
            }
        }
    }
    
    public Vector3 GetRandomPosInsideLastLevel()
    {
        if (spawnedBuildings[0].spawnedBuildingLevels.Count < 1)
        {
            return transform.position;
        }
        
        var tilesInside = spawnedBuildings[0].spawnedBuildingLevels[spawnedBuildings[0].spawnedBuildingLevels.Count - 1].tilesInside;
        var tile = tilesInside[Random.Range(0, tilesInside.Count)];
        int tries = 30;
        while (tile == null)
        {
            tile = tilesInside[Random.Range(0, tilesInside.Count)];
            tries--;
            if (tries < 1)
            {
                return transform.position;
            }
        }
        return tile.transform.position;
    }

    public void DestroyBuilding()
    {
        return;
        for (int i = 0; i < spawnedBuildings.Count; i++)
        {
            if (spawnedBuildings[i].spawnedBuildingLevels.Count < 1)
                continue;
            for (int j = spawnedBuildings[i].spawnedBuildingLevels.Count - 1; j >= 0; j--)
            {
                Destroy(spawnedBuildings[i].spawnedBuildingLevels[j].transform.gameObject);
            }
        }
    }
}

[Serializable]
public class Room
{
    public List<Vector3Int> coordsInside = new List<Vector3Int>();
}

[Serializable]
public class BuildingSettings
{
    [Space]
    [Header("BUILDING")]
    public List<LevelSetting> levelsSettings = new List<LevelSetting>();
    public Transform BuildingOriginTransform;
    
    public Vector2Int offsetPosMinMaxX = new Vector2Int(0, 0);
    public Vector2Int offsetPosMinMaxZ = new Vector2Int(0, 0);
    [Header("scaled by 2 in code")]
    public Vector2Int levelsScaleMinMaxX = new Vector2Int(3, 10);
    public Vector2Int levelsScaleMinMaxZ = new Vector2Int(3, 10);

    public bool spawnLoot = true;
    public bool spawnLadders = true;
    public bool spawnRooms = true;
    public bool spawnNavMesh = true;
    public bool updateClash = false;
    public bool spawnSupports = false;

    [Serializable]
    public class LevelSetting
    {
        public int levelHeight = 5;
        public List<HealthController> uniqueNpcsToSpawn = new List<HealthController>();
        public List<HealthController> unitsToSpawn = new List<HealthController>();
        public List<ControlledMachine> controlledMachinesToSpawn = new List<ControlledMachine>();
        [Header("NETWORKED")]public List<GameObject> extraGameObjectsToSpawn = new List<GameObject>();
    }
    
}

public static class LevelgenTransforms
{
    public static TileHealth SetSupporterTile(List<Level> levels, TileHealth tile)
    {
        if (tile == null)
            return null;

        if (Physics.Raycast(tile.transform.position, Vector3.down, out var hit, 1,  GameManager.Instance.AllSolidsMask,QueryTriggerInteraction.Ignore))
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
