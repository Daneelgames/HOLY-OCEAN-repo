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
        
        public GameObject levelGoalPrefab;
        public TileHealth tilePrefab;
        public TileHealth tileWallPrefab;
        public TileHealth tileWallThinPrefab;
        public GameObject explosiveBarrelPrefab;
        public GrindRail grindRailsPrefab;
        
        public int explosiveBarrelsAmount = 2;
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
            DistanceIsBigger, DistanceIsSmaller, PlayerIsDead, PlayerIsDriving, PlayerIsCloseToNpc, PlayerIsAlive
        }

        public ConditionType conditionType = ConditionType.DistanceIsSmaller;

        [ShowIf("@(conditionType) == ConditionType.DistanceIsBigger || (conditionType) == ConditionType.DistanceIsSmaller")]
        public Transform transformA;
        [ShowIf("@(conditionType) == ConditionType.DistanceIsBigger || (conditionType) == ConditionType.DistanceIsSmaller")]
        public int actorIdA = -1;
        [ShowIf("@(conditionType) == ConditionType.DistanceIsBigger || (conditionType) == ConditionType.DistanceIsSmaller")]
        public Transform transformB;
        [ShowIf("@(conditionType) == ConditionType.DistanceIsBigger || (conditionType) == ConditionType.DistanceIsSmaller")]
        public int actorIdB = -1;
        [ShowIf("@(conditionType) == ConditionType.DistanceIsBigger || (conditionType) == ConditionType.DistanceIsSmaller || (conditionType) == ConditionType.PlayerIsCloseToNpc")]
        public float distanceToCompare = 5;

        [ShowIf("conditionType", ConditionType.PlayerIsCloseToNpc)]
        public int spawnedQuestNpcId;
    }
}