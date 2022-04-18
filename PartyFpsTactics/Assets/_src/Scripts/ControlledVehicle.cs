using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using MrPink.PlayerSystem;
using UnityEngine;
using VehicleBehaviour;

public class ControlledVehicle : MonoBehaviour
{
    public WheelVehicle wheelVehicle;
    public Transform vehicleVisual;
    public float visualFollowSpeed = 10;
    public Transform sitTransform;
    public Transform sitTransformNpc;
    public List<Collider> driveColliders;
    public void StartInput()
    {
        wheelVehicle.IsPlayer = true;
        wheelVehicle.Handbrake = false;
        
        for (int i = 0; i < driveColliders.Count; i++)
        {
            driveColliders[i].gameObject.SetActive(true);    
        }

        if (visualFollowCoroutine != null)
            StopCoroutine(visualFollowCoroutine);
        
        visualFollowCoroutine = StartCoroutine(VisualFollow());
    }

    private Coroutine visualFollowCoroutine;
    IEnumerator VisualFollow()
    {
        vehicleVisual.transform.parent = null;
        while (true)
        {
            vehicleVisual.transform.position = Vector3.Lerp(vehicleVisual.transform.position, transform.position, visualFollowSpeed * Time.deltaTime);
            vehicleVisual.transform.rotation = Quaternion.Lerp(vehicleVisual.transform.rotation, transform.rotation, visualFollowSpeed * Time.deltaTime);
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

        for (int i = 0; i < driveColliders.Count; i++)
        {
            driveColliders[i].gameObject.SetActive(false);    
        }

        visualFollowCoroutine = StartCoroutine(ResetVisualTransform());
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