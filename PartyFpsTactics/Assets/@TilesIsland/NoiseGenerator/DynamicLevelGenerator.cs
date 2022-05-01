using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GPUInstancer;
using MrPink;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

public class DynamicLevelGenerator : MonoBehaviour
{
    public static DynamicLevelGenerator instance;

    public float tileSize = 20;
    public Rigidbody playerTarget;
    public int viewDistance = 8;
    public int distanceToDestroyTile = 8;
    public float waterLevel = 250;
    public float maxBridgePartsHeightDiffrenece = 40;
    [Tooltip("Distance between bridge part and spawned tile")]
    public float bridgePlatformThreshold = 40;

    public TileCoordinate[,] spawnedTiles;
    public List<Vector2Int> spawnedTilesPositions = new List<Vector2Int>();
    public GameObject islandWalkerPrefab;
    public GameObject poiChurchPrefab;

    private float[,] noiseMap;

    private GameObject tilesParent;

    [Header("GPU Instancing")]
    public List<Pooling.IslandTilePool.IslandTilePrefabTag> tileGpuPrefabsTags;
    //public GPUInstancerPrefabManager prefabManager;
    
    [Serializable]
    public class TileCoordinate
    {
        public GameObject spawnedBuilding;
        public IslandTile spawnedTile;
        public GameObject spawnedNpc;
        public GPUInstancerPrefab spawnedGpuTile;
        public GameObject spawnedPath;
        public GameObject spawnedBridgePart;
        public int x = 0;
        public int z = 0;
    }
    void Awake()
    {
        instance = this;
        //GPUInstancerAPI.InitializeGPUInstancer(prefabManager);
    }

    
    public void Init()
    {
        noiseMap = IslandGenerator.instance.GetNoiseMap();
        spawnedTiles = new TileCoordinate[IslandGenerator.instance.mapWidth,IslandGenerator.instance.mapHeight];
        
        tilesParent = new GameObject();
        tilesParent.name = "TilesParent";

        var spawnCoords = IslandGenerator.instance.playerSpawnPoint.coordinates;
        playerTarget.transform.position = new Vector3(spawnCoords[0].x, 0, spawnCoords[0].y)  * tileSize - Offset() + Vector3.up * 200;
        playerTarget.isKinematic = false;
        
        Invoke(nameof(TeleportPlayerToTarget), 1);
        StartCoroutine(UpdateLevelAroundPlayer());
        StartCoroutine(DestroyTiles());
        
        BuildingGenerator.Instance.Init();
    }
    
    [ContextMenu("TeleportPlayerToTarget")]
    public Vector3 TeleportPlayerToTarget()
    {
        Vector3 pos = playerTarget.position + Vector3.up * 1000;
        Game.Player.Movement.TeleportToPosition(pos);
        return pos;
    }

    Vector3 Offset()
    {
        return Vector3.zero;
        return new Vector3(IslandGenerator.instance.mapWidth / 2, 0, IslandGenerator.instance.mapHeight / 2) * tileSize;
    }

    public Vector2Int GetCoordinatesFromWorldPosition(Vector3 pos)
    {
        return new Vector2Int(Mathf.RoundToInt(pos.x / tileSize), Mathf.RoundToInt(pos.z / tileSize));
    }

    
    public List<Node> GetCoordsOfClosestNeighbourOfTarget(Vector3 pos)
    {
        var targetCoords = GetCoordinatesFromWorldPosition(pos);
        return IslandRoadsGenerator.instance.GetNeighbours(IslandRoadsGenerator.instance.grid[targetCoords.x, targetCoords.y]);
    }
    
    private Vector2Int playerCoords;
    
    IEnumerator UpdateLevelAroundPlayer()
    {
        var ig = IslandGenerator.instance;
        while (IslandGenerator.instance.generationComplete == false)
        {
            yield return null;
        }

        int t = 0;
        
        while (true)
        {
            //playerCoords = GetCoordinatesFromWorldPosition(playerTarget.transform.position);
            playerCoords = GetCoordinatesFromWorldPosition(Game.Player.Position);

            // spawn new tiles around player
            for (int x = - viewDistance + playerCoords.x; x <= viewDistance + playerCoords.x; x++)
            {
                for (int z = viewDistance + playerCoords.y; z > -viewDistance + playerCoords.y; z--)
                {
                    if (CoordinatesInBounds(x,z) && !TileOnCoordinates(x, z, false))
                    {   
                        bool isPath = ig.IsPath(x, z);
                        float newHeight = noiseMap[x, z];

                        if (isPath) newHeight -= Random.Range(0.1f, 0.5f);
                            
                        int regionIndex = IslandGenerator.instance.GetRegionIndexByHeight(newHeight);
                        if (regionIndex < 2 || IslandGenerator.instance.regions[regionIndex].tileAssetReferenceList.Count == 0)
                            continue;
                            
                        var reference = tileGpuPrefabsTags[regionIndex];
                        //var reference = tileGpuPrefabs[regionIndex];
                        //var reference = IslandGenerator.instance.regions[regionIndex].tileAssetReferenceList[Random.Range(0, IslandGenerator.instance.regions[regionIndex].tileAssetReferenceList.Count)] ;
                        
                        if (reference == null)
                            continue;
                        
                        var newCoord = new TileCoordinate();
                        newCoord.x = x;
                        newCoord.z = z;
                        spawnedTiles[x,z] = newCoord;

                        float newY = 0;
                        switch (regionIndex)
                        {
                            case 0: // deep
                                newY = Random.Range(0, 200);
                                break;
                            case 1: // water
                                newY = Random.Range(200, 300);
                                break;
                            case 2: // sand
                                newY = Random.Range(300, 400);
                                break;
                            case 3: // grass
                                newY = Random.Range(400, 500);
                                break;
                            case 4: // jungles
                                newY = Random.Range(500f, 800f);
                                break;
                            case 5: // rocks
                                newY = Random.Range(700f, 1000f);
                                break;
                            case 6: // cliffs
                                newY = Random.Range(800f, 1500f);
                                break;
                            case 7: // snows
                                newY = 2000;
                                break;
                        }

                        if (isPath) newY /= 10;
                        
                        //AssetSpawner.instance.SpawnTile(reference, new Vector3(x * tileSize, noiseMap[x,z] * newY, z * tileSize), 0, -1, -1, false, -1, false );

                        SpawnTileGpu(reference, new Vector3(x * tileSize, noiseMap[x,z] * newY, z * tileSize) - Offset(), new Vector2Int(x, z));

                        yield return new WaitForSeconds(0.01f);
                    }
                }
            }
            yield return new WaitForSeconds(0.01f);
        }
    }

    void SpawnTileGpu(Pooling.IslandTilePool.IslandTilePrefabTag prefabTag, Vector3 pos, Vector2Int coords)
    {
        var newInstance = Pooling.Instance.SpawnIslandTile(prefabTag, pos, Quaternion.identity);
        //prefabManager.AddPrefabInstance(newInstance, true);
        spawnedTilesPositions.Add(coords);
        StartCoroutine(ProceedTile(newInstance, coords));
    }

    bool CoordinatesInBounds(int x, int z)
    {
        if (x > 0 && x < noiseMap.GetLength(0) && z > 0 && z < noiseMap.GetLength(1))
        {
            return true;
        }

        return false;
    }

    IEnumerator DestroyTiles()
    {
        while (IslandGenerator.instance.generationComplete == false)
        {
            yield return null;
        }

        int t = 0;
        while (true)
        {
            if (spawnedTiles.Length <= 0 || spawnedTilesPositions.Count <= 0)
            {
                yield return null;
                continue;
            }

            for (int i = spawnedTilesPositions.Count - 1; i >= 0; i--)
            {
                int x = spawnedTilesPositions[i].x;
                int z = spawnedTilesPositions[i].y;
                
                
                if (x < playerCoords.x - distanceToDestroyTile || x > playerCoords.x + distanceToDestroyTile ||
                    z < playerCoords.y - distanceToDestroyTile || z > playerCoords.y + distanceToDestroyTile)
                {
                    if (spawnedTiles[x, z] != null)
                    {
                        if (spawnedTiles[x,z].spawnedTile)
                        {
                            Pooling.Instance.ReleaseIslandTile(spawnedTiles[x,z].spawnedTile);
                            Destroy(spawnedTiles[x, z].spawnedTile);
                        }
                    
                        if (spawnedTiles[x,z].spawnedPath)
                            Destroy(spawnedTiles[x,z].spawnedPath);
                        
                        if (spawnedTiles[x,z].spawnedBuilding)
                            Destroy(spawnedTiles[x,z].spawnedBuilding);
                        
                        if (spawnedTiles[x,z].spawnedBridgePart)
                            Destroy(spawnedTiles[x,z].spawnedBridgePart);

                        if (spawnedTiles[x, z].spawnedGpuTile)
                        {
                            Destroy(spawnedTiles[x,z].spawnedGpuTile);
                        }
                        
                        spawnedTiles[x, z] = null;
                        spawnedTilesPositions.RemoveAt(i);
                    }
                }
                t++;
                if (t > 100)
                {
                    t = 0;
                    yield return null;
                }
                //yield return null;
            }
            
            /*
            for (var x = 0; x < spawnedTiles.GetLength(0); x++)
            for (var z = 0; z < spawnedTiles.GetLength(1); z++)
            {
                if (x < playerCoords.x - distanceToDestroyTile || x > playerCoords.x + distanceToDestroyTile ||
                    z < playerCoords.y - distanceToDestroyTile || z > playerCoords.y + distanceToDestroyTile)
                {
                    if (spawnedTiles[x, z] != null)
                    {
                        if (spawnedTiles[x,z].spawnedTile)
                            Destroy(spawnedTiles[x,z].spawnedTile);
                    
                        if (spawnedTiles[x,z].spawnedPath)
                            Destroy(spawnedTiles[x,z].spawnedPath);
                        
                        if (spawnedTiles[x,z].spawnedBuilding)
                            Destroy(spawnedTiles[x,z].spawnedBuilding);
                        
                        if (spawnedTiles[x,z].spawnedBridgePart)
                            Destroy(spawnedTiles[x,z].spawnedBridgePart);

                        if (spawnedTiles[x, z].spawnedGpuTile)
                        {
                            GPUInstancerAPI.RemovePrefabInstance(prefabManager, spawnedTiles[x,z].spawnedGpuTile);
                            Destroy(spawnedTiles[x,z].spawnedGpuTile);
                        }
                        
                        spawnedTiles[x, z] = null;
                    }
                    t++;
                    if (t > 100)
                    {
                        t = 0;
                        yield return null;
                    }
                }
            }*/
        }
    }

    bool TileOnCoordinates(int x, int z, bool remove)
    {
        if (spawnedTiles[x, z] != null && spawnedTiles[x,z].spawnedTile != null)
        {
            if (remove)
            {
                spawnedTiles[x, z] = null;
            }
            return true;
        }

        return false;
    }
    
    bool TileOnCoordinates(Vector3 tilePos, bool remove)
    {
        int x = Mathf.RoundToInt(tilePos.x / tileSize);
        int z = Mathf.RoundToInt(tilePos.z / tileSize);

        if (spawnedTiles[x, z] != null && spawnedTiles[x,z].spawnedTile != null)
        {
            if (remove)
            {
                spawnedTiles[x, z] = null;
            }
            return true;
        }

        return false;
    }

    public IEnumerator ProceedTile(IslandTile tile, Vector2Int coords)
    {
        tile.transform.Rotate(Vector3.up, Random.Range(0, 360f));
        
        int _x = Mathf.RoundToInt(tile.transform.position.x / tileSize);
        int _z = Mathf.RoundToInt(tile.transform.position.z / tileSize);

        /*
        _x = coords.x;
        _z = coords.y;
        */
        
        if (spawnedTiles[_x, _z] == null)
        {
            spawnedTiles[_x, _z] = new TileCoordinate();
            spawnedTiles[_x, _z].x = _x;
            spawnedTiles[_x, _z].z = _z;
        }
        spawnedTiles[_x, _z].spawnedTile = tile;

        tile.transform.parent = tilesParent.transform;
        yield return null;
        
        // IF THIS TILE HAS POI
        if (IslandGenerator.instance.poisCoordinates.Contains(new Vector2Int(_x, _z)))
        {
            var poiIndex = IslandGenerator.instance.poisCoordinates.IndexOf(new Vector2Int(_x, _z));
            BuildingGenerator.Instance.SpawnRandomBuilding(tile.transform.position,IslandGenerator.instance.poisCoordinates[poiIndex]);
            /*if (poiIndex != 0) // 0 is player spawner?
            {
                // SPAWN BUILDING
                BuildingGenerator.Instance.SpawnRandomBuilding(go.transform.position,IslandGenerator.instance.poisCoordinates[poiIndex]);
            }*/
            
            //spawnedTiles[_x, _z].spawnedNpc = Instantiate(islandWalkerPrefab, go.transform.position, Quaternion.identity);
        }
        
        yield break;
        
        if (spawnedTiles[_x, _z].spawnedBuilding == null && IslandGenerator.instance.poisCoordinates.Contains(new Vector2Int(_x, _z)) && IslandGenerator.instance.poisCoordinates.IndexOf(new Vector2Int(_x, _z)) != 0)
        {
            var poi = Instantiate(poiChurchPrefab, tile.transform.position, Quaternion.identity);
            poi.transform.Rotate(Vector3.up, 90 * Random.Range(0,4));
            spawnedTiles[_x, _z].spawnedBuilding = poi;
        }
        
    } 
    
    public void ProceedPath(GameObject go)
    {
        go.transform.position += Vector3.up * Random.Range(10f,20f);
        if (go.transform.position.y <= waterLevel + 10)
            go.transform.position = new Vector3(go.transform.position.x, waterLevel + Random.Range(10f, 20f), go.transform.position.z);
        
        int x = Mathf.RoundToInt(go.transform.position.x / tileSize);
        int z = Mathf.RoundToInt(go.transform.position.z / tileSize);

        if (spawnedTiles[x, z] == null)
        {
            spawnedTiles[x, z] = new TileCoordinate();
            spawnedTiles[x, z].x = x;
            spawnedTiles[x, z].z = z;
        }
        spawnedTiles[x, z].spawnedPath = go;

        go.transform.eulerAngles = new Vector3(Random.Range(80f,100f), Random.Range(0,180f), Random.Range(-10,10f));
        go.transform.localScale = new Vector3(1, 1, go.transform.position.y);
        go.transform.parent = tilesParent.transform;
        return;
        //find neighbour bridges
        for (var _z = z - 1; _z <= z + 1; _z++)
        {
            for (var _x = x - 1; _x <= x + 1; _x++)
            {
                if (_z < 0 || _z >= IslandGenerator.instance.mapHeight || _x < 0 || _x > IslandGenerator.instance.mapWidth || (_z == z && _x == x))
                    continue;

                if (spawnedTiles[_x, _z] == null || spawnedTiles[_x, _z].spawnedPath == null)
                    continue;

                if (Mathf.Abs(spawnedTiles[_x, _z].spawnedPath.transform.position.y - spawnedTiles[x, z].spawnedPath.transform.position.y) > maxBridgePartsHeightDiffrenece)
                    go.transform.position = new Vector3(go.transform.position.x, Mathf.Lerp(go.transform.position.y, spawnedTiles[_x, _z].spawnedPath.transform.position.y, Random.Range(0.75f, 0.99f)), go.transform.position.z);
                
                //allign with a neighbour                
                spawnedTiles[x, z].spawnedPath.transform.position += new Vector3(Random.Range(-2, 2), Random.Range(2, 5), Random.Range(-2, 2));
                
                //spawnedTiles[x, z].spawnedPath.transform.LookAt(spawnedTiles[_x, _z].spawnedPath.transform);

                // SMOOTH BRIDGES ROTATIONS BY
                // add Y rotation of target bridge to this one!
                spawnedTiles[x, z].spawnedPath.transform.LookAt(spawnedTiles[_x, _z].spawnedPath.transform.position);
                spawnedTiles[x, z].spawnedPath.transform.rotation = Quaternion.Slerp(spawnedTiles[x, z].spawnedPath.transform.rotation, spawnedTiles[_x, _z].spawnedPath.transform.rotation, 0.5f);
                
                spawnedTiles[x, z].spawnedPath.transform.localScale = new Vector3(spawnedTiles[x, z].spawnedPath.transform.localScale.x, spawnedTiles[x, z].spawnedPath.transform.localScale.y,
                    Vector3.Distance(spawnedTiles[x, z].spawnedPath.transform.position,
                        spawnedTiles[_x, _z].spawnedPath.transform.position));
                break;
            }
        }
        
        // GET BRIDGE
        if (spawnedTiles[x, z].spawnedTile && Mathf.Abs(spawnedTiles[x, z].spawnedPath.transform.position.y -
                                                    spawnedTiles[x, z].spawnedTile.transform.position.y) > bridgePlatformThreshold)
        {
            go.transform.localScale = new Vector3(40, go.transform.localScale.y, go.transform.localScale.z);
        }

        // CONSTRUCT BRIDGE PART
        //
        GameObject newGo = Instantiate(go, go.transform.position - Vector3.up + go.transform.forward * go.transform.localScale.z / 2, Quaternion.identity);
        newGo.transform.eulerAngles = new Vector3(Random.Range(80f,100f), Random.Range(0,180f), Random.Range(-10,10f));
        newGo.transform.localScale = new Vector3(1, 1, newGo.transform.position.y);
        spawnedTiles[x, z].spawnedBridgePart = newGo;
        
        // put ground down
        if (spawnedTiles[x, z].spawnedTile && spawnedTiles[x, z].spawnedTile.transform.position.y >
            spawnedTiles[x, z].spawnedPath.transform.position.y - 3)
        {
            spawnedTiles[x, z].spawnedTile.transform.position = new Vector3(spawnedTiles[x, z].spawnedTile.transform.position.x, spawnedTiles[x, z].spawnedPath.transform.position.y - Random.Range(3f,10f), spawnedTiles[x, z].spawnedTile.transform.position.z); 
        }
    }
}
