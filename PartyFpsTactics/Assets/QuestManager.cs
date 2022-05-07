using System;
using System.Collections;
using System.Collections.Generic;
using _src.Scripts.Data;
using MrPink;
using UnityEngine;
using Random = UnityEngine.Random;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;
    public List<Quest> activeQuests = new List<Quest>();
    public List<Quest> generatedQuests = new List<Quest>();
    public QuestData questTemplates;
    public int questsToGenerate = 5;
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GenerateQuests();
    }


    void GenerateQuests()
    {
        var questsTemplatesTemp = new List<Quest>(questTemplates.questVariants);
        for (int i = 0; i < questsToGenerate; i++)
        {
            if (questsTemplatesTemp.Count <= 0)
                questsTemplatesTemp = new List<Quest>(questTemplates.questVariants);

            int r = Random.Range(0, questsTemplatesTemp.Count);
            var quest = new Quest(questsTemplatesTemp[r]);
            questsTemplatesTemp.RemoveAt(r);
            ShuffleQuest(quest);
            generatedQuests.Add(quest);
        }
    }

    void ShuffleQuest(Quest newQuest)
    {
        /*
        // choose quest giver
        // fill conditions
        for (int i = 0; i < newQuest.eventsOnConditions.Count; i++)
        {
            var subQuest = newQuest.eventsOnConditions[i];
            for (int conditionIndex = 0; conditionIndex < subQuest.conditions.Count; conditionIndex++)
            {
                var condition = subQuest.conditions[conditionIndex];
                switch (condition.conditionType)
                {
                    case Condition.ConditionType.PlayerIsCloseToNpc:
                        
                        break;
                }
            }
        }
        */
    }

    public void StartRandomQuest()
    {
        var randomQuest = generatedQuests[Random.Range(0, generatedQuests.Count)];
        
        activeQuests.Add(randomQuest);

        StartCoroutine(CheckingQuest(randomQuest));
    }

    IEnumerator CheckingQuest(Quest quest)
    {
        for (int i = 0; i < quest.eventsOnConditions.Count; i++)
        {
            yield return StartCoroutine(LevelEventsOnConditions.Instance.CheckingEvent(quest.eventsOnConditions[i], quest));   
        }    
    }

    public void FailQuest(Quest quest)
    {
        // feedback on level failed
        ScoringSystem.Instance.CustomTextMessage("QUEST FAILED");
        ScoringSystem.Instance.ItemFoundSoundLowPitch();
        for (int i = 0; i < quest.spawnedQuestNpcs.Count; i++)
        {
            QuestMarkers.Instance.RemoveMarker(quest.spawnedQuestNpcs[i].visibilityTrigger.transform);
        }
    }
}
