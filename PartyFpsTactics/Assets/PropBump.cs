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
        var _rb = gameObject.AddComponent<Rigidbody>();
        _rb.isKinematic = false;
        _rb.useGravity = true;
        _rb.drag = 1;
        _rb.angularDrag = 1;

        rb = _rb;
    }
}
