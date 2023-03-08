using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using MrPink;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerHookshot : NetworkBehaviour
{
    public static PlayerHookshot Instance;
    
    [SerializeField] private float maxHookshotDistance = 50;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] [ReadOnly] private Transform hookPoint;
    [SerializeField] private LineRenderer hookshotLineRenderer;
    private bool swinging = false;
    [SerializeField] private AudioSource hookshotAu;
    [SerializeField] private List<GameObject> collidersToIgnore = new List<GameObject>();

    
    [SerializeField] float swingingMinDistance = 0.01f;
    [SerializeField] float swingingMaxDistance = 0.1f;
    [SerializeField] float swingingSpring = 50f;
    [SerializeField] float swingingDamper = 7f;
    [SerializeField] float swingingMassScale = 4.5f;
    
    private SpringJoint joint;
    
    public override void OnOwnershipClient(NetworkConnection prevOwner)
    {
        base.OnOwnershipClient(prevOwner);
        /* Current owner can be found by using base.Owner. prevOwner
        * contains the connection which lost ownership. Value will be
        * -1 if there was no previous owner. */

        Instance = this;
    }


    private void Update()
    {
        if (base.IsOwner == false)
            return;
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (swinging)
            {
                StopSwing();
            }
            else
            {
                StartSwing();
            }
        }

        if (swinging && joint)
        {
            if (hookPoint == null)
            {
                StopSwing();
                return;
            }
            joint.connectedAnchor = hookPoint.position;
            hookshotLineRenderer.SetPosition(0, Game._instance.PlayerCamera.transform.position + Vector3.down);
            hookshotLineRenderer.SetPosition(1, hookPoint.position);
        }
    }

    public void AddColliderToIgnore(GameObject go)
    {
        if (collidersToIgnore.Contains(go))
            return;
        collidersToIgnore.Add(go);
    }
    
    public void RemoveColliderToIgnore(GameObject go)
    {
        if (collidersToIgnore.Contains(go) == false)
            return;
        collidersToIgnore.Remove(go);
    }
    


    void StartSwing()
    {
        var cam = Game._instance.PlayerCamera;
        var hits = Physics.SphereCastAll(cam.transform.position + cam.transform.forward, 0.75f, cam.transform.forward, maxHookshotDistance, _layerMask, QueryTriggerInteraction.Ignore);
        RaycastHit _hit = new RaycastHit();
        foreach (var hit in hits)
        {
            if (Vector3.Distance(hit.point, transform.position) > maxHookshotDistance)
                continue;
            if (collidersToIgnore.Contains(hit.collider.gameObject))
                continue;
            if (Game.LocalPlayer.VehicleControls.controlledMachine && Game.LocalPlayer.VehicleControls.controlledMachine.gameObject == hit.collider.gameObject)
                continue;
            _hit = hit;
            break;
        }            

        if (_hit.collider == null)
        {
            StopSwing();
            return;
        }
        
        if (Game.LocalPlayer.VehicleControls.controlledMachine)
            Game.LocalPlayer.VehicleControls.RequestVehicleAction(Game.LocalPlayer.VehicleControls.controlledMachine);
        
        Game.LocalPlayer.Movement.State.IsSwinging = true;
        if (hookPoint == null)
            hookPoint = new GameObject("HookshotPoint").transform;

        hookPoint.position = _hit.point;
        hookPoint.parent = _hit.collider.transform;

        if (joint == null)
            joint = gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = hookPoint.position;

        hookshotAu.pitch = Random.Range(0.75f, 1.25f);
        hookshotAu.Play();
        /*
         var distanceFromPoint = Vector3.Distance(transform.position, hookPoint.position);
        joint.maxDistance = distanceFromPoint * 0.8f;
        joint.minDistance = distanceFromPoint * 0.25f;
        */
        joint.maxDistance = swingingMaxDistance;
        joint.minDistance = swingingMinDistance;

        joint.spring = swingingSpring;
        joint.damper = swingingDamper;
        joint.massScale = swingingMassScale;
        
        hookshotLineRenderer.positionCount = 2;
        swinging = true;
    }

    public void StopSwing()
    {
        Game.LocalPlayer.Movement.State.IsSwinging = false;
        hookshotLineRenderer.positionCount = 0;
        if (joint)
            Destroy(joint);
        if (hookPoint)
            Destroy(hookPoint.gameObject);
        swinging = false;
    }
}
