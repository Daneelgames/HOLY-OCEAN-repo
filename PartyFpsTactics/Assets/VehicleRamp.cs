using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleRamp : MonoBehaviour
{
    public Transform ForceTransform;
    public float forceAmount = 2000;
    private void OnTriggerEnter(Collider other)
    {

        var vehicle = other.gameObject.GetComponent<ControlledVehicle>();
        if (vehicle)
        {
            vehicle.rb.AddForce(ForceTransform.forward * forceAmount, ForceMode.Impulse);
        }
    }
}
