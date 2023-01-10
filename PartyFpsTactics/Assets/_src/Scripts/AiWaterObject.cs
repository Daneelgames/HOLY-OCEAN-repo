using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityAnimator;
using MrPink;
using Sirenix.OdinInspector;
using UnityEngine;

public class AiWaterObject : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float thrustPower = 30;
    [SerializeField] private float rotateSmooth = 1;

    [SerializeField] [ReadOnly] private float resultThrust;
    [SerializeField] [ReadOnly] private Quaternion targetRotation;
    
    private void Update()
    {
        if (Game._instance == null || Game.LocalPlayer == null)
            return;

        resultThrust = thrustPower;

        targetRotation = Quaternion.Slerp(rb.transform.rotation,
            Quaternion.LookRotation(Game.LocalPlayer.transform.position - transform.position),
            rotateSmooth * Time.unscaledDeltaTime);
        rb.MoveRotation(targetRotation);
    }

    void FixedUpdate()
    {
        if (Game._instance == null || Game.LocalPlayer == null)
            return;
        
        ApplyMotion();
    }

    void ApplyMotion()
    {
        rb.AddRelativeForce(Vector3.forward * resultThrust);
    }
}
