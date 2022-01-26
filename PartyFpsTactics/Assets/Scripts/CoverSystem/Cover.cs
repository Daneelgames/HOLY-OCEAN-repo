using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Cover : MonoBehaviour
{
    [Range(1,3)]
    public int coverHeight = 1;
    public List<Transform> coverSpotsActive;
    public List<Transform> coverSpotsList;


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
            Gizmos.DrawWireCube(coverSpotsActive[i].position + Vector3.up * 0.25f, Vector3.one / 2);
        }
    }

    public List<Transform> GetGoodCoverSpots(Transform targetToCoverFrom)
    {
        List<Transform> goodCovers = new List<Transform>();
        for (int i = 0; i < coverSpotsActive.Count; i++)
        {
            Vector3 targetDirection = targetToCoverFrom.position - coverSpotsActive[i].position;
            Vector3 coverDirection = transform.position - coverSpotsActive[i].position;

            if (Vector3.Dot(coverDirection, targetDirection) > 0.9f)
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
