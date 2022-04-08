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
    public float rotateSpeed = 10;
    public float maxVelocityMagnitude = 100;
    public float verticalBalancePower = 10;
    public float upAcceleration = 10;
    [Range(0,90)]
    public float balancingAngleThreshold = 20;

    private float h = 0;
    private float v = 0;
    float z = 0;

    private bool inControl = false;

    private void Start()
    {
        rb.centerOfMass = centerOfMass.localPosition;
    }

    public void SetPlayerInput(float hor, float ver)
    {
        inControl = true;
        h = hor;
        v = ver;

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

    private void Update()
    {
        //if (!inControl)
            return;

        float targetZ = z;
        if (Vector3.Angle(Vector3.up, transform.up) < balancingAngleThreshold)
        {
            targetZ = -h * balancingAngleThreshold;
        }

        z = Mathf.Lerp(z, targetZ, Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, transform.rotation.eulerAngles.y, z), verticalBalancePower * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (!inControl)
            return;
        for (int i = 0; i < wheels.Count; i++)
        {
            wheels[i].motorTorque = v * acceleration;
            
            if (i < 2)
            {
                wheels[i].steerAngle = h * rotateSpeed;
                //Debug.Log("wheels[i].steerAngle" + wheels[i].steerAngle);
            }
        }

        return;
        
        if (v > 0 && rb.velocity.magnitude < maxVelocityMagnitude || v < 0)
            rb.AddForce((transform.forward * v * acceleration + Vector3.up * upAcceleration) * Time.deltaTime, ForceMode.Acceleration);

        rb.AddTorque(Vector3.up * h * rotateSpeed * Time.deltaTime, ForceMode.Acceleration);
    }
}