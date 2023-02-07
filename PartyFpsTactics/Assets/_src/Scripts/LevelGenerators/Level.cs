using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityRigidbody;
using IngameDebugConsole;
using MrPink;
using MrPink.Health;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Random = UnityEngine.Random;

namespace _src.Scripts.LevelGenerators
{
    public class Level: MonoBehaviour
    {
        public List<Room> spawnedRooms = new List<Room>();
        public float floorWorldHeight;
        public TileHealth[,,] roomTilesMatrix;
        public List<TileHealth> allTiles = new List<TileHealth>();
        public List<TileHealth> tilesInside = new List<TileHealth>();
        public List<TileHealth> tilesWalls = new List<TileHealth>();
        public List<TileHealth> tilesFloor = new List<TileHealth>();
        public List<TileHealth> tilesTop = new List<TileHealth>();
        public Transform spawnedTransform;
        public Vector3 position;
        public Vector3Int size;
        public bool firstFloor = false;

        int wallsInCurrentIslandAmount = 0;
        int floorConnectionsInCurrentIslandAmount = 0;
        private int currentIslandSupports = 0;
        
        public bool spawnProps = true;
        public bool spawnLoot = true;
        public bool spawnUnits = true;
        public bool spawnRooms = true;
        public bool spawnLadders = true;
        public bool spawnNavMesh = true;
        public bool updateClash = true;

        public List<HealthController> uniqueNpcToSpawn = new List<HealthController>();
        public List<HealthController> unitsToSpawn = new List<HealthController>();
        public List<ControlledMachine> controlledMachinesToSpawn = new List<ControlledMachine>();

        public void SetBuildingSettings(BuildingSettings buildingSettings, int levelIndex)
        {
            spawnProps = buildingSettings.spawnProps;
            spawnLoot = buildingSettings.spawnLoot;
            spawnUnits = buildingSettings.spawnUnits;
            spawnRooms = buildingSettings.spawnRooms;
            spawnLadders = buildingSettings.spawnLadders;
            spawnNavMesh = buildingSettings.spawnNavMesh;
            updateClash = buildingSettings.updateClash;
            uniqueNpcToSpawn = new List<HealthController>(buildingSettings.levelsSettings[levelIndex].uniqueNpcsToSpawn);
            unitsToSpawn = new List<HealthController>(buildingSettings.levelsSettings[levelIndex].unitsToSpawn);
            controlledMachinesToSpawn = new List<ControlledMachine>(buildingSettings.levelsSettings[levelIndex].controlledMachinesToSpawn);
        }
        
        public void Init()
        {
            return;
            
            if (updateClash)
                StartCoroutine(CheckTiles());
        }

        private void OnDrawGizmosSelected()
        {
            if (floorWorldHeight > 1)
            {
                for (int i = 0; i < tilesFloor.Count; i++)
                {
                    var tile = tilesFloor[i];

                    if (tile == null)
                        continue;
                    
                    if (!tile.supporterTile)
                    {
                        Gizmos.color = Color.red;
                    }
                    else
                    {
                        Gizmos.color = Color.green;
                    }
                    Gizmos.DrawCube(tile.transform.position + Vector3.down, Vector3.one);
                }
            }
        }

        IEnumerator CheckTiles()
        {
            var closestBuilding = IslandSpawner.Instance.GetClosestTileBuilding(transform.position);
            while (true)
            {
                if (Vector3.Distance(position, Game.LocalPlayer.Position) > 200)
                {
                    yield return new WaitForSecondsRealtime(1);
                    continue;
                }
                
                //find disconnected islands
                var allTilesTemp = new List<TileHealth>(allTiles);
                
                for (int i = allTilesTemp.Count - 1; i >= 0; i--)
                {
                    if (allTilesTemp.Count <= 0)
                        break;
                    
                    
                    if (i <= 0 || i >= allTilesTemp.Count || allTilesTemp[i] == null)
                        continue;
                    
                    /*
                    if (allTilesTemp[i].floorLevelTile)
                        continue;
                        */
                    
                    List<TileHealth> newIsland = new List<TileHealth>();
                    wallsInCurrentIslandAmount = 0;
                    floorConnectionsInCurrentIslandAmount = 0;
                    currentIslandSupports = 0;

                    var tile = allTilesTemp[i];
                    allTilesTemp.Remove(tile);
                    yield return StartCoroutine(AddNeighboursToIsland(tile, newIsland, allTilesTemp,0));

                    if (wallsInCurrentIslandAmount <= 0)
                        continue;

                    bool canCrash = false;
                    if (firstFloor == false)
                    {
                     if (currentIslandSupports * closestBuilding.islandSupportsScalerToClash < wallsInCurrentIslandAmount || 
                         floorConnectionsInCurrentIslandAmount == 0 ||wallsInCurrentIslandAmount > floorConnectionsInCurrentIslandAmount * size.y * 5)
                         canCrash = true;   
                    }
                    else if (currentIslandSupports == 0 || floorConnectionsInCurrentIslandAmount == 0 /*|| currentIslandSupports * LevelGenerator.Instance.islandSupportsScalerToClash < wallsInCurrentIslandAmount ||
                             wallsInCurrentIslandAmount > floorConnectionsInCurrentIslandAmount * size.y * 5*/)
                    canCrash = true;
                    
                    if (canCrash)
                    {
                        Debug.Log("ClashIsland. SUPPORTERS = " + currentIslandSupports + "; walls: " + wallsInCurrentIslandAmount + "; floorConnectionsPoints: " + floorConnectionsInCurrentIslandAmount + "; size.y * 2: " + size.y * 2);
                        StartCoroutine(ClashIsland(newIsland));
                    }
                    #region algorithm
                    // mark this tile as an island, remove it from allTilesTemp
                    // check its connected neighbours
                    // repeat step for all neighbours

                    // once there's no more neighbours, check if island has any connections to floor

                    // if island isn't connected to floor in any point - CLASH the island

                    // if tiles amount in current island is bigger then FloorConnectionPoints * scaler - CLASH the island

                        #endregion
                    yield return null;
                }
                yield return null;
            }
        }
        
        IEnumerator ClashIsland(List<TileHealth> newIsland)
        {
            var closestBuilding = IslandSpawner.Instance.GetClosestTileBuilding(transform.position);
            for (int i = newIsland.Count - 1; i >= 0; i--)
            {
                if (i >= newIsland.Count)
                    continue;

                if (newIsland[i] == null)
                {
                    newIsland.RemoveAt(i);
                }
                else
                {
                    allTiles.Remove(newIsland[i]);
                    roomTilesMatrix[newIsland[i].TileLevelCoordinates.x, newIsland[i].TileLevelCoordinates.y,
                        newIsland[i].TileLevelCoordinates.z] = null;
                }
            }
            
            if (newIsland.Count <= 0)
                yield break;
            
            GameObject islandGo = new GameObject(gameObject.name + "'s Disconnected Island");
            islandGo.transform.position = newIsland[0].transform.position;
            islandGo.transform.rotation = newIsland[0].transform.rotation;

            for (int i = 0; i < newIsland.Count; i++)
            {
                newIsland[i].transform.parent = islandGo.transform;
            }

            var rb = islandGo.AddComponent<Rigidbody>();
            rb.mass = newIsland.Count;
            rb.drag = 1;
            rb.angularDrag = 1;
            rb.AddForce(((rb.transform.position - rb.transform.position - Vector3.up * 10) + Random.insideUnitSphere * 3).normalized * 20, ForceMode.VelocityChange);


            int amount = 0;
            while(newIsland.Count > 0)
            {
                if (newIsland.Count > 100)
                    amount = Random.Range(10, 20);
                else if (newIsland.Count > 30) 
                    amount = Random.Range(5, 10);
                else
                    amount = Random.Range(1, 5);
                
                for (int i = 0; i < amount; i++)
                {
                    if (newIsland.Count <= 0)
                        break;
                    yield return new WaitForSeconds(Random.Range(0.01f, 0.2f));
                    var tile = newIsland[Random.Range(0, newIsland.Count)];
                    newIsland.Remove(tile);
                    if (tile != null)
                    {
                        tile.ActivateRigidbody(100, closestBuilding.tilePhysicsMaterial, false, 150);
                        closestBuilding.AddToDisconnectedTilesFolder(tile.transform);
                    }   
                }
            }
            Destroy(islandGo);
        }
        
        // this method loops until all connected tiles in level found
        IEnumerator AddNeighboursToIsland(TileHealth tile, List<TileHealth> island, List<TileHealth> allTilesTemp, int iterationInFrame)
        {
            iterationInFrame++;
            if (iterationInFrame > 5)
            {
                iterationInFrame = 0;
                yield return null;
            }
            
            if (!tile.floorLevelTile)
                wallsInCurrentIslandAmount++;
            else
            {
                // LOOK SUPPORT FOR FLOOR TILE
                if (firstFloor || tile.supporterTile != null)
                    currentIslandSupports++;
            }
            
            island.Add(tile);
            allTilesTemp.Remove(tile);
            var tileCoords = tile.TileLevelCoordinates;
            for (int i = 0; i < 6; i++)
            {
                int x = tileCoords.x;
                int y = tileCoords.y;
                int z = tileCoords.z;

                switch (i)
                {
                    case 0: x--; break; // left
                    case 1: y++; break; // up
                    case 2: x++; break; // right
                    case 3: y--; break; // down
                    case 4: z++; break; // fwd
                    case 5: z--; break; // back
                }

                if (x < 0 || x >= size.x|| y < 0 || y >= size.y || z < 0 || z >= size.z)
                    continue; // OUT OF BOUNDS

                var connectedNeighbour = roomTilesMatrix[x, y, z];
                
                if (connectedNeighbour != null)
                {
                    if (allTilesTemp.Contains(connectedNeighbour))
                    {
                        if (tile.floorLevelTile != connectedNeighbour.floorLevelTile)
                            floorConnectionsInCurrentIslandAmount++;
                    
                        island.Add(roomTilesMatrix[x,y,z]);
                    
                        yield return StartCoroutine(AddNeighboursToIsland(connectedNeighbour, island, allTilesTemp,iterationInFrame));
                    }
                }
            }
        }
    }
}

