using System;
using System.Collections;
using System.Collections.Generic;
using MrPink;
using MrPink.Health;
using MrPink.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _src.Scripts.Data
{
    [CreateAssetMenu(fileName = "ProcLevelData", menuName = "ScriptableObjects/ProcLevelData", order = 1)]
    public class ProcLevelData : ScriptableObject
    {
        [Header("Settings used in game")]
        public string levelName = "GOLDENDOLA";
        
        public bool spawnWalls = true;
        public bool spawnLadders = true;
        
        public Vector2Int levelsPosMinMaxX = new Vector2Int(-10, 10);
        public Vector2Int levelsPosMinMaxZ = new Vector2Int(-10, 10);
        public Vector2Int levelsScaleMinMaxX = new Vector2Int(3, 10);
        public Vector2Int levelsScaleMinMaxZ = new Vector2Int(3, 10);
        public int additionalSmallBuildingsAmount = 3;
        public Vector2Int additionalBuildingsScaleMinMaxX = new Vector2Int(4, 8);
        public Vector2Int additionalBuildingsScaleMinMaxY = new Vector2Int(5, 10);
        public Vector2Int additionalBuildingsScaleMinMaxZ = new Vector2Int(4, 8);
        
        public GameObject levelGoalPrefab;
        public TileHealth tilePrefab;
        public TileHealth tileWallPrefab;
        public TileHealth tileWallThinPrefab;
        public GameObject explosiveBarrelPrefab;
        public GrindRail grindRailsPrefab;
        
        public List<int> levelsHeights = new List<int>();
        
        public int explosiveBarrelsAmount = 2;
        public Vector2Int enemiesPerRoomMinMax = new Vector2Int(2,2);
        public Vector2Int npcsPerMainBuildingRoomMinMax = new Vector2Int(1,2);
        public int desertBeastsSpawnAmount = 0;
        public HealthController mrCaptainPrefabToSpawn;
        public Vector2Int grindRailsPerLevelMinMax = new Vector2Int(1, 2);
        public Vector2Int propsPerLevelMinMax = new Vector2Int(1, 10);
        public Vector2Int lootPerLevelMinMax = new Vector2Int(1, 5);
        public Vector2Int stairsDistanceMinMax = new Vector2Int(5, 10);
        public Vector2Int thinWallsPerLevelMinMax = new Vector2Int(1, 10);
        public Vector2 distanceToCutCeilingUnderStairsMinMax = new Vector2(1,5);

        public List<Tool> toolsInShop;

        public List<LevelEvent> levelEvents;
    }

    [Serializable]
    public class LevelEvent
    {
        public List<Condition> conditions;
        public List<ScriptedEvent> events;
    }

    [Serializable]
    public class Condition
    {
        public enum ConditionType
        {
            DistanceIsBigger, DistanceIsSmaller, PlayerIsDead
        }

        public ConditionType conditionType = ConditionType.DistanceIsSmaller;

        [Header("Distance Comparing")]
        public Transform transformA;
        public int actorIdA = -1;
        public Transform transformB;
        public int actorIdB = -1;
        public float distanceToCompare = 5;
    }
}