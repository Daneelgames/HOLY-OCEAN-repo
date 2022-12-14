using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetLineRendererToRaycast : MonoBehaviour
{
    [SerializeField] private float rayDistance = 100;
    [SerializeField] private Transform pointVisual;
    [SerializeField] private LayerMask raycastMask;
    [SerializeField] private Vector2 scaleMinMax = new Vector2(0.01f, 0.3f);
    private float currentDistance;
    private void OnEnable()
    {
        pointVisual.parent = null;
    }

    void Update()
    {
        if (Physics.Raycast(transform.position, transform.forward, out var hit, rayDistance, raycastMask))
        {
            pointVisual.position = hit.point;
            currentDistance = Vector3.Distance(pointVisual.position, transform.position);
            var scaler = currentDistance / rayDistance;
            pointVisual.localScale = Vector3.Lerp(Vector3.one * scaleMinMax.x, Vector3.one * scaleMinMax.y, scaler);
        }
        else
        {
            pointVisual.position = transform.position + transform.forward * 10000;
        }
    }
}
