using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _src.Scripts.Data
{
    [CreateAssetMenu(fileName = "CharacterSubtitlesData", menuName = "ScriptableObjects/CharacterSubtitlesData",
        order = 1)]

    [Serializable]
    public class CharacterSubtitlesData : ScriptableObject
    {
        public List<TextAudio> phrases;
        [Serializable]
        public struct TextAudio
        {
            public string messageText;
            public AudioClip messageAudio;
            public bool RunEvent;
            [ShowIf("RunEvent")]public ScriptedEvent eventToRun;
        }

    }
}