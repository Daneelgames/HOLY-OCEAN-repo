using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawGizmoLineForward : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color= Color.red;
        
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 50);
    }
}
