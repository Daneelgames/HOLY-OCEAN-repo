using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _src.Scripts.Data
{
    [CreateAssetMenu(fileName = "LevelEvents", menuName = "ScriptableObjects/LevelEvents", order = 1)]
    public class LevelEvents : ScriptableObject
    {
        public List<ScriptedEvent> eventsList;
    }

    [Serializable]
    public class ScriptedEvent
    {
        public ScriptedEventType scriptedEventType = ScriptedEventType.SpawnObject;

        [ShowIf("scriptedEventType", ScriptedEventType.SpawnObject)]
        public GameObject prefabToSpawn;

        [ShowIf("scriptedEventType", ScriptedEventType.StartDialogue)]
        public PhoneDialogue dialogueToStart;
        
        [ShowIf("scriptedEventType", ScriptedEventType.SetCurrentLevel)]
        public int currentLevelToSet;
        
        [ShowIf("scriptedEventType", ScriptedEventType.SpawnObject)]
        [Tooltip("Spawns object inside player's camera at zero local coordinates")]
        public bool spawnInsideCamera = false;
        
        
        [Header("Time")]
        public float delayIn = 1;
        public float delayOut = 1;
    }
    
    public enum ScriptedEventType
    {
        StartDialogue, SpawnObject, DestroyOnInteraction, StartProcScene, StartFlatScene, SetCurrentLevel
    }
}