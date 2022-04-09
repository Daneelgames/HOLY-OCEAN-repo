using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlledVehicle : MonoBehaviour
{
    public Transform sitTransform;
    public Rigidbody rb;
    public Transform centerOfMass;
    public List<WheelCollider> wheels;
    public float acceleration = 100;
    public float slopesHelpScaler = 3;
    [Range(0, 90)]
    public float minAngleToHelpOnSlope = 5;
    public float brakeForce = 500;
    private bool braking = false;
    public float rotateSpeed = 10;
    public float minMaxWheelsAngle = 20;

    public Transform slopeRaycastTransform;
    private float h = 0;
    private float v = 0;
    float z = 0;

    private bool inControl = false;

    private void Start()
    {
        rb.centerOfMass = centerOfMass.localPosition;
    }

    public void SetPlayerInput(float hor, float ver, bool brake)
    {
        inControl = true;
        h = hor;
        v = ver;
        braking = brake;

        /*
        if (v < 0)
            h *= -1;*/
    }

    public void StopMovement()
    {
        inControl = false;
        v = 0;
        h = 0;
        z = 0;
    }

    private void FixedUpdate()
    {
        if (!inControl)
            return;

        float resultAcceleration = acceleration;
        if (Physics.Raycast(slopeRaycastTransform.position, slopeRaycastTransform.forward, out var hit, 2,
            GameManager.Instance.AllSolidsMask))
        {
            if (Vector3.Angle(transform.forward, hit.normal) > minAngleToHelpOnSlope)
            {
                resultAcceleration *= slopesHelpScaler;
            }
        }

        for (int i = 0; i < wheels.Count; i++)
        {
            if (braking)
            {
                wheels[i].brakeTorque = brakeForce;
                wheels[i].motorTorque = 0;
            }
            else
            {
                wheels[i].brakeTorque = 0;
                wheels[i].motorTorque = v * resultAcceleration;
            }

            
            if (i < 2)
            {
                wheels[i].steerAngle = Mathf.Clamp(h * rotateSpeed, -minMaxWheelsAngle, minMaxWheelsAngle);
            }
        }
    }
}