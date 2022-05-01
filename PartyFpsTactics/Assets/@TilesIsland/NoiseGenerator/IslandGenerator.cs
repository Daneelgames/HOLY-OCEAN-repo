using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

public class IslandGenerator : MonoBehaviour
{
    public static IslandGenerator instance;
    
    public enum DrawMode
    {
        NoiseMap, ColourMap
    }

    public DrawMode drawMode = DrawMode.ColourMap;

    private float[,] NoiseMap;
    private int[,] ObstaclesMap;
    
    [Header("Map")]
    public int mapWidth;
    public int mapHeight;
    public int cellSize = 100;
    public float smooth = 0.5f;
    public float noiseScale;

    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;
    public TerrainType[] regions;
    
    [Range(0,10)]
    public int PoiScaler = 5;

    public bool addScaledPoiPointsToLists = false;
    [Header("Obstacles")] 
    public bool visualizeObstacles = true;
    public int randomFillPercent;
    public int obstaclesSmoothIterations = 10;
    
    public PointOfInterest playerSpawnPoint;
    public PointOfInterest bossSpawnPoint;
    public List<PointOfInterest> pointsOfInterest = new List<PointOfInterest>();
    public List<PixelsByRegion> pixelsByRegions = new List<PixelsByRegion>();
    public List<Vector2Int> obstaclesCoordinates = new List<Vector2Int>();
    public List<Vector2Int> pathsCoordinates = new List<Vector2Int>();
    public List<Vector2Int> poisCoordinates = new List<Vector2Int>();
    List<CellWithPixels> cellsWithPixels = new List<CellWithPixels>();

    public bool generationComplete = false;
    
    public NoiseMapDisplay mapDisplay;
    private IslandRoadsGenerator irg;

    void Awake()
    {
        irg = gameObject.GetComponent<IslandRoadsGenerator>();
        instance = this;
        GenerateNewMap();
    }

    public float[,] GetNoiseMap()
    {
        return NoiseMap;
    }

    [ContextMenu("GenerateNew")]
    public void GenerateNewMap()
    {
        RandomSeed();
        StartCoroutine(GenerateMap());
    }
    
    public void RandomSeed()
    {
        Random.InitState((int) DateTime.Now.Ticks);
        
        mapWidth = cellSize * Random.Range(5, 20);
        mapHeight = cellSize * Random.Range(5, 20);
        
        noiseScale = Random.Range(20, 200);
        smooth = Random.Range(0.7f, 1f);
        seed = Random.Range(0, 1000000);
        offset = new Vector2(Random.Range(0, 100000), Random.Range(0, 100000));
    }

    [ContextMenu("UpdateMap")]
    public void UpdateMap()
    {
        StartCoroutine(GenerateMap());
    }
    public IEnumerator GenerateMap()
    {
        mapDisplay.ResetMaps();
        print("GenerateMap");
        NoiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, smooth, seed, noiseScale, octaves, persistance, lacunarity, offset);
        
        obstaclesCoordinates.Clear();
        pathsCoordinates.Clear();
        poisCoordinates.Clear();
        pointsOfInterest.Clear();
        pixelsByRegions.Clear();
        cellsWithPixels.Clear();
        
        for (int i = 0; i < regions.Length; i++)
        {
            pixelsByRegions.Add(new PixelsByRegion());
            pixelsByRegions[i].name = regions[i].name;
        }
        
        // FIND AND SET COLOURS TO REGIONS
        Color[] colourMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = NoiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        // save color to colourMap
                        colourMap[y * mapWidth + x] = regions[i].colour;
                        
                        // add pixel to pixels by regions list
                        pixelsByRegions[i].pixelsCoordinates.Add(new Vector2Int(x,y));
                        break;
                    }
                }
            }
        }

        if (drawMode == DrawMode.NoiseMap)
            mapDisplay.DrawTexture(NoiseTextureGenerator.TextureFromHeightMap(NoiseMap));
        else if (drawMode == DrawMode.ColourMap)
            mapDisplay.DrawTexture(NoiseTextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
        
        Color[] colourMapPoi = new Color[mapWidth * mapHeight];
        Color[] colourMapObstacles = new Color[mapWidth * mapHeight];
        yield return StartCoroutine(CreateCells(colourMap));
        print("CELLS CREATED!");
        yield return StartCoroutine(SetPointsOfInterest(colourMapPoi));
        mapDisplay.DrawPoi(NoiseTextureGenerator.TextureFromColourMap(colourMapPoi, mapWidth, mapHeight));
        print("POINTS OF INTEREST!");
        yield return StartCoroutine(GenerateObstacles(colourMapObstacles));
        print("OBSTACLES CREATED!");
        mapDisplay.DrawObstacles(NoiseTextureGenerator.TextureFromColourMap(colourMapObstacles, mapWidth, mapHeight));
        
        irg.ConnectPointsOfInterest();
    }

    public void PointsConnected(Color[] colourRoads)
    {
        mapDisplay.DrawRoads(NoiseTextureGenerator.TextureFromColourMap(colourRoads, mapWidth, mapHeight));
        generationComplete = true;
        print("ISLAND GENERATION COMPLETED!");
        
        DynamicLevelGenerator.instance.Init();
    }

    IEnumerator CreateCells(Color[] colorMap)
    {
        cellsWithPixels = new List<CellWithPixels>();
        for (int y = 0; y < mapHeight / cellSize; y++)
        {
            for (int x = 0; x < mapWidth / cellSize; x++)
            {
                int offsetX = Random.Range(-15, 15);
                int offsetY = Random.Range(-15, 15);
                cellsWithPixels.Add(new CellWithPixels());
                cellsWithPixels[cellsWithPixels.Count - 1].index = cellsWithPixels.Count - 1;
                cellsWithPixels[cellsWithPixels.Count - 1].deepPixelsCoordinates = new List<Vector2Int>();
                cellsWithPixels[cellsWithPixels.Count - 1].waterPixelsCoordinates = new List<Vector2Int>();
                cellsWithPixels[cellsWithPixels.Count - 1].sandPixelsCoordinates = new List<Vector2Int>();
                cellsWithPixels[cellsWithPixels.Count - 1].grassPixelsCoordinates = new List<Vector2Int>();
                cellsWithPixels[cellsWithPixels.Count - 1].junglePixelsCoordinates = new List<Vector2Int>();
                cellsWithPixels[cellsWithPixels.Count - 1].rocksPixelsCoordinates = new List<Vector2Int>();
                cellsWithPixels[cellsWithPixels.Count - 1].cliffsPixelsCoordinates = new List<Vector2Int>();
                cellsWithPixels[cellsWithPixels.Count - 1].snowPixelsCoordinates = new List<Vector2Int>();
                
                for (int pixelY = y * cellSize; pixelY < y * cellSize + cellSize; pixelY++)
                {
                    for (int pixelX = x * cellSize; pixelX < x * cellSize + cellSize; pixelX++)
                    {
                        var pixelHeight = NoiseMap[pixelX, pixelY];

                        if (pixelX == x * cellSize + cellSize / 2 + offsetX && pixelY == y * cellSize + cellSize / 2 + offsetY)
                        {
                            cellsWithPixels[cellsWithPixels.Count - 1].centerPixel = new Vector2Int(pixelX, pixelY);
                        }
                        
                        if (pixelHeight < regions[0].height)
                            cellsWithPixels[cellsWithPixels.Count - 1].deepPixelsCoordinates.Add(new Vector2Int(pixelX, pixelY));
                        else if (pixelHeight < regions[1].height)
                            cellsWithPixels[cellsWithPixels.Count - 1].waterPixelsCoordinates.Add(new Vector2Int(pixelX, pixelY));
                        else if (pixelHeight < regions[2].height)
                            cellsWithPixels[cellsWithPixels.Count - 1].sandPixelsCoordinates.Add(new Vector2Int(pixelX, pixelY));
                        else if (pixelHeight < regions[3].height)
                            cellsWithPixels[cellsWithPixels.Count - 1].grassPixelsCoordinates.Add(new Vector2Int(pixelX, pixelY));
                        else if (pixelHeight < regions[4].height)
                            cellsWithPixels[cellsWithPixels.Count - 1].junglePixelsCoordinates.Add(new Vector2Int(pixelX, pixelY));
                        else if (pixelHeight < regions[5].height)
                            cellsWithPixels[cellsWithPixels.Count - 1].rocksPixelsCoordinates.Add(new Vector2Int(pixelX, pixelY));
                        else if (pixelHeight < regions[6].height)
                            cellsWithPixels[cellsWithPixels.Count - 1].cliffsPixelsCoordinates.Add(new Vector2Int(pixelX, pixelY));
                        else if (pixelHeight < regions[7].height)
                            cellsWithPixels[cellsWithPixels.Count - 1].snowPixelsCoordinates.Add(new Vector2Int(pixelX, pixelY));
                    }       
                    
                    /*
                    // mark cells for test
                    if (pixelY < y * cellSize + cellSize - 5)
                        colorMap[pixelY * mapWidth + x * cellSize] = Color.black;
                        */
                }

                yield return null;
            }
        }
    }
    
    IEnumerator SetPointsOfInterest(Color[] colourMap)
    {
        int deepPoiLeft = 1;
        int junglesPoiLeft = 4;
        int rocksPoiLeft = 3;
        int cliffsPoiLeft = 2;
        int snowPoiLeft = 1;
        Debug.Log("Cells amount: " + cellsWithPixels.Count);

        SavePlayerSpawner(colourMap);

        #region place POIs

        for (int i = cellsWithPixels.Count - 1; i >= 0; i--)
        {
            yield return null;
            var cell = cellsWithPixels[Random.Range(0, cellsWithPixels.Count)];
            cellsWithPixels.Remove(cell);
            //Debug.Log("Try Spawn Poi on cell index " + cell.index);
            // REMOVE EMPTY
            if (cell.deepPixelsCoordinates.Count > cellSize * cellSize * 0.9f)
            {
                //cellsWithPixels.Remove(cell);
                continue;
            }

            
            // CENTER SPAWN
            {
                Vector2Int centerSpawn = cell.centerPixel;
                if (cell.waterPixelsCoordinates.Contains(centerSpawn))
                {
                    SaveNewPoi(centerSpawn, 1, colourMap);
                    cell.waterPixelsCoordinates.Remove(centerSpawn);    
                } 
                else if (cell.sandPixelsCoordinates.Contains(centerSpawn))
                {
                    SaveNewPoi(centerSpawn, 2, colourMap);
                    cell.sandPixelsCoordinates.Remove(centerSpawn);    
                } 
                else if (cell.grassPixelsCoordinates.Contains(centerSpawn))
                {
                    SaveNewPoi(centerSpawn, 3, colourMap);
                    cell.grassPixelsCoordinates.Remove(centerSpawn);    
                } 
                else if (cell.junglePixelsCoordinates.Contains(centerSpawn))
                {
                    SaveNewPoi(centerSpawn, 4, colourMap);
                    cell.junglePixelsCoordinates.Remove(centerSpawn);    
                } 
                else if (cell.rocksPixelsCoordinates.Contains(centerSpawn))
                {
                    SaveNewPoi(centerSpawn, 5, colourMap);
                    cell.rocksPixelsCoordinates.Remove(centerSpawn);    
                } 
                else if (cell.cliffsPixelsCoordinates.Contains(centerSpawn))
                {
                    SaveNewPoi(centerSpawn, 6, colourMap);
                    cell.cliffsPixelsCoordinates.Remove(centerSpawn);    
                } 
                else if (cell.snowPixelsCoordinates.Contains(centerSpawn))
                {
                    SaveNewPoi(centerSpawn, 7, colourMap);
                    cell.snowPixelsCoordinates.Remove(centerSpawn);    
                } 
            }
            
            // SNOW SECRET
            if (cell.snowPixelsCoordinates.Count > 0 && cell.snowPixelsCoordinates.Count < cellSize * cellSize * 0.5f)
            {
                int r = Random.Range(0, cell.snowPixelsCoordinates.Count);
                SaveNewPoi(cell.snowPixelsCoordinates[r], 7, colourMap);
                cell.snowPixelsCoordinates.RemoveAt(r);
            }
            
            // DEEP SECRET
            if (cell.deepPixelsCoordinates.Count > 0 && cell.deepPixelsCoordinates.Count < cellSize * cellSize * 0.5f)
            {
                int r = Random.Range(0, cell.deepPixelsCoordinates.Count);
                SaveNewPoi(cell.deepPixelsCoordinates[r], 0, colourMap);
                cell.deepPixelsCoordinates.RemoveAt(r);
            }
            
            // SAND SECRET
            if (cell.sandPixelsCoordinates.Count > cellSize * cellSize * 0.3f)
            {
                int r = Random.Range(0, cell.sandPixelsCoordinates.Count);
                SaveNewPoi(cell.sandPixelsCoordinates[r], 2, colourMap);
                cell.sandPixelsCoordinates.RemoveAt(r);
            }
            // CLIFFS SECRET
            if (cell.cliffsPixelsCoordinates.Count > cellSize * cellSize * 0.3f)
            {
                int r = Random.Range(0, cell.cliffsPixelsCoordinates.Count);
                SaveNewPoi(cell.cliffsPixelsCoordinates[r], 6, colourMap);
                cell.cliffsPixelsCoordinates.RemoveAt(r);
            }
            
            // ROCKS SECRET
            if (cell.rocksPixelsCoordinates.Count > cellSize * cellSize * 0.3f)
            {
                int r = Random.Range(0, cell.rocksPixelsCoordinates.Count);
                SaveNewPoi(cell.rocksPixelsCoordinates[r], 5, colourMap);
                cell.rocksPixelsCoordinates.RemoveAt(r);
            }
            
            // JUNGLE SECRET
            if (cell.junglePixelsCoordinates.Count > cellSize * cellSize * 0.3f)
            {
                int r = Random.Range(0, cell.junglePixelsCoordinates.Count);
                SaveNewPoi(cell.junglePixelsCoordinates[r], 4, colourMap);
                cell.junglePixelsCoordinates.RemoveAt(r);
            }
            
            // GRASS SECRET
            if (cell.grassPixelsCoordinates.Count > cellSize * cellSize * 0.3f)
            {
                int r = Random.Range(0, cell.grassPixelsCoordinates.Count);
                SaveNewPoi(cell.grassPixelsCoordinates[r], 3, colourMap);
                cell.grassPixelsCoordinates.RemoveAt(r);
            }
            // WATER SECRET
            if (cell.waterPixelsCoordinates.Count > 0 && cell.waterPixelsCoordinates.Count < cellSize * cellSize * 0.5f)
            {
                int r = Random.Range(0, cell.waterPixelsCoordinates.Count);
                SaveNewPoi(cell.waterPixelsCoordinates[r], 1, colourMap);
                cell.waterPixelsCoordinates.RemoveAt(r);
            }
            
            // SPAWN IN SNOW
            if (cell.snowPixelsCoordinates.Count > 0 && snowPoiLeft > 0)
            {
                int r = Random.Range(0, cell.snowPixelsCoordinates.Count);
                r = Mathf.RoundToInt(cell.snowPixelsCoordinates.Count / 2);
                SaveNewPoi(cell.snowPixelsCoordinates[r], 7, colourMap);
                cell.snowPixelsCoordinates.RemoveAt(r);
                snowPoiLeft--;
                continue;
            }
            
            // SPAWN IN DEEP
            if (cell.deepPixelsCoordinates.Count > 0 && deepPoiLeft > 0)
            {
                int r = Random.Range(0, cell.deepPixelsCoordinates.Count);
                r = Mathf.RoundToInt(cell.deepPixelsCoordinates.Count / 2);
                SaveNewPoi(cell.deepPixelsCoordinates[r], 0, colourMap);
                cell.deepPixelsCoordinates.RemoveAt(r);
                deepPoiLeft--;
                continue;
            }
            // SPAWN IN CLIFFS
            if (cell.cliffsPixelsCoordinates.Count > 200 && cliffsPoiLeft > 0)
            {
                int r = Random.Range(0, cell.cliffsPixelsCoordinates.Count);
                r = Mathf.RoundToInt(cell.cliffsPixelsCoordinates.Count / 2);
                SaveNewPoi(cell.cliffsPixelsCoordinates[r], 6, colourMap);
                cell.cliffsPixelsCoordinates.RemoveAt(r);
                cliffsPoiLeft--;
                continue;
            }
            // SPAWN IN ROCKS
            if (cell.rocksPixelsCoordinates.Count > 200 && rocksPoiLeft > 0)
            {
                int r = Random.Range(0, cell.rocksPixelsCoordinates.Count);
                r = Mathf.RoundToInt(cell.rocksPixelsCoordinates.Count / 2);
                SaveNewPoi(cell.rocksPixelsCoordinates[r], 5, colourMap);
                cell.rocksPixelsCoordinates.RemoveAt(r);
                rocksPoiLeft--;
                continue;
            }
            // SPAWN IN JUNGLE
            if (cell.junglePixelsCoordinates.Count > 200 && junglesPoiLeft > 0)
            {
                int r = Random.Range(0, cell.junglePixelsCoordinates.Count);
                r = Mathf.RoundToInt(cell.junglePixelsCoordinates.Count / 2);
                SaveNewPoi(cell.junglePixelsCoordinates[r], 4, colourMap);
                cell.junglePixelsCoordinates.RemoveAt(r);

                junglesPoiLeft--;
                continue;
            }
            
            // SPAWN IN GRASS
            if (cell.grassPixelsCoordinates.Count > 0)
            {
                int r = Random.Range(0, cell.grassPixelsCoordinates.Count);
                r = Mathf.RoundToInt(cell.grassPixelsCoordinates.Count / 2);
                SaveNewPoi(cell.grassPixelsCoordinates[r], 3, colourMap);
                cell.grassPixelsCoordinates.RemoveAt(r);
                continue;
            }
            // SPAWN IN SAND
            if (cell.sandPixelsCoordinates.Count > 0)
            {
                int r = Random.Range(0, cell.sandPixelsCoordinates.Count);
                r = Mathf.RoundToInt(cell.sandPixelsCoordinates.Count / 2);
                SaveNewPoi(cell.sandPixelsCoordinates[r], 2, colourMap);
                cell.sandPixelsCoordinates.RemoveAt(r);
                continue;
            }
            // SPAWN IN WATER
            if (cell.waterPixelsCoordinates.Count > 0)
            {
                int r = Random.Range(0, cell.waterPixelsCoordinates.Count);
                r = Mathf.RoundToInt(cell.waterPixelsCoordinates.Count / 2);
                SaveNewPoi(cell.waterPixelsCoordinates[r], 1, colourMap);
                cell.waterPixelsCoordinates.RemoveAt(r);
            }
        }
        #endregion

        #region scale POI and find the highest one
        
        float _maxHeight = 0;
        var highestPoi = pointsOfInterest[0];
        poisCoordinates.Add(pointsOfInterest[0].coordinates[0]);
        // scale them up 
        for (int i = pointsOfInterest.Count - 1; i >= 0; i--)
        {
            var poi = pointsOfInterest[i];
            if (i > 0)
                poisCoordinates.Add(poi.coordinates[0]);
            
            for (var y = poi.coordinates[0].y - PoiScaler; y < poi.coordinates[0].y + PoiScaler; ++y)
            {
                for (var x= poi.coordinates[0].x - PoiScaler; x < poi.coordinates[0].x + PoiScaler; ++x)
                {
                    if ( x < 0 || x > mapWidth || y < 0 || y > mapHeight)
                        continue;
                    for (int j = 0; j < pointsOfInterest.Count; j++)
                    {
                        if (poi == pointsOfInterest[j])
                            continue;
                        if (j >= i)
                            break;
                        
                        if (pointsOfInterest[j].coordinates.Contains(new Vector2Int(x, y)))
                        {
                            for (int k = 0; k < pointsOfInterest[j].coordinates.Count; k++)
                            {
                                if (addScaledPoiPointsToLists && !poi.coordinates.Contains(pointsOfInterest[j].coordinates[k]))
                                    poi.coordinates.Add(pointsOfInterest[j].coordinates[k]);
                            }
                            pointsOfInterest.RemoveAt(j);
                            break;
                        }
                    }

                    //NoiseMap[x, y] = NoiseMap[poi.coordinates[0].x, poi.coordinates[0].y];
                    
                    if (i == 0)
                        colourMap[y * mapWidth + x] = new Color(0f, 0.75f, 0.24f);
                    else
                        colourMap[y * mapWidth + x] = new Color(1f, 0.01f, 0f);
                    
                    if (addScaledPoiPointsToLists)
                        poi.coordinates.Add(new Vector2Int(x,y));
                }   
            }

            if (NoiseMap[poi.coordinates[0].x, poi.coordinates[0].y] > _maxHeight)
            {
                _maxHeight = NoiseMap[poi.coordinates[0].x, poi.coordinates[0].y];
                highestPoi = poi;
            }
        }
        #endregion

        #region set boss spawner and set it as the last POI
            bossSpawnPoint = new PointOfInterest();
            bossSpawnPoint.coordinates = new List<Vector2Int>();
            bossSpawnPoint.name = "Boss Spawner";
            bossSpawnPoint.regionIndex = highestPoi.regionIndex;
            highestPoi.name = bossSpawnPoint.name;
            for (int h = highestPoi.coordinates.Count - 1; h >= 0; h--)
            {
                bossSpawnPoint.coordinates.Add(highestPoi.coordinates[h]);   
                colourMap[highestPoi.coordinates[h].y * mapWidth + highestPoi.coordinates[h].x] = new Color(0.43f, 0f, 0.36f);
            }

            pointsOfInterest.Remove(highestPoi);
            pointsOfInterest.Add(highestPoi);
            poisCoordinates.Remove(highestPoi.coordinates[0]);
            poisCoordinates.Add(highestPoi.coordinates[0]);
        #endregion
    }

    
    IEnumerator GenerateObstacles(Color[] colorMap)
    {
        System.Random pseudoRandom = new System.Random(seed);
        ObstaclesMap = new int[mapWidth,mapHeight];
        
        // create noise map
        for (int x = 0; x < mapWidth; x ++) {
            for (int y = 0; y < mapHeight; y ++) {
                if (x == 0 || x == mapWidth-1 || y == 0 || y == mapHeight -1) 
                {
                    ObstaclesMap[x,y] = 0;
                }
                else if (NoiseMap[x,y] < regions[0].height)
                {
                    ObstaclesMap[x,y] = 0;
                }
                else if (IsPointInsidePOI(new Vector2Int(x, y)))
                {
                    ObstaclesMap[x,y] = 0;
                }
                else
                {
                    ObstaclesMap[x, y] = pseudoRandom.Next(0, 100) < randomFillPercent ? 1 : 0;
                }
            }
        }

        
        // smooth
        for (int i = 0; i < obstaclesSmoothIterations; i++)
        {
            for (int x = 0; x < mapWidth; x ++) {
                for (int y = 0; y < mapHeight; y ++) {
                    int neighbourWallTiles = GetSurroundingWallCount(x,y);

                    if (neighbourWallTiles > 4)
                        ObstaclesMap[x,y] = 1;
                    else if (neighbourWallTiles < 4)
                        ObstaclesMap[x,y] = 0;
                }
            }   
            yield return null;
        }
        
        // fill colourMap
        
        for (int x = 0; x < mapWidth; x ++) {
            for (int y = 0; y < mapHeight; y ++) {
                Color newColor = Color.clear;
                if (ObstaclesMap[x, y] == 1)
                {
                    obstaclesCoordinates.Add(new Vector2Int(x,y));
                    if (visualizeObstacles)
                        newColor = Color.black;
                }

                colorMap[y * mapWidth + x] = newColor;
            }
        }
    }

    bool IsPointInsidePOI(Vector2Int newPoint)
    {
        for (int i = 0; i < pointsOfInterest.Count; i++)
        {
            if (pointsOfInterest[i].coordinates.Contains(newPoint))
                return true;
        }

        return false;
    }
    
    int GetSurroundingWallCount(int gridX, int gridY) 
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX ++) 
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY ++) 
            {
                if (neighbourX >= 0 && neighbourX <  mapWidth && neighbourY >= 0 && neighbourY < mapHeight) 
                {
                    if (neighbourX != gridX || neighbourY != gridY) 
                    {
                        wallCount += ObstaclesMap[neighbourX,neighbourY];
                    }
                }
                else 
                {
                    wallCount ++;
                }
            }
        }
        return wallCount;
    }
    

    void SaveNewPoi(Vector2Int pixelCoords, int regionIndex, Color[] colourMap)
    {
        colourMap[pixelCoords.y * mapWidth + pixelCoords.x] = Color.red;
            
        pointsOfInterest.Add(new PointOfInterest());
        pointsOfInterest[pointsOfInterest.Count - 1].name = "Unknown Place in " + pixelsByRegions[regionIndex].name;
        pointsOfInterest[pointsOfInterest.Count - 1].regionIndex = regionIndex;
        pointsOfInterest[pointsOfInterest.Count - 1].coordinates = new List<Vector2Int>();
        pointsOfInterest[pointsOfInterest.Count - 1].coordinates.Add(pixelCoords);
    }
    
    void SavePlayerSpawner(Color[] colourMap)
    {
        int x = mapWidth;
        playerSpawnPoint = new PointOfInterest();
        playerSpawnPoint.coordinates = new List<Vector2Int>();
        playerSpawnPoint.coordinates.Add(pixelsByRegions[2].pixelsCoordinates[Random.Range(0,pixelsByRegions[2].pixelsCoordinates.Count)]);
        
        /*
        for (int i = 0; i < pixelsByRegions[2].pixelsCoordinates.Count; i++)
        {
            if (pixelsByRegions[2].pixelsCoordinates[i].x < x)
            {
                x = pixelsByRegions[2].pixelsCoordinates[i].x;
                playerSpawnPoint.coordinates[0] = pixelsByRegions[2].pixelsCoordinates[i];
            }
        }*/
        
        colourMap[playerSpawnPoint.coordinates[0].y * mapWidth + playerSpawnPoint.coordinates[0].x] = new Color(0.94f, 0f, 1f);
        playerSpawnPoint.name = "Player Spawner";
        playerSpawnPoint.regionIndex = 2;
        
        pointsOfInterest.Add(new PointOfInterest());
        pointsOfInterest[pointsOfInterest.Count - 1].name = "PlayerSpawner " + pixelsByRegions[2].name;
        pointsOfInterest[pointsOfInterest.Count - 1].regionIndex = 2;
        pointsOfInterest[pointsOfInterest.Count - 1].coordinates = new List<Vector2Int>();
        pointsOfInterest[pointsOfInterest.Count - 1].coordinates.Add(playerSpawnPoint.coordinates[0]);
    }
    
    public Vector2Int FindPlayerSpawner()
    {
        return playerSpawnPoint.coordinates[0];
    }
    
    private void OnValidate()
    {
        if (mapWidth < 1)
            mapWidth = 1;
        if (noiseScale < 0.001)
            noiseScale = 0.001f;
        if (mapHeight < 1)
            mapHeight = 1;
        if (lacunarity < 1)
            lacunarity = 1;
        if (octaves < 1)
            octaves = 1;
        if (smooth < 0.7f)
            smooth = 0.7f;
        if (smooth > 1)
            smooth = 1;
        if (randomFillPercent > 100)
            randomFillPercent = 100;
        if (randomFillPercent < 1)
            randomFillPercent = 1;
    }

    public Color GetColorByHeight(float height)
    {
        for (int i = 0; i < regions.Length; i++)
        {
            if (height <= regions[i].height)
            {
                return regions[i].colour;
            }
        }
        return Color.black;
    }

    public int GetRegionIndexByHeight(float height)
    {
        for (int i = 1; i < regions.Length; i++)
        {
            if (height <= regions[i].height)
            {
                return i;
            }
        }

        return 0;
    }

    public AssetReference GetRoadTile(int x, int y)
    {
        var coord = new Vector2Int(x, y);
        if (pathsCoordinates.Contains(coord))
        {
            for (int i = 0; i < pixelsByRegions.Count; i++)
            {
                if (pixelsByRegions[i].pixelsCoordinates.Contains(coord) && regions[i].roadAssetReferenceList.Count > 0)
                    return regions[i].roadAssetReferenceList[Random.Range(0, regions[i].roadAssetReferenceList.Count)];
            }
        }
        
        return null;
    }
    public bool IsPath(int x, int y)
    {
        var coord = new Vector2Int(x, y);
        if (pathsCoordinates.Contains(coord))
        {
            return true;
        }
        
        return false;
    }
}

[Serializable]
public struct TerrainType
{
    public string name;
    public int pointsOfInterestAmountMin;
    public int pointsOfInterestAmountMax;
    public float height;
    public int movementPenalty;
    public Color colour;
    public List<AssetReference> tileAssetReferenceList;
    public List<AssetReference> roadAssetReferenceList;
}

[Serializable]
public class PixelsByRegion
{
    public string name;
    public List<Vector2Int> pixelsCoordinates = new List<Vector2Int>();
}

[Serializable]
public class PointOfInterest
{
    public string name;
    public int regionIndex;
    public List<Vector2Int> coordinates;
}

[Serializable]
public class CellWithPixels
{
    public int index = 0;
    public Vector2Int centerPixel;
    public List<Vector2Int> deepPixelsCoordinates;
    public List<Vector2Int> waterPixelsCoordinates;
    public List<Vector2Int> sandPixelsCoordinates;
    public List<Vector2Int> grassPixelsCoordinates;
    public List<Vector2Int> junglePixelsCoordinates;
    public List<Vector2Int> rocksPixelsCoordinates;
    public List<Vector2Int> cliffsPixelsCoordinates;
    public List<Vector2Int> snowPixelsCoordinates;
}
