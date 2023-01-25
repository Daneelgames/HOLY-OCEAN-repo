using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using UnityEngine;

public class UiMarkWorldObject : NetworkBehaviour
{
    [SerializeField] private string markerText = "?";
    [SerializeField] private Color markColor = new Color(1f, 0.82f, 0f);
    [SerializeField] private bool hideOnOwner = false;
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (base.IsOwner && hideOnOwner)
        {
            return;
        }
        QuestMarkers.Instance.AddMarker(transform, markColor, markerText);    
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
