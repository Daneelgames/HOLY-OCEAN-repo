using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestMarkers : MonoBehaviour
{
    public static QuestMarkers Instance;
    
    public List<QuestMark> activeMarks = new List<QuestMark>();
    public QuestMark questMarkPrefab;
    public Transform questMarkersParent;

    private void Awake()
    {
        Instance = this;
    }

    public void AddMarker(Transform markerTarget, Quest quest)
    {
        var mark = Instantiate(questMarkPrefab, questMarkersParent);
        mark.target = markerTarget;
        mark.markerName.text = quest.questName;
        activeMarks.Add(mark);
    }

    public void RemoveMarker(Transform markerTarget)
    {
        for (int i = 0; i < activeMarks.Count; i++)
        {
            if (activeMarks[i].target == markerTarget)
            {
                Destroy(activeMarks[i].gameObject);
                activeMarks.RemoveAt(i);
                return;
            }
        }
    }

    private void Update()
    {
        for (int i = 0; i < activeMarks.Count; i++)
        {
            activeMarks[i].transform.LookAt(activeMarks[i].target);
        }
    }
}
