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

    [Header("IK")] 
    public Transform ikAimBone;
    public Transform ikTarget;
    public Transform ikAimTransform;
    public int aimIkIterations = 10;
    public Vector3 rotationOffset;
    private static readonly int InCover = Animator.StringToHash("InCover");

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
            //joints[i].targetPosition = animatedBones[i].position;
            joints[i].transform.position = animatedBones[i].position;
        }
    }

    /*
    private void LateUpdate()
    {
        for (int i = 0; i < aimIkIterations; i++)
        {
            AimAtTarget(ikAimBone, ikTarget.position);
        }
    }

    void AimAtTarget(Transform bone, Vector3 pos)
    {
        Vector3 aimDirection = ikAimTransform.forward;
        Vector3 targetDirection = pos - ikAimTransform.position;
        Quaternion aimTowards = Quaternion.FromToRotation(aimDirection, targetDirection);
        bone.rotation = aimTowards * bone.rotation;
    }
    */

    Quaternion CopyRotation(int index)
    {
        return Quaternion.Inverse(animatedBones[index].localRotation) * initRotations[index];
    }

    public void SetInCover(bool inCover)
    {
        anim.SetBool(InCover, inCover);
    }
}
