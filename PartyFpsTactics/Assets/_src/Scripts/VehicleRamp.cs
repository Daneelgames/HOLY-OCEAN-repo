using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using UnityEngine;

public class VehicleRamp : MonoBehaviour
{
    public Transform ForceTransform;
    public float forceAmount = 2000;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != 11) // vehicle colliders are on interaactiveObjectLayer
            return;

        var bodyPart = other.gameObject.GetComponent<BodyPart>();
        if (!bodyPart)
            return;
        
        var hc = bodyPart.HealthController;
        if (!hc)
            return;
        
        var vehicle = hc.controlledMachine;
        if (!vehicle)
            return;

        vehicle.AddRampForce(forceAmount, ForceTransform.forward);
    }
}
