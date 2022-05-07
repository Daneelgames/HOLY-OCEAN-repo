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
    public List<HealthController> spawnedQuestNpcs = new List<HealthController>();
    
    [Header("EventsOnConditions проверяются, только если выполнены все прошлые ивенты")]
    public List<LevelEvent> eventsOnConditions = new List<LevelEvent>();

    public Quest(Quest templateQuest)
    {
        questName = templateQuest.questName;
        eventsOnConditions = templateQuest.eventsOnConditions;
    }

    public void AddSpawnedHc(HealthController hc)
    {
        spawnedQuestNpcs.Add(hc);
    }
    public void RemoveSpawnedHc(HealthController hc)
    {
        if (spawnedQuestNpcs.Contains(hc))
            spawnedQuestNpcs.Remove(hc);
    }
}
