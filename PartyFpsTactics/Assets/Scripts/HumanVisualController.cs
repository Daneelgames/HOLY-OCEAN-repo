using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanVisualController : MonoBehaviour
{
    public Animator anim;
    public void SetMovementVelocity(Vector3 velocity)
    {
        float velocityX = Vector3.Dot(velocity.normalized, transform.right);
        float velocityZ = Vector3.Dot(velocity.normalized, transform.forward);
        
        anim.SetFloat("VelocityX", velocityX, 0.1f, Time.deltaTime);
        anim.SetFloat("VelocityZ", velocityZ, 0.1f, Time.deltaTime);
    }
}
