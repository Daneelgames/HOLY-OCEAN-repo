using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetLineRendererToRaycast : MonoBehaviour
{
    [SerializeField] private float rayDistance = 100;
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private LayerMask raycastMask;
    private Vector3 rayPoint;
    private Vector3 rayPointCloser;
    private Vector3 rayPointFar;
    void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, transform.forward, out var hit, rayDistance, raycastMask))
        {
            rayPoint = hit.point;
            rayPointCloser = rayPoint - transform.forward * 0.5f;
            rayPointFar = rayPoint + transform.forward * 0.5f;
        }
    }

    private void Update()
    {
        _lineRenderer.SetPosition(0, rayPointCloser);
        _lineRenderer.SetPosition(1, rayPointFar);
    }
}
