using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CommanderControls : MonoBehaviour
{
    public Camera commanderPlayerCamera;
    public LayerMask layersToRaycastOrders;
    public List<HealthController> unitsInParty;

    public GameObject moveOrderVisualFeedback;
    private Vector3 moveOrderCurrentPos;

    private float cooldownForRunOrder = 0.5f;
    
    private void Update()
    {
        if (cooldownForRunOrder > 0)
            cooldownForRunOrder -= Time.deltaTime;
        
        OrderControls();
    }

    void OrderControls()
    {
        if (Input.GetKeyDown(KeyCode.Z)) // FOLLOW
        {
            if (cooldownForRunOrder > 0)
            {
                RunOrder();
                return;   
            }
                    
            cooldownForRunOrder = 0.5f;
            FollowLeader();
            ToggleMoveOrderFeedback(false, transform, transform.position);
        }
        
        if (Input.GetKeyDown(KeyCode.X)) // MOVE TO POINT
        {
            if (cooldownForRunOrder > 0)
            {
                RunOrder();
                return;   
            }
                    
            if (MoveOrder()) // feedback on positive order
            {
                cooldownForRunOrder = 0.5f;
                ToggleMoveOrderFeedback(true, null, moveOrderCurrentPos);
            }
        }
    }

    void ToggleMoveOrderFeedback(bool active, Transform parentTransform, Vector3 newPosition)
    {
        moveOrderVisualFeedback.SetActive(active);
        moveOrderVisualFeedback.transform.parent = parentTransform;
        moveOrderVisualFeedback.transform.position = newPosition;
    }
    
    void FollowLeader()
    {
        
        for (int i = 0; i < unitsInParty.Count; i++)
        {
            if (unitsInParty[i].AiMovement)
                unitsInParty[i].AiMovement.FollowTargetOrder(transform);
        }
    }
    
    bool MoveOrder()
    {
        var ray = commanderPlayerCamera.ScreenPointToRay(Input.mousePosition);

        Vector3 worldPointOnMouse = Vector3.zero;
        
        
        if (Physics.Raycast(ray, out var hitData, 1000, layersToRaycastOrders))
        {
            worldPointOnMouse = hitData.point;
        }
        else
        {
            return false;
        }
        
        Vector3 closestNavPoint = worldPointOnMouse;
        if (NavMesh.SamplePosition(closestNavPoint, out var hit, 10f, NavMesh.AllAreas))
        {
            closestNavPoint = hit.position;
        }
        else
        {
            return false;
        }
        
        for (int i = 0; i < unitsInParty.Count; i++)
        {
            if (unitsInParty[i].AiMovement)
                unitsInParty[i].AiMovement.MoveToPositionOrder(closestNavPoint);
        }

        moveOrderCurrentPos = closestNavPoint;
        return true;
    }

    void RunOrder()
    {
        for (int i = 0; i < unitsInParty.Count; i++)
        {
            if (unitsInParty[i].AiMovement)
                unitsInParty[i].AiMovement.RunOrder();
        }
    }
}
