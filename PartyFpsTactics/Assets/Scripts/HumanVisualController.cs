using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class HumanVisualController : MonoBehaviour
{
    public Animator anim;

    [Header("Ragdoll")] 
    public List<Transform> animatedBones;
    public List<ConfigurableJoint> joints;
    List<Quaternion> initRotations = new List<Quaternion>();
    private void Start()
    {
        for (int i = 0; i < joints.Count; i++)
        {
            initRotations.Add(animatedBones[i].localRotation);
        }
    }

    public void SetMovementVelocity(Vector3 velocity)
    {
        float velocityX = Vector3.Dot(velocity.normalized, transform.right);
        float velocityZ = Vector3.Dot(velocity.normalized, transform.forward);
        
        anim.SetFloat("VelocityX", velocityX, 0.1f, Time.deltaTime);
        anim.SetFloat("VelocityZ", velocityZ, 0.1f, Time.deltaTime);
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < joints.Count; i++)
        {
            joints[i].targetRotation = CopyRotation(i);
        }
    }

    Quaternion CopyRotation(int index)
    {
        return Quaternion.Inverse(animatedBones[index].localRotation) * initRotations[index];
    }
}
