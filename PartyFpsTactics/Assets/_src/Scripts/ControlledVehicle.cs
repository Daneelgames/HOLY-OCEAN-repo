using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using MrPink.PlayerSystem;
using MrPink.WeaponsSystem;
using UnityEngine;
using VehicleBehaviour;

public class ControlledVehicle : MonoBehaviour
{
    public WheelVehicle wheelVehicle;
    public Transform vehicleVisual;
    public float visualFollowSpeed = 10;
    public Transform sitTransform;
    public Transform sitTransformNpc;
    public List<Collider> collidersToDisableWhenNotDriving;
    public List<Collider> carCrashDamageColliders;
    List<Transform> carCrashCollidersParents = new List<Transform>();
    public Rigidbody rb;
    private float rbDrag = 1;
    private float rbAngularDrag = 1;
    
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

    public void StartInput()
    {
        wheelVehicle.IsPlayer = true;
        wheelVehicle.Handbrake = false;

        rb.drag = rbDrag;
        rb.angularDrag = rbAngularDrag;
        if (rotateVehicleStraight != null)
            StopCoroutine(rotateVehicleStraight);
        
        if (Vector3.Angle(transform.up, Vector3.down) < 120)
            rotateVehicleStraight = StartCoroutine(RotateVehicleStraight());
        
        for (int i = 0; i < collidersToDisableWhenNotDriving.Count; i++)
        {
            collidersToDisableWhenNotDriving[i].gameObject.SetActive(true);    
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
        vehicleVisual.transform.parent = null;
        for (var index = 0; index < carCrashDamageColliders.Count; index++)
        {
            var col = carCrashDamageColliders[index];
            col.transform.parent = null;
        }

        while (true)
        {
            vehicleVisual.transform.position = Vector3.Lerp(vehicleVisual.transform.position, transform.position, visualFollowSpeed * Time.deltaTime);
            vehicleVisual.transform.rotation = Quaternion.Lerp(vehicleVisual.transform.rotation, transform.rotation, visualFollowSpeed * Time.deltaTime);

            for (var index = 0; index < carCrashDamageColliders.Count; index++)
            {
                var col = carCrashDamageColliders[index];
                if (carCrashCollidersParents.Count <= index)
                    break;
                
                col.transform.position = carCrashCollidersParents[index].position;
                col.transform.rotation = carCrashCollidersParents[index].rotation;
            }

            yield return null;
        }
    }

    public void StopMovement()
    {
        if (visualFollowCoroutine != null)
            StopCoroutine(visualFollowCoroutine);
        
        vehicleVisual.transform.parent = transform;
        wheelVehicle.IsPlayer = false;
        wheelVehicle.Handbrake = true;

        for (int i = 0; i < collidersToDisableWhenNotDriving.Count; i++)
        {
            collidersToDisableWhenNotDriving[i].gameObject.SetActive(false);    
        }

        for (var index = 0; index < carCrashDamageColliders.Count; index++)
        {
            var col = carCrashDamageColliders[index];
            col.transform.position = carCrashCollidersParents[index].position;
            col.transform.rotation = carCrashCollidersParents[index].rotation;
            col.transform.parent = carCrashCollidersParents[index];
        }
        visualFollowCoroutine = StartCoroutine(ResetVisualTransform());
        
        rb.drag = 0.5f;
        rb.angularDrag = 0.5f;
    }

    IEnumerator ResetVisualTransform()
    {
        float t = 0;
        float tt = 1;
        while (t < tt)
        {
            t += Time.deltaTime;
            vehicleVisual.localPosition = Vector3.Lerp(vehicleVisual.localPosition, Vector3.zero, t/tt); 
            vehicleVisual.localRotation = Quaternion.Lerp(vehicleVisual.localRotation, Quaternion.identity, t/tt); 
            yield return null;
        }
    }
    
    public void SetCarInput(float hor, float ver, bool brake)
    {
        // перекинуть в общий прием инпута от юнитов
        wheelVehicle.SetInput(hor, ver, brake);
    }
}