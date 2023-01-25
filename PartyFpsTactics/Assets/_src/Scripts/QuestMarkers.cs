using System;
using System.Collections;
using System.Collections.Generic;
using MrPink;
using UnityEngine;

public class QuestMarkers : MonoBehaviour
{
    public static QuestMarkers Instance;
    
    public List<QuestMark> activeMarks = new List<QuestMark>();
    public QuestMark questMarkPrefab;
    public Transform questMarkersParent;
    [SerializeField] private float markerSpeed;
    [SerializeField] private float distanceScaler = 10;

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
    public void AddMarker(Transform markerTarget, Color color, string marketText)
    {
        var mark = Instantiate(questMarkPrefab, questMarkersParent);
        mark.target = markerTarget;
        mark.markerName.color = color;
        mark.markerName.text = marketText;
        activeMarks.Add(mark);
    }

    public void RemoveMarker(Transform markerTarget)
    {
        for (int i = activeMarks.Count - 1; i >= 0; i--)
        {
            if (activeMarks[i] == null)
            {
                activeMarks.RemoveAt(i);
                continue;
            }
            if (activeMarks[i].target == markerTarget)
            {
                if (activeMarks[i].gameObject != null)
                    Destroy(activeMarks[i].gameObject);
                activeMarks.RemoveAt(i);
            }
        }
    }

    private void Update()
    {
        if (Game._instance == null || Game.LocalPlayer == null)
        {
            return;
        }
        if (this != Instance)
        {
            return;
        }
        for (int i = 0; i < activeMarks.Count; i++)
        {
            var marker = activeMarks[i];
            
            if (marker == null || marker.target == null)
                continue;
            var textUI = marker.markerName;
            var target = marker.target;
            var distance = Vector3.Distance(marker.target.position, Game.LocalPlayer.MainCamera.transform.position);
            marker.transform.localScale = Vector3.one / Mathf.Clamp(distance / distanceScaler, 1, 100);

            float minX = textUI.GetPixelAdjustedRect().width / 2;
            float maxX = Screen.width - minX;
            float minY = textUI.GetPixelAdjustedRect().height / 2;   
            float maxY = Screen.height - minY;

            Vector2 pos = Game.LocalPlayer.MainCamera.WorldToScreenPoint(target.position);

            
            // Check if the target is behind us, to only show the icon once the target is in front
            if (Vector3.Dot((target.position - Game.LocalPlayer.MainCamera.transform.position),
                Game.LocalPlayer.MainCamera.transform.forward) < 0)
            {
                // Check if the target is on the left side of the screen
                if (pos.x < Screen.width / 2)
                {
                    // Place it on the right (Since it's behind the player, it's the opposite)
                    pos.x = maxX;
                }
                else
                {
                    // Place it on the left side
                    pos.x = minX;
                }

                pos.y = Screen.height / 2;
            }

            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);

            marker.transform.position =
                Vector3.Lerp(marker.transform.position, pos, markerSpeed * Time.unscaledDeltaTime);
        }
    }
}
