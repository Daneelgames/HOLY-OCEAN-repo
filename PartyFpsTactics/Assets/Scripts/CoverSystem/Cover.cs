using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cover : MonoBehaviour
{
    public List<CoverSpot> coverSpotsActive;
    public List<CoverSpot> coverSpotsList;
    public bool Initialized = false;
    
    [Header("Test")]
    public Transform testTarget;
    private void Start()
    {
        CoverSystem.Instance.covers.Add(this);
    }

    private void OnDestroy()
    {
        if (CoverSystem.Instance.covers.Contains(this))
            CoverSystem.Instance.covers.Remove(this);
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < coverSpotsActive.Count; i++)
        {
            if (coverSpotsActive[i] == null)
                continue;
            
            Gizmos.DrawWireCube(coverSpotsActive[i].transform.position + Vector3.up * 0.25f, Vector3.one / 2);
        }
    }

    [ContextMenu("TestGoodSpotsAgainstTarget")]
    public void TestGoodSpotsAgainstTarget()
    {
        ToggleSpot(0, true);
        ToggleSpot(1, true);
        ToggleSpot(2, true);
        ToggleSpot(3, true);
        var newPoints = GetGoodCoverSpots(transform, testTarget);
        ToggleSpot(0, false);
        ToggleSpot(1, false);
        ToggleSpot(2, false);
        ToggleSpot(3, false);
        for (int i = 0; i < newPoints.Count; i++)
        {
            ToggleSpot(coverSpotsList.IndexOf(newPoints[i]), true);
        }
    }
    
    public List<CoverSpot> GetGoodCoverSpots(Transform requester, Transform targetToCoverFrom)
    {
        List<CoverSpot> goodCovers = new List<CoverSpot>();
        for (int i = 0; i < coverSpotsActive.Count; i++)
        {
            if (coverSpotsActive[i].Occupator != null)
                continue;
            
            if (Application.isPlaying)
            {
                if (Vector3.Distance(targetToCoverFrom.position, coverSpotsActive[i].transform.position) <
                    CoverSystem.Instance.minDistanceToEnemy)
                    continue;

                if (Vector3.Distance(requester.position, coverSpotsActive[i].transform.position) >
                    CoverSystem.Instance.maxDistanceFromRequester)
                    continue;
                if (Vector3.Distance(requester.position, coverSpotsActive[i].transform.position) <
                    CoverSystem.Instance.minDistanceFromRequester)
                    continue;
            }
            else
            {
                if (Vector3.Distance(targetToCoverFrom.position, coverSpotsActive[i].transform.position) < 3)
                    continue;

                if (Vector3.Distance(requester.position, coverSpotsActive[i].transform.position) > 30)
                    continue;
                if (Vector3.Distance(requester.position, coverSpotsActive[i].transform.position) < 5)
                    continue;
            }
            
            Vector3 targetDirection = targetToCoverFrom.position - coverSpotsActive[i].transform.position;
            Vector3 coverDirection = transform.position - coverSpotsActive[i].transform.position;

            if (Vector3.Dot(coverDirection, targetDirection) > 0.5f)
            {
                goodCovers.Add(coverSpotsActive[i]);
            }
        }

        return goodCovers;
    }

    
    void ToggleSpot(int index)
    {
        var cover = Selection.activeTransform.GetComponent<Cover>();
        if (cover)
        {
            if (cover.coverSpotsActive.Contains(cover.coverSpotsList[index]))
            {
                cover.coverSpotsActive.Remove(cover.coverSpotsList[index]);
            }
            else
            {
                cover.coverSpotsActive.Add(cover.coverSpotsList[index]);
            }
        }
    }
    public void ToggleSpot(int index, bool add)
    {
        if (add && coverSpotsActive.Contains(coverSpotsList[index]) == false)
            coverSpotsActive.Add(coverSpotsList[index]);
        else if (!add && coverSpotsActive.Contains(coverSpotsList[index]))
            coverSpotsActive.Remove(coverSpotsList[index]);
        
    }
}
[CustomEditor(typeof(CoverEditorHelper))]
class CoverCustomEditor : Editor {
    public override void OnInspectorGUI() {
        if(GUILayout.Button("All On"))
        {
            var cover = Selection.activeTransform.GetComponent<Cover>();
            if (cover)
            {
                cover.ToggleSpot(0, true);
                cover.ToggleSpot(1, true);
                cover.ToggleSpot(2, true);
                cover.ToggleSpot(3, true);
                SceneView.RepaintAll();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
        if(GUILayout.Button("All Off"))
        {
            var cover = Selection.activeTransform.GetComponent<Cover>();
            if (cover)
            {
                cover.ToggleSpot(0, false);
                cover.ToggleSpot(1, false);
                cover.ToggleSpot(2, false);
                cover.ToggleSpot(3, false);
                SceneView.RepaintAll();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
        if(GUILayout.Button("Spot 0"))
        {
            var cover = Selection.activeTransform.GetComponent<Cover>();
            if (cover)
            {
                cover.ToggleSpot(0, false);
                SceneView.RepaintAll();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
        if(GUILayout.Button("Spot 1"))
        {
            var cover = Selection.activeTransform.GetComponent<Cover>();
            if (cover)
            {
                cover.ToggleSpot(1, false);
                SceneView.RepaintAll();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
        if(GUILayout.Button("Spot 2"))
        {
            var cover = Selection.activeTransform.GetComponent<Cover>();
            if (cover)
            {
                cover.ToggleSpot(2, false);
                SceneView.RepaintAll();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
        if(GUILayout.Button("Spot 3"))
        {
            var cover = Selection.activeTransform.GetComponent<Cover>();
            if (cover)
            {
                cover.ToggleSpot(3, false);
                SceneView.RepaintAll();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
    }

}
