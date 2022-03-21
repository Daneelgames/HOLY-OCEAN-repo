using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _src.Scripts.Data
{
    [CreateAssetMenu(fileName = "ProcLevelData", menuName = "ScriptableObjects/ProcLevelData", order = 1)]
    public class ProcLevelData : ScriptableObject
    {

        public bool spawnWalls = true;
        public bool spawnLadders = true;
        public bool spawnAdditionalTiles = true;
        
        public Vector2Int levelsPosMinMaxX = new Vector2Int(-10, 10);
        public Vector2Int levelsPosMinMaxZ = new Vector2Int(-10, 10);
        public Vector2Int levelsScaleMinMaxX = new Vector2Int(3, 10);
        public Vector2Int levelsScaleMinMaxZ = new Vector2Int(3, 10);
        
        public GameObject levelGoalPrefab;
        public GameObject tilePrefab;
        public GameObject tileWallPrefab;
        public GameObject tileWallThinPrefab;
        public GameObject explosiveBarrelPrefab;
        
        public List<int> levelsHeights = new List<int>();
        
        public int explosiveBarrelsAmount = 2;
        public Vector2Int enemiesPerRoomMinMax = new Vector2Int(2,2);
        
        public Vector2Int coversPerLevelMinMax = new Vector2Int(1, 10);
        public Vector2Int stairsDistanceMinMax = new Vector2Int(5, 10);
        public Vector2Int thinWallsPerLevelMinMax = new Vector2Int(1, 10);
        public Vector2 distanceToCutCeilingUnderStairsMinMax = new Vector2(1,5);

        public List<Tool> toolsInShop;
    }
}