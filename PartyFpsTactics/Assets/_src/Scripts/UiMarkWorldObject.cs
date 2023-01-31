using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using MrPink;
using Sirenix.OdinInspector;
using UnityEngine;

public class UiMarkWorldObject : NetworkBehaviour
{
    [SerializeField] private string markerText = "?";
    [SerializeField] private Color markColor = new Color(1f, 0.82f, 0f);
    [SerializeField] private bool hideOnOwner = false;
    [SerializeField] private bool hideOnDistanceToLocalPlayer = false;
    [ShowIf("hideOnDistanceToLocalPlayer")][SerializeField] private float distanceToLocalPlayer = 300;
    private bool showing = false;
    
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (base.IsOwner && hideOnOwner)
        {
            return;
        }
        if (showing == false)
            QuestMarkers.Instance.AddMarker(transform, markColor, markerText);

        if (hideOnDistanceToLocalPlayer)
            StartCoroutine(CheckDistanceToPlayer());
    }

    IEnumerator CheckDistanceToPlayer()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            var d = Vector3.Distance(transform.position, Game.LocalPlayer.Position);
            if (d < distanceToLocalPlayer)
            {
                if (!showing) continue;
                QuestMarkers.Instance.RemoveMarker(transform);
                showing = false;
            }
            else
            {
                if (showing) continue;
                QuestMarkers.Instance.AddMarker(transform, markColor, markerText);
                showing = true;
            }
        }
    }
    

    private void OnDestroy()
    {
        showing = false;
        QuestMarkers.Instance.RemoveMarker(transform);
    }
    private void OnDisable()
    {
        showing = false;
        QuestMarkers.Instance.RemoveMarker(transform);    
    }
}
