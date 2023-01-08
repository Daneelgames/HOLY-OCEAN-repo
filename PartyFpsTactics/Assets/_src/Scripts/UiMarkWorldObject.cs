using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiMarkWorldObject : MonoBehaviour
{
    [SerializeField] private Color markColor = new Color(1f, 0.82f, 0f);
    void Start()
    {
        QuestMarkers.Instance.AddMarker(transform, markColor);    
    }
    void OnEnable()
    {
        if (QuestMarkers.Instance)
            QuestMarkers.Instance.AddMarker(transform, markColor);    
    }

    private void OnDestroy()
    {
        QuestMarkers.Instance.RemoveMarker(transform);    
    }
    private void OnDisable()
    {
        QuestMarkers.Instance.RemoveMarker(transform);    
    }
}
