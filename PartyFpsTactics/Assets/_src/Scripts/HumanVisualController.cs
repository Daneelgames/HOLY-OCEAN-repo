using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using MrPink.Health;
using MrPink.PlayerSystem;
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
    public float timeToStandUp = 2;

    private static readonly int InCover = Animator.StringToHash("InCover");

    private HealthController hc;
    private bool ragdoll = false;
    private bool visibleToPlayer = false;
    public Material deadMaterial;
    public SkinnedMeshRenderer meshRenderer;
    public LayerMask tilesLayerMask;
    private float lerpToStand = 1;
    private void Start()
    {
        hc = gameObject.GetComponent<HealthController>();
        for (int i = 0; i < joints.Count; i++)
        {
            initRotations.Add(animatedBones[i].localRotation);
        }

        StartCoroutine(GetDistanceToPlayer());
    }

    IEnumerator GetDistanceToPlayer()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (Player._instance == null)
            {
                visibleToPlayer = true;
                continue;
            }
            
            if (Vector3.Distance(transform.position, Player.MainCamera.transform.position) >= 50)
            {
                if (visibleToPlayer)
                {
                    for (int i = 0; i < joints.Count; i++)
                    {
                        joints[i].gameObject.SetActive(false);
                    }
                }
                visibleToPlayer = false;
            }
            else
            {
                if (!visibleToPlayer)
                {
                    for (int i = 0; i < joints.Count; i++)
                    {
                        joints[i].gameObject.SetActive(true);
                    }
                }
                
                visibleToPlayer = true;
            }
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
        if (hc.health <= 0)
            return;
        
        if (ragdoll)
            return;
        
        if (!visibleToPlayer)
            return;
        
        for (int i = 0; i < joints.Count; i++)
        {
            joints[i].targetRotation = CopyRotation(i);
            joints[i].transform.position = Vector3.Lerp(joints[i].transform.position, animatedBones[i].position, lerpToStand);
        }
    }

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
        {
            if (followRagdollCoroutine != null)
                StopCoroutine(followRagdollCoroutine);
            //return;
        }
        
        if (ChangeLerpToStandCoroutine != null)
        {
            StopCoroutine(ChangeLerpToStandCoroutine);
        }
        
        anim.enabled = false;
        ragdoll = true;
        followRagdollCoroutine = StartCoroutine(FollowTheRagdoll());
        
        for (int i = 0; i < joints.Count; i++)
        {
            var angularXDrive = joints[i].angularXDrive;
            angularXDrive.positionSpring = 0;
            angularXDrive.positionDamper = 0;
            joints[i].angularXDrive = angularXDrive;
            
            var angularYZDrive = joints[i].angularYZDrive;
            angularYZDrive.positionSpring = 0;
            angularYZDrive.positionDamper = 0;
            joints[i].angularYZDrive = angularYZDrive;
            
            var xDrive = joints[i].xDrive;
            xDrive.positionSpring = 0;
            xDrive.positionDamper = 0;
            joints[i].xDrive = xDrive;

            joints[i].xDrive = xDrive;
            var yDrive = joints[i].yDrive;
            xDrive.positionSpring = 0;
            xDrive.positionDamper = 0;
            joints[i].yDrive = yDrive;

            joints[i].xDrive = xDrive;
            var zDrive = joints[i].zDrive;
            xDrive.positionSpring = 0;
            xDrive.positionDamper = 0;
            joints[i].zDrive = zDrive;
        }
        
        for (int i = 0; i < rigidbodies.Count; i++)
        {
            rigidbodies[i].drag = 0.5f;
            rigidbodies[i].angularDrag = 0.5f;
            rigidbodies[i].isKinematic = false;
            rigidbodies[i].useGravity = true;
        }
        
        for (int i = 0; i < colliders.Count; i++)
        {
            colliders[i].material = UnitsManager.Instance.corpsesMaterial;
        }
    }

    void DeactivateRagdoll()
    {
        ChangeLerpToStandCoroutine = StartCoroutine(ChangeLerpToStand());
        
        anim.enabled = true;
        ragdoll = false;
        
        for (int i = 0; i < joints.Count; i++)
        {
            var angularXDrive = joints[i].angularXDrive;
            var angularYZDrive = joints[i].angularYZDrive;
            if (i == 0)
            {
                var xDrive = joints[i].xDrive;
                xDrive.positionSpring = 900;
                xDrive.positionDamper = 100;
                joints[i].xDrive = xDrive;

                joints[i].xDrive = xDrive;
                var yDrive = joints[i].yDrive;
                xDrive.positionSpring = 900;
                xDrive.positionDamper = 100;
                joints[i].yDrive = yDrive;

                joints[i].xDrive = xDrive;
                var zDrive = joints[i].zDrive;
                xDrive.positionSpring = 900;
                xDrive.positionDamper = 100;
                joints[i].zDrive = zDrive;
                
                joints[i].angularXMotion = ConfigurableJointMotion.Free;
                joints[i].angularYMotion = ConfigurableJointMotion.Free;
                joints[i].angularZMotion = ConfigurableJointMotion.Free;
                
                angularXDrive.positionSpring = 900;
                angularXDrive.positionDamper = 0;
                angularYZDrive.positionSpring = 900;
                angularYZDrive.positionDamper = 0; 
                
                joints[i].angularXDrive = angularXDrive;
                joints[i].angularYZDrive = angularYZDrive;
                continue;
            }
            
            angularXDrive.positionSpring = 900;
            angularYZDrive.positionSpring = 900;
            angularXDrive.positionDamper = 0;
            angularYZDrive.positionDamper = 0;
            
            joints[i].angularXDrive = angularXDrive;
            joints[i].angularYZDrive = angularYZDrive;
        }
        
        for (int i = 0; i < rigidbodies.Count; i++)
        {
            rigidbodies[i].drag = 0f;
            rigidbodies[i].angularDrag = 0.05f;
            
            if (i == 0)
            {
                rigidbodies[i].isKinematic = false;
                rigidbodies[i].useGravity = false;
                continue;
            }
            rigidbodies[i].isKinematic = false;
            rigidbodies[i].useGravity = false;
        }
        for (int i = 0; i < colliders.Count; i++)
        {
            colliders[i].material = null;
        }
    }

    private Coroutine ChangeLerpToStandCoroutine;
    IEnumerator ChangeLerpToStand()
    {
        float t = 0;
        lerpToStand = 0;

        while (t < timeToStandUp)
        {
            t += Time.deltaTime;
            lerpToStand = t;
            yield return null;
        }

        lerpToStand = 1;
    }
    IEnumerator FollowTheRagdoll()
    {
        ragdollOriginParent = ragdollOrigin.parent;
        ragdollOrigin.parent = null;
        float standupCooldown = hc.UnitRagdollStandupCooldown;
        float t = 0;
        while (true)
        {
            yield return null;
            transform.position = ragdollOrigin.position;
            t += Time.deltaTime;
            
            if (t < standupCooldown)
                continue;
            
            if (hc.health <= 0)
            {
                ragdollOrigin.parent = ragdollOriginParent;
                yield break;
            }
            
            t = 0;
            
            if (Physics.CheckSphere(ragdollOrigin.position, 1f, tilesLayerMask, QueryTriggerInteraction.Ignore))
            {
                break;
            }
        }
        ragdollOrigin.parent = ragdollOriginParent;
        DeactivateRagdoll();
        hc.RestoreEndurance();
        hc.AiMovement.Resurrect();

    }

    public void ExplosionRagdoll(Vector3 pos, float force, float distance)
    {
        Debug.Log("ExplosionRagdoll");
        for (int i = 0; i < rigidbodies.Count; i++)
        {
            rigidbodies[i].AddExplosionForce(force, pos, distance);
        }
    }
}
