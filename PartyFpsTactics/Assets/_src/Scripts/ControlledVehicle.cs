using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using MrPink.PlayerSystem;
using UnityEngine;
using VehicleBehaviour;

public class ControlledVehicle : MonoBehaviour
{
    public WheelVehicle wheelVehicle;
    public Transform sitTransform;
    public Transform sitTransformNpc;
    
    [Header("DONT USE BELOW")]
    //public Rigidbody rb;
    public List<WheelCollider> wheels;
    public float acceleration = 100;
    public float slopesHelpScaler = 3;
    [Range(0, 90)]
    public float minAngleToHelpOnSlope = 5;
    public float brakeForce = 500;
    private bool braking = false;
    public float rotateSpeed = 10;
    public float setRotationStraightThreshold = 90;
    public float minMaxWheelsAngle = 20;


    public Transform slopeRaycastTransform;
    private float h = 0;
    private float v = 0;
    float z = 0;

    private HealthController ownerHc;

    [Header("CRASH")]
    public float cooldownOnDamageOwnerOnCrash = 1;
    private float cooldownOnDamageOwnerOnCrashCurrent = 0;
    public float velocityToDamageCrashThreshold = 10;
    public float contactAngleToDamageCrashThreshold = 10;
    
    [Header("Feedback")] 
    public ParticleSystem bikeMovementParticles;
    ParticleSystem.EmissionModule bikeMovementParticlesEmission;
    public float particlesMaxDriveRate = 200;
    public AudioSource bikeIdleAu;
    public AudioSource bikeDriveAu;
    public AudioSource bikeWheelsAu;
    public AudioSource bikeDriftAu;

    private void Start()
    {
        //bikeMovementParticlesEmission = bikeMovementParticles.emission;

        //StartCoroutine(UpdateBikeMoveFeedback());
    }

    IEnumerator UpdateBikeMoveFeedback()
    {
        var rate = bikeMovementParticlesEmission.rateOverTime;
        while (true)
        {
            if (v > 0)
            {
                rate.constant = Mathf.Lerp(rate.constant, particlesMaxDriveRate, 10 * Time.deltaTime);
                bikeDriveAu.volume += 0.1f;
            }
            else if (v < 0)
            {
                rate.constant = Mathf.Lerp(rate.constant, 0, 10 * Time.deltaTime);
                bikeDriveAu.volume -= 0.1f;
            }
            else
            {
                rate.constant = Mathf.Lerp(rate.constant, 0, 10 * Time.deltaTime);
                bikeDriveAu.volume -= 0.1f;
            }

            /*
            if (rb.velocity.magnitude > 0.5f)
            {
                bikeWheelsAu.volume += 0.1f;
            }
            else
            {
                bikeWheelsAu.volume -= 0.1f;
            }

            if (rb.velocity.magnitude > 0.5f && Vector3.Angle(transform.forward, rb.velocity) > 20 && Vector3.Angle(-transform.forward, rb.velocity) > 20)
            {
                bikeDriftAu.volume += 0.1f;
            }
            else
            {
                bikeDriftAu.volume -= 0.1f;
            }*/

            bikeDriveAu.volume = Mathf.Clamp(bikeDriveAu.volume, 0, 0.5f);
            bikeDriftAu.volume = Mathf.Clamp(bikeDriftAu.volume, 0, 0.5f);
            bikeWheelsAu.volume = Mathf.Clamp(bikeWheelsAu.volume, 0, 0.5f);

            bikeMovementParticlesEmission.rateOverTime = rate;
            yield return new WaitForSeconds(0.1f);
        }
    }


    public void StartPlayerInput()
    {
        wheelVehicle.IsPlayer = true;
        wheelVehicle.Handbrake = false;
        return;
        /*
        rb.drag = 0.1f;
        rb.angularDrag = 0.5f;*/
        SetRotationStraight();
    }

    void SetRotationStraight()
    {
        return;
        if (SetRotationStraightCoroutine != null)
            return;
        
        SetRotationStraightCoroutine = StartCoroutine(SetRotationStraightOverTime());
    }


    private Coroutine SetRotationStraightCoroutine;
    IEnumerator SetRotationStraightOverTime()
    {
        float t = 0;
        float tt = 3f;
        while (t < tt)
        {/*
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, t/tt);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, Quaternion.Euler(new Vector3(rb.rotation.eulerAngles.x, rb.rotation.eulerAngles.y, 0)), t / tt));*/
            t += Time.deltaTime;
            yield return null;
        }

        SetRotationStraightCoroutine = null;
    }
    
    public void StopMovement()
    {
        wheelVehicle.IsPlayer = false;
        wheelVehicle.Handbrake = true;
        ownerHc = null;
        return;
        /*
        rb.drag = 1f;
        rb.angularDrag = 1f;*/
        cooldownOnDamageOwnerOnCrashCurrent = 0;
        for (int i = 0; i < wheels.Count; i++)
        {
            wheels[i].brakeTorque = brakeForce;
            wheels[i].motorTorque = 0;
            wheels[i].steerAngle = 0;
        }
        
        v = 0;
        h = 0;
        z = 0;
    }
    public void SetPlayerInput(float hor, float ver, bool brake)
    {
        return;
        // перекинуть в общий прием инпута от юнитов

        if (cooldownOnDamageOwnerOnCrashCurrent > 0)
            cooldownOnDamageOwnerOnCrashCurrent -= Time.deltaTime;
        
        ownerHc = Player.Health;
        h = hor;
        v = ver;
        braking = brake;

        if (wheels[0].motorTorque > 0 && v < 0)
            braking = true;
        
        /*
        if (v < 0)
            h *= -1;*/
    }

    private void FixedUpdate()
    {
        return;/*
        if (!ownerHc)
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
            if (braking || (rb.velocity.magnitude < 5 && v > -1 && v < 1))
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

        if (Vector3.Angle(transform.up, Vector3.down) < setRotationStraightThreshold)
        {
            SetRotationStraight();
        }*/
    }

    void OnCollisionEnter(Collision coll)
    {
        return;
        if (cooldownOnDamageOwnerOnCrashCurrent > 0)
            return;
        
        if (coll.gameObject.layer != 6 && coll.gameObject.layer != 12 )
            return;

        /*
        var collisionForce = coll.impulse / Time.fixedDeltaTime;
        if (Vector3.Angle(collisionForce, rb.velocity) > contactAngleToDamageCrashThreshold)
            return;*/
        
        if (coll.relativeVelocity.magnitude > velocityToDamageCrashThreshold)
        {
            if (ownerHc == Player.Health)
            {
                cooldownOnDamageOwnerOnCrashCurrent = cooldownOnDamageOwnerOnCrash;
                ownerHc.Damage(50, DamageSource.Environment);
            }
        }
    }
}