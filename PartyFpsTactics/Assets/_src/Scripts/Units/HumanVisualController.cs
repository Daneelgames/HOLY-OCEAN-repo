using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using MrPink.PlayerSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink.Units
{
    public class HumanVisualController : MonoBehaviour
    {
        public Animator anim;
        public HealthController hc;

        public bool noRagdoll = false;
        [Header("Ragdoll")] 
        public Transform ragdollOrigin;
        Transform ragdollOriginParent;
        public List<Collider> colliders;
        public List<Rigidbody> rigidbodies;
        public List<Transform> animatedBones;
        public List<ConfigurableJoint> joints;
        public List<MeshRenderer> bodyPartsVisuals;

        [Space]
        public List<Transform> allBones;

        public float rbNormalDrag = 1;
        public float rbNormalAngularDrag = 1;
        public float rbRagdollDrag = 1;
        public float rbRagdollAngularDrag = 1;
        public float activePositionSpring = 1500;
        public float activePositionDamper = 100;
        public float activeAngularPositionSpring = 1500;
        public float activeAngularPositionDamper = 100;
    
        List<Quaternion> initRotations = new List<Quaternion>();
        public float timeToStandUp = 2;

        private static readonly int InCover = Animator.StringToHash("InCover");
    
        private bool ragdoll = false;
        private bool inVehicle = false;
        public Material aliveMaterial;
        public Material deadMaterial;
        public SkinnedMeshRenderer meshRenderer;
        public LayerMask tilesLayerMask;
        private float lerpToStand = 1;
    
        private Coroutine _changeLerpToStandCoroutine;
        private Coroutine _followRagdollCoroutine;
        private static readonly int Passenger = Animator.StringToHash("Passenger");
        [SerializeField] private List<Collider> extraCollidersToIgnore;

        private void Start()
        {
            if (hc == null)
                hc = gameObject.GetComponent<HealthController>();
            
            if (noRagdoll)
                return;
            
            ragdollOriginParent = ragdollOrigin.parent;
            for (int i = 0; i < joints.Count; i++)
                initRotations.Add(animatedBones[i].localRotation);

            for (int i = 0; i < rigidbodies.Count; i++)
            {
                rigidbodies[i].drag = rbNormalDrag;
                rigidbodies[i].angularDrag = rbNormalAngularDrag;
            }

            IgnoreOwnCollision();

            if (hc.IsPlayer == false)
                StartCoroutine(GetGroundedAi());
        }

        [SerializeField] [ReadOnly] private bool grounded = true;
        [SerializeField]  private float groundedCheckSphereRadius = 2.5f;
        IEnumerator GetGroundedAi()
        {
            while (hc.IsDead == false)
            {
                yield return new WaitForSeconds(3f);

                grounded = Physics.CheckSphere(ragdollOrigin.position, groundedCheckSphereRadius, GameManager.Instance.AllSolidsMask, QueryTriggerInteraction.Ignore);
                if (!grounded && ragdoll == false)
                {
                    if (hc.AiMovement)
                        hc.AiMovement.StopActivities();
                    
                    ActivateRagdoll();
                }
            }
        }
        
        void IgnoreOwnCollision()
        {
            foreach (var collider1 in colliders)
            {
                foreach (var collider2 in colliders)
                {
                    if (collider1 == collider2)
                        continue;
                    
                    Physics.IgnoreCollision(collider1, collider2);
                }

                foreach (var collider2 in extraCollidersToIgnore)
                {
                    Physics.IgnoreCollision(collider1, collider2);
                }
            }
        }

        private void OnEnable()
        {
            if (getMovementAnimCoroutine != null)
                StopCoroutine(getMovementAnimCoroutine);
            
            DeactivateRagdoll();
            
            getMovementAnimCoroutine = StartCoroutine(GetMovementAnim());
        }

        private Coroutine getMovementAnimCoroutine;

        private void OnDestroy()
        {
            Destroy(ragdollOrigin.gameObject);
            Destroy(anim.gameObject);
        }

        [ContextMenu("SetAliveMaterial")]
        public void SetAliveMaterial()
        {
            for (int i = 0; i < bodyPartsVisuals.Count; i++)
            {
                bodyPartsVisuals[i].material = aliveMaterial;
            }
        }

        private Vector3 lastPos;
        private IEnumerator GetMovementAnim()
        {
            while (hc == null)
            {
                yield return null;
            }
            Vector3 vector = Vector3.zero;
            float velocityX;
            float velocityZ;
            lastPos = ragdollOrigin.position;
            while (hc.IsDead == false)
            {
                yield return new WaitForSeconds(0.5f);
                var curPos = ragdollOrigin.position;
                vector = (curPos - lastPos).normalized;
                velocityX = Vector3.Dot(vector, transform.right);
                velocityZ = Vector3.Dot(vector, transform.forward);
                
                anim.SetFloat("VelocityX", velocityX, 0.1f, 0.1f);
                anim.SetFloat("VelocityZ", velocityZ, 0.1f, 0.1f);
                lastPos = ragdollOrigin.position;
            }
        }

        public void SetMovementVelocity(Vector3 velocity)
        {
            if(velocity.magnitude < 0.2f)
                velocity = Vector3.zero;
            
            float velocityX = Vector3.Dot(velocity.normalized, transform.right);
            float velocityZ = Vector3.Dot(velocity.normalized, transform.forward);
        
            anim.SetFloat("VelocityX", velocityX, 0.1f, Time.deltaTime);
            anim.SetFloat("VelocityZ", velocityZ, 0.1f, Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (noRagdoll)
                return;
            
            if (hc.health <= 0)
                return;
        
            if (ragdoll)
                return;
        
            for (int i = 0; i < joints.Count; i++)
            {
                if (!inVehicle && rigidbodies[i].isKinematic == false)
                    joints[i].targetRotation = CopyRotation(i);
                else
                    joints[i].transform.rotation = animatedBones[i].rotation;
            
                joints[i].transform.position = Vector3.Lerp(joints[i].transform.position, animatedBones[i].position, lerpToStand);
            }
        }

        private Quaternion CopyRotation(int index)
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
            foreach (var rb in rigidbodies)
                colliders.Add(rb.gameObject.GetComponent<Collider>());
        }

        public void SetCollidersTriggers(bool trigger)
        {
            foreach (var coll in colliders)
            {
                coll.isTrigger = trigger;
            }
        }
    
        public void SetVehicleAiPassenger(ControlledMachine machine)
        {
            if (machine && machine.sitTransformNpc == null)
                return;
            
            inVehicle = machine;
            anim.enabled = true;
            anim.SetBool(Passenger, inVehicle);
        
            foreach (var rb in rigidbodies)
            {
                rb.isKinematic = inVehicle;
                rb.useGravity = false;
            }
        }
    
        public void Death()
        {
            if (noRagdoll)
                return;
            
            meshRenderer.material = deadMaterial;
            if (!ragdoll)
                ActivateRagdoll();
        }
    
        public void ActivateRagdoll()
        {
            if (noRagdoll)
                return;

            if (hc.aiVehicleControls)
                hc.aiVehicleControls.SetPassengerSit(null,false);
            if (ragdoll && _followRagdollCoroutine != null)
                StopCoroutine(_followRagdollCoroutine);
        
            if (_changeLerpToStandCoroutine != null)
                StopCoroutine(_changeLerpToStandCoroutine);

            anim.enabled = false;
            ragdoll = true;
            _followRagdollCoroutine = StartCoroutine(FollowTheRagdoll());
        
            foreach (var joint in joints)
            {
                var angularXDrive = joint.angularXDrive;
                angularXDrive.positionSpring = 0;
                angularXDrive.positionDamper = 0;
                joint.angularXDrive = angularXDrive;
            
                var angularYZDrive = joint.angularYZDrive;
                angularYZDrive.positionSpring = 0;
                angularYZDrive.positionDamper = 0;
                joint.angularYZDrive = angularYZDrive;
            
                var xDrive = joint.xDrive;
                xDrive.positionSpring = 0;
                xDrive.positionDamper = 0;
                joint.xDrive = xDrive;

                joint.xDrive = xDrive;
                var yDrive = joint.yDrive;
                xDrive.positionSpring = 0;
                xDrive.positionDamper = 0;
                joint.yDrive = yDrive;

                joint.xDrive = xDrive;
                var zDrive = joint.zDrive;
                xDrive.positionSpring = 0;
                xDrive.positionDamper = 0;
                joint.zDrive = zDrive;
            }
        
            foreach (var rb in rigidbodies)
            {
                rb.drag = rbRagdollDrag;
                rb.angularDrag = rbRagdollAngularDrag;
                /*
                rb.drag = 0.5f;
                rb.angularDrag = 0.5f;
                */
                rb.isKinematic = false;
                rb.useGravity = true;
            }
        
            foreach (var col in colliders)
            {
                col.material = UnitsManager.Instance.corpsesMaterial;
                col.isTrigger = false;
            }
        }

        void DeactivateRagdoll()
        {
            if (noRagdoll)
                return;

            _changeLerpToStandCoroutine = StartCoroutine(ChangeLerpToStand());
        
            anim.enabled = true;
            ragdoll = false;
        
            for (int i = 0; i < joints.Count; i++)
            {
                var angularXDrive = joints[i].angularXDrive;
                var angularYZDrive = joints[i].angularYZDrive;
                if (i == 0)
                {
                    var xDrive = joints[i].xDrive;
                    xDrive.positionSpring = activePositionSpring;
                    xDrive.positionDamper = activePositionDamper;
                    joints[i].xDrive = xDrive;

                    joints[i].xDrive = xDrive;
                    var yDrive = joints[i].yDrive;
                    xDrive.positionSpring = activePositionSpring;
                    xDrive.positionDamper = activePositionDamper;
                    joints[i].yDrive = yDrive;

                    joints[i].xDrive = xDrive;
                    var zDrive = joints[i].zDrive;
                    xDrive.positionSpring = activePositionSpring;
                    xDrive.positionDamper = activePositionDamper;
                    joints[i].zDrive = zDrive;
                
                    joints[i].angularXMotion = ConfigurableJointMotion.Free;
                    joints[i].angularYMotion = ConfigurableJointMotion.Free;
                    joints[i].angularZMotion = ConfigurableJointMotion.Free;
                
                    angularXDrive.positionSpring = activeAngularPositionSpring;
                    angularXDrive.positionDamper = activeAngularPositionDamper;
                    angularYZDrive.positionSpring = activeAngularPositionSpring;
                    angularYZDrive.positionDamper = activeAngularPositionDamper; 
                
                    joints[i].angularXDrive = angularXDrive;
                    joints[i].angularYZDrive = angularYZDrive;
                    continue;
                }
            
                angularXDrive.positionSpring = activeAngularPositionSpring;
                angularYZDrive.positionSpring = activeAngularPositionSpring;
                angularXDrive.positionDamper = activeAngularPositionDamper;
                angularYZDrive.positionDamper = activeAngularPositionDamper;
            
                joints[i].angularXDrive = angularXDrive;
                joints[i].angularYZDrive = angularYZDrive;
            }
        
            for (int i = 0; i < rigidbodies.Count; i++)
            {
                rigidbodies[i].drag = rbNormalDrag;
                rigidbodies[i].angularDrag = rbNormalAngularDrag;
            
                if (i == 0)
                {
                    rigidbodies[i].isKinematic = true;
                    //rigidbodies[i].isKinematic = false;
                    rigidbodies[i].useGravity = false;
                    continue;
                }
                rigidbodies[i].isKinematic = true;
                //rigidbodies[i].isKinematic = false;
                rigidbodies[i].useGravity = false;
                rigidbodies[i].velocity = Vector3.zero;
                rigidbodies[i].angularVelocity = Vector3.zero;
            }
        
            foreach (var col in colliders)
                col.material = null;
        }
    
        private IEnumerator ChangeLerpToStand()
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

        [SerializeField] private float moveRbToOriginIfPlayerForce = 5;
        private IEnumerator FollowTheRagdoll()
        {
            if (hc.IsPlayer == false) // only for mobs
                ragdollOrigin.parent = null;
            float standupCooldown = hc.UnitRagdollStandupCooldown;
            float t = 0;
            Vector3 prevPos = rigidbodies[0].transform.position;
            var pelvisRb = rigidbodies[0];
            while (true)
            {
                yield return null;
                if (hc.IsPlayer)
                {
                    if( Vector3.Distance(pelvisRb.transform.position, transform.position) > 2)
                        pelvisRb.AddForce((transform.position - pelvisRb.transform.position).normalized * moveRbToOriginIfPlayerForce, ForceMode.Acceleration);
                    continue;
                }
                
                transform.position = ragdollOrigin.position;
                t += Time.deltaTime;

                if (Physics.Linecast(prevPos, transform.position, out var hit,
                    GameManager.Instance.AllSolidsMask, QueryTriggerInteraction.Ignore))
                {
                    transform.position = hit.point;
                }
                
                prevPos = transform.position;
                
                if (t < standupCooldown)
                    continue;
            
                if (hc.health <= 0)
                {
                    ragdollOrigin.parent = ragdollOriginParent;
                    yield break;
                }
            
                t = 0;
            
                if (grounded)
                    break;
            }
            
            Resurrect();

        }

        public void Resurrect()
        {
            if (hc.health <= 0)
                return;
            
            if (Physics.Linecast(transform.position, transform.position + Vector3.up, out var hit,
                GameManager.Instance.AllSolidsMask, QueryTriggerInteraction.Ignore))
            {
                transform.position = hit.point + Vector3.up * 0.1f;
            }
            ragdollOrigin.parent = ragdollOriginParent;
            DeactivateRagdoll();
            
            hc.RestoreEndurance();
            // AI
            if (hc.AiMovement)
            {
                hc.AiMovement.RestartActivities();
                hc.selfUnit.UnitMovement.Resurrect();
            }
        }

        public void ExplosionRagdoll(Vector3 pos, float force, float distance)
        {
            if (noRagdoll)
                return;

            Debug.Log("ExplosionRagdoll");
            foreach (var rb in rigidbodies)
                rb.AddExplosionForce(force, pos, distance);
        }
    }
}