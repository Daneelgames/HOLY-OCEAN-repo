using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class UiMarkWorldObject : NetworkBehaviour
{
    [SerializeField] private Color markColor = new Color(1f, 0.82f, 0f);
   
    public override void OnStartClient()
    {
        base.OnStartClient();
        
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
