using System;
using System.Collections;
using System.Collections.Generic;
using MrPink;
using MrPink.Health;
using MrPink.PlayerSystem;
using MrPink.WeaponsSystem;
using NWH.DWP2.ShipController;
using Sirenix.OdinInspector;
using UnityEngine;
using VehicleBehaviour;

public class ControlledMachine : MonoBehaviour
{
    [BoxGroup]
    public AdvancedShipController AdvancedShipController;
    [BoxGroup]
    public AiWaterObject AiWaterObject;
    [BoxGroup]
    public WheelVehicle wheelVehicle;
    [BoxGroup]
    public SleepMachine sleepMachine;
    
    [SerializeField]private float dashForce = 1000;
    
    public Transform Visual;
    public float visualFollowSpeed = 10;
    public Transform sitTransform;
    public Transform CameraTransform;
    public Transform sitTransformNpc;
    public float DamageToControllingHcScaler = 0.33f;
    public List<Collider> collidersEnabledWhenPlayerInside;
    public List<MeleeCollider> carCrashDamageColliders;
    List<Transform> carCrashCollidersParents = new List<Transform>();
    public Rigidbody rb;
    private float rbDrag = 1;
    private float rbAngularDrag = 1;
    public HealthController controllingHc;
    
    private void Awake()
    {
        rbDrag = rb.drag;
        rbAngularDrag = rb.angularDrag;
        foreach (var col in carCrashDamageColliders)
        {
            GameObject carCrashColliderParent = new GameObject("carCrashColliderParent");
            carCrashColliderParent.transform.position = col.transform.position;
            carCrashColliderParent.transform.rotation = col.transform.rotation;
            carCrashColliderParent.transform.parent = col.transform.parent;
            carCrashCollidersParents.Add(carCrashColliderParent.transform);
            col.transform.parent = null;
        }
    }

    public void DashForward()
    {
        rb.AddForce(transform.forward * dashForce, ForceMode.Impulse);
    }
    public void StartInput(HealthController driverHc)
    {
        controllingHc = driverHc;
        if (AdvancedShipController && driverHc.IsPlayer)
        {
            AdvancedShipController.Wake();
        }
        if (AiWaterObject)
        {
            AiWaterObject.StartInput(driverHc);
        }
        if (wheelVehicle)
        {
            wheelVehicle.IsPlayer = driverHc.IsPlayer;
            wheelVehicle.Handbrake = false;
        }

        if (sleepMachine)
        {
            if (driverHc.IsPlayer)
                sleepMachine.PlayerInside(true);
        }

        if (rotateVehicleStraight != null)
            StopCoroutine(rotateVehicleStraight);
        
        if (driverHc.IsPlayer)
        {
            if (Vector3.Angle(transform.up, Vector3.down) < 120)
                rotateVehicleStraight = StartCoroutine(RotateVehicleStraight());

            for (int i = 0; i < collidersEnabledWhenPlayerInside.Count; i++)
            {
                collidersEnabledWhenPlayerInside[i].gameObject.SetActive(true);
            }
        }

        if (visualFollowCoroutine != null)
            StopCoroutine(visualFollowCoroutine);
        
        visualFollowCoroutine = StartCoroutine(VisualFollow());
    }

    public void AddForceOnImpact(Vector3 impactOrigin)
    {
        rb.AddForce((transform.position - impactOrigin).normalized * 300, ForceMode.Impulse);
    }
    public void AddRampForce(float amount, Vector3 dir)
    {
        rb.AddForce(dir * amount, ForceMode.Impulse);
    }

    private Coroutine rotateVehicleStraight;
    IEnumerator RotateVehicleStraight()
    {
        float t = 0;
        float tt = 1;
        Quaternion rot = transform.rotation;
        while (t < tt)
        {
            t += Time.deltaTime;
            rot.eulerAngles = Vector3.Slerp(rot.eulerAngles, new Vector3(rot.eulerAngles.x, rot.eulerAngles.y, 0), t/tt);
            transform.rotation = rot;
            yield return null;
        }
    }
    
    private Coroutine visualFollowCoroutine;
    IEnumerator VisualFollow()
    {
        if (Visual == null)
            yield break;
        
        Visual.transform.parent = null;
        for (var index = 0; index < carCrashDamageColliders.Count; index++)
        {
            var col = carCrashDamageColliders[index];
            col.transform.parent = null;
        }

        while (true)
        {
            Visual.transform.position = Vector3.Lerp(Visual.transform.position, transform.position, visualFollowSpeed * Time.fixedUnscaledDeltaTime);
            Visual.transform.rotation = Quaternion.Lerp(Visual.transform.rotation, transform.rotation, visualFollowSpeed * Time.fixedUnscaledDeltaTime);

            for (var index = 0; index < carCrashDamageColliders.Count; index++)
            {
                var col = carCrashDamageColliders[index];
                if (carCrashCollidersParents.Count <= index)
                    break;
                
                col.FollowDetachedTransform(carCrashCollidersParents[index]);
            }

            yield return null;
        }
    }

    public void StopMachine()
    {
        if (Visual)
        {
            if (visualFollowCoroutine != null)
                StopCoroutine(visualFollowCoroutine);

            Visual.transform.parent = transform;
            visualFollowCoroutine = StartCoroutine(ResetVisualTransform());
        }
        if (AdvancedShipController)
        {
            AdvancedShipController.Sleep();
        }

        if (AiWaterObject)
            AiWaterObject.StopInput();
        if (wheelVehicle)
        {
            wheelVehicle.IsPlayer = false;
            wheelVehicle.Handbrake = true;
        }
        
        if (sleepMachine)
            sleepMachine.PlayerInside(false);

        for (int i = 0; i < collidersEnabledWhenPlayerInside.Count; i++)
        {
            collidersEnabledWhenPlayerInside[i].gameObject.SetActive(false);    
        }

        for (var index = 0; index < carCrashDamageColliders.Count; index++)
        {
            var col = carCrashDamageColliders[index];
            col.transform.position = carCrashCollidersParents[index].position;
            col.transform.rotation = carCrashCollidersParents[index].rotation;
            col.transform.parent = carCrashCollidersParents[index];
        }
        
        rb.drag = rbDrag;
        rb.angularDrag = rbAngularDrag;
        controllingHc = null;
    }

    IEnumerator ResetVisualTransform()
    {
        float t = 0;
        float tt = 1;
        while (t < tt)
        {
            t += Time.deltaTime;
            Visual.localPosition = Vector3.Lerp(Visual.localPosition, Vector3.zero, t/tt); 
            Visual.localRotation = Quaternion.Lerp(Visual.localRotation, Quaternion.identity, t/tt); 
            yield return null;
        }
    }

    public void SetCarInputAi()
    {
        Debug.Log("Debug Ai Water bike 1");
        if (AiWaterObject)
            AiWaterObject.SetInputAi();
    }
    public void SetCarInputPlayer(float hor, float ver, bool brake, bool boost = false)
    {
        // перекинуть в общий прием инпута от юнитов
        if (wheelVehicle)
            wheelVehicle.SetInput(hor, ver, brake, boost);
        
        if (AiWaterObject) // controlled by player
            AiWaterObject.SetInput(hor, ver, brake, boost);
    }

    private void OnDestroy()
    {
        if (Visual) Destroy(Visual.gameObject);
    }
}