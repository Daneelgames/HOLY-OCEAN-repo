using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using MrPink.Health;
using UnityEngine;

public class HumanVisualController : MonoBehaviour
{
    public Animator anim;

    [Header("Ragdoll")] 
    public Transform ragdollOrigin;
    Transform ragdollOriginParent;
    public List<Collider> colliders;
    public List<Rigidbody> rigidbodies;
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

    private HealthController hc;
    private bool ragdoll = false;
    public Material deadMaterial;
    public SkinnedMeshRenderer meshRenderer;
    private void Start()
    {
        hc = gameObject.GetComponent<HealthController>();
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
        /*
        if (Input.GetKeyDown("k"))
        {
            var hc = gameObject.GetComponent<HealthController>();
            hc.Damage(hc.health);
        }*/
        if (hc.health <= 0)
            return;
        
        if (ragdoll)
            return;
        
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

    [ContextMenu("GetCollidersFromRigidbodies")]
    public void GetCollidersFromRigidbodies()
    {
        for (int i = 0; i < rigidbodies.Count; i++)
        {
            colliders.Add(rigidbodies[i].gameObject.GetComponent<Collider>());
        }
    }

    public void Death()
    {
        meshRenderer.material = deadMaterial;
        if (!ragdoll)
            ActivateRagdoll();
    }

    private Coroutine followRagdollCoroutine;
    public void ActivateRagdoll()
    {
        if (ragdoll)
            return;
        
        anim.enabled = false;
        ragdoll = true;
        followRagdollCoroutine = StartCoroutine(FollowTheRagdoll());
        
        for (int i = 0; i < joints.Count; i++)
        {
            /*
            joints[i].angularXMotion = ConfigurableJointMotion.Free;
            joints[i].angularYMotion = ConfigurableJointMotion.Free;
            joints[i].angularZMotion = ConfigurableJointMotion.Free;*/
            
            var angularXDrive = joints[i].angularXDrive;
            angularXDrive.positionSpring = 0;
            angularXDrive.positionDamper = 0;
            joints[i].angularXDrive = angularXDrive;
            
            var angularYZDrive = joints[i].angularYZDrive;
            angularYZDrive.positionSpring = 0;
            angularYZDrive.positionDamper = 0;
            joints[i].angularYZDrive = angularYZDrive;
        }
        
        for (int i = 0; i < rigidbodies.Count; i++)
        {
            rigidbodies[i].drag = 0.5f;
            rigidbodies[i].angularDrag = 0.5f;
            rigidbodies[i].isKinematic = false;
            rigidbodies[i].useGravity = true;
            //rigidbodies[i].gameObject.layer = 6;
        }
        for (int i = 0; i < colliders.Count; i++)
        {
            colliders[i].material = UnitsManager.Instance.corpsesMaterial;
        }
    }

    void DeactivateRagdoll()
    {
        anim.enabled = true;
        ragdoll = false;
        
        for (int i = 0; i < joints.Count; i++)
        {
            var angularXDrive = joints[i].angularXDrive;
            var angularYZDrive = joints[i].angularYZDrive;
            if (i == 0)
            {
                joints[i].angularXMotion = ConfigurableJointMotion.Limited;
                joints[i].angularYMotion = ConfigurableJointMotion.Limited;
                joints[i].angularZMotion = ConfigurableJointMotion.Limited;
                
                angularXDrive.positionSpring = 1500;
                angularXDrive.positionDamper = 200;
                joints[i].angularXDrive = angularXDrive;
            
                angularYZDrive.positionSpring = 1500;
                angularYZDrive.positionDamper = 200;
                joints[i].angularYZDrive = angularYZDrive;
                continue;
            }
            
            /*
            joints[i].angularXMotion = ConfigurableJointMotion.Free;
            joints[i].angularYMotion = ConfigurableJointMotion.Free;
            joints[i].angularZMotion = ConfigurableJointMotion.Free;
            angularXDrive.positionSpring = 0;
            angularXDrive.positionDamper = 0;
            angularYZDrive.positionSpring = 0;
            angularYZDrive.positionDamper = 0;*/
            
            angularXDrive.positionSpring = 1500;
            angularXDrive.positionDamper = 200;
            joints[i].angularXDrive = angularXDrive;
            
            angularYZDrive.positionSpring = 1500;
            angularYZDrive.positionDamper = 200;
            joints[i].angularYZDrive = angularYZDrive;
        }
        
        for (int i = 0; i < rigidbodies.Count; i++)
        {
            rigidbodies[i].drag = 0f;
            rigidbodies[i].angularDrag = 0.05f;
            
            if (i == 0)
            {
                rigidbodies[i].isKinematic = true;
                rigidbodies[i].useGravity = false;
                continue;
            }
            rigidbodies[i].isKinematic = false;
            rigidbodies[i].useGravity = true;
            //rigidbodies[i].gameObject.layer = 6;
        }
        for (int i = 0; i < colliders.Count; i++)
        {
            colliders[i].material = null;
        }
    }

    IEnumerator FollowTheRagdoll()
    {
        ragdollOriginParent = ragdollOrigin.parent;
        ragdollOrigin.parent = null;
        while (true)
        {
            yield return new WaitForSeconds(3f);
            transform.position = ragdollOrigin.transform.position;
            
            if (Physics.CheckSphere(transform.position, 0.5f, 1<<6, QueryTriggerInteraction.Ignore))
            {
                break;
            }
        }
        ragdollOrigin.parent = ragdollOriginParent;
        DeactivateRagdoll();
        hc.AiMovement.Resurrect();

    }

    public void ExplosionRagdoll(Vector3 pos, float force, float distance)
    {
        for (int i = 0; i < rigidbodies.Count; i++)
        {
            rigidbodies[i].AddExplosionForce(force, pos, distance);
        }
    }
}
