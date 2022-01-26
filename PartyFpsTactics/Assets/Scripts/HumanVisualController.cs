using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanVisualController : MonoBehaviour
{
    public Animator anim;
    public List<Transform> animatedTransforms;
    public List<ConfigurableJoint> joints;
    public Transform pelvisBone;
    private List<Quaternion> jointsLocalRotations;

    public void SetMovementVelocity(Vector3 velocity)
    {
        float velocityX = Vector3.Dot(velocity.normalized, transform.right);
        float velocityZ = Vector3.Dot(velocity.normalized, transform.forward);
        
        anim.SetFloat("VelocityX", velocityX, 0.1f, Time.deltaTime);
        anim.SetFloat("VelocityZ", velocityZ, 0.1f, Time.deltaTime);
    }

    private void Start()
    {
        for (int i = 0; i < joints.Count; i++)
        {
            jointsLocalRotations.Add(joints[i].transform.localRotation);
        }
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < joints.Count; i++)
        {
            ConfigurableJointExtensions.SetTargetRotationLocal(joints[i], animatedTransforms[i + 1].localRotation, jointsLocalRotations[i]);
        }

        pelvisBone.transform.position = animatedTransforms[0].transform.position;
        pelvisBone.transform.rotation = animatedTransforms[0].transform.rotation;
    }
}
