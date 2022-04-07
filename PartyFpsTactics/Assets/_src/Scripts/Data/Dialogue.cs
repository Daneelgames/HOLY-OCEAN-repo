using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _src.Scripts.Data
{
    [CreateAssetMenu(fileName = "PhoneDialogue", menuName = "ScriptableObjects/PhoneDialogues", order = 1)]
    public class Dialogue : ScriptableObject
    {
        public List<Phrase> phrases;
    }

    [Serializable]
    public class Phrase
    {
        [Range(0.1f, 10)]
        public float delayIn = 1f;
        
        public string messengerName;
        public string messageText;

        public bool waitForPlayerAnswer = true;
        
        public bool answerOnNegative = false;
        [ShowIf("answerOnNegative", true)]
        public string answerOnNegativeText;
        [ShowIf("answerOnNegative", true)]
        [Range(0.1f, 10)] 
        public float delayBeforeReactionOnNegativeAnswer = 1;
        [ShowIf("answerOnNegative", true)]
        [Range(0.1f, 10)] 
        public float delayAfterReactionOnNegativeAnswer = 2;
        
        public bool answerOnPositive = false;
        [ShowIf("answerOnPositive", true)]
        public string answerOnPositiveText;
        [ShowIf("answerOnPositive", true)]
        [Range(0.1f, 10)] 
        public float delayBeforeReactionOnPositiveAnswer = 1;
        [ShowIf("answerOnPositive", true)]
        [Range(0.1f, 10)] 
        public float delayAfterReactionOnPositiveAnswer = 2;
    }
}