using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropBump : MonoBehaviour
{
    private Rigidbody rb;
    public Rigidbody RB => rb;
    void Start()
    {
        InteractableEventsManager.Instance.AddPropBump(this);
    }

    private void OnDisable()
    {
        InteractableEventsManager.Instance.RemovePropBump(this);
    }
    private void OnDestroy()
    {
        InteractableEventsManager.Instance.RemovePropBump(this);
    }

    public void SetRb()
    {
        if (gameObject == null || gameObject.activeInHierarchy == false)
            return;
        
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
        
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.drag = 1;
        rb.angularDrag = 1;
    }
}
