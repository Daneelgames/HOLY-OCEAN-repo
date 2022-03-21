using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityDebug;
using UnityEngine;

public class GrindRail : MonoBehaviour
{
    public List<Transform> nodes;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (nodes.Count < 2)
            return;
        
        for (int i = 0; i < nodes.Count-1; i++)
        {
            Gizmos.DrawLine(nodes[i].position, nodes[i+ 1].position);
        }
    }
}
