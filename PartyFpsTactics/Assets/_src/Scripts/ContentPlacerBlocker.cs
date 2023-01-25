using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContentPlacerBlocker : MonoBehaviour
{
    [SerializeField] private float blockDistance = 50;
    public float BlockDistance => blockDistance;

    private void Start()
    {
        ContentPlacer.Instance.AddContentBlocker(this);
    }

    private void OnDestroy()
    {
        ContentPlacer.Instance.RemoveContentBlocker(this);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, blockDistance);
    }
}
