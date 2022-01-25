using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CommanderControls : MonoBehaviour
{
    [Header("1: FollowLeader; 2: MoveToPoint")]
    [Range(1,2)]
    public int currentOrder = 1; 
    public Camera commanderPlayerCamera;
    public LayerMask layersToRaycastOrders;
    public List<HealthController> unitsInParty;

    public GameObject moveOrderVisualFeedback;
    private Vector3 moveOrderCurrentPos;
    
    private void Update()
    {
        OrderControls();
        
    }

    void OrderControls()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            currentOrder = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentOrder = 2;
        }
        
        
        if (Input.GetMouseButtonDown(1))
        {
            switch (currentOrder)
            {
                case 1:
                    FollowLeader();
                    ToggleMoveOrderFeedback(false, transform, transform.position);
                    break;
                case 2:
                    
                    if (MoveOrder()) // feedback on positive order
                    {
                        ToggleMoveOrderFeedback(true, null, moveOrderCurrentPos);
                    }
                    else // feedback on negative order
                        return;
                    break;
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
                unitsInParty[i].AiMovement.FollowLeaderOrder(transform);
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
                unitsInParty[i].AiMovement.MoveOrder(closestNavPoint);
        }

        moveOrderCurrentPos = closestNavPoint;
        return true;
    }
}
