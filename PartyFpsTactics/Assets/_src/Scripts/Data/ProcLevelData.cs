using System;
using System.Collections;
using System.Collections.Generic;
using MrPink;
using MrPink.Health;
using MrPink.Tools;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

namespace _src.Scripts.Data
{
    [CreateAssetMenu(fileName = "ProcLevelData", menuName = "ScriptableObjects/ProcLevelData", order = 1)]
    public class ProcLevelData : ScriptableObject
    {
        [Header("Settings used in game")]
        public string levelName = "GOLDENDOLA";

        public CharacterSubtitlesData LevelStartCharacterSubtitlesData;
        
        public List<Tool> toolsInShop;

        public List<LevelEvent> levelEvents;
        public List<AssetReference> islandsReferences;
        public List<AssetReference> mobsReferences;
        [SerializeField] private int killsForLevelComplete = 10;
        public int GetTargetHavok => killsForLevelComplete;
        public HealthController boss;

        public AssetReference GetRandomMobReference => mobsReferences[Random.Range(0, mobsReferences.Count)];

        public enum SpawnBossType
        {
            Ocean, Island, Building
        }

        public SpawnBossType spawnBossType;
        public BuildingSettings BuildingSettings;
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
    }
}