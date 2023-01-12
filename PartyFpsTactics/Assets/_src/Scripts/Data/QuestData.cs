using System;
using System.Collections;
using System.Collections.Generic;
using _src.Scripts.Data;
using MrPink.Health;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestData", menuName = "ScriptableObjects/QuestData", order = 1)]
public class QuestData : ScriptableObject
{
    public List<Quest> questVariants;
}

[Serializable]
public class Quest
{
    public string questName;
    
    public Quest(Quest templateQuest)
    {
        questName = templateQuest.questName;
    }
}