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

        public float delayIn = 1;
        public float delayOut = 1;
        [ShowIf("scriptedEventType", ScriptedEventType.SpawnObject)]
        public GameObject prefabToSpawn;

        [ShowIf("scriptedEventType", ScriptedEventType.StartDialogue)]
        public PhoneDialogue dialogueToStart;
    }
    
    public enum ScriptedEventType
    {
        StartDialogue, SpawnObject
    }
}