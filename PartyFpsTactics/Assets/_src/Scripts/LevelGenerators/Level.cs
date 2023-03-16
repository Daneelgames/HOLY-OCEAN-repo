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
        
        public bool spawnLadders = true;

        public List<ControlledMachine> controlledMachnesToSpawn = new List<ControlledMachine>();

        public void SetBuildingSettings(BuildingSettings buildingSettings, int levelIndex)
        {
            spawnLadders = buildingSettings.spawnLadders;
        }
        
        public void Init()
        {
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
    }
}

