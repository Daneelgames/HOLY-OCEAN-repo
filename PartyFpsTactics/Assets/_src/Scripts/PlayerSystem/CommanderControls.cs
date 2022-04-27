using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace MrPink.PlayerSystem
{
    public class CommanderControls : MonoBehaviour
    {
        
        public LayerMask layersToRaycastOrders;
        public int unitToGiveOrder = 0;
        public List<HealthController> unitsInParty;

        public GameObject moveOrderVisualFeedback;
        private Vector3 moveOrderCurrentPos;

        private float cooldownForRunOrder = 0.5f;

        private IEnumerator Start()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(10f, 30f));
                unitToGiveOrder = 0;
                FollowLeader();
            }
        }

        private void Update()
        {
            if (cooldownForRunOrder > 0)
                cooldownForRunOrder -= Time.deltaTime;
            
            return;
            OrderControls();
        }

        void OrderControls()
        {

            if (Input.GetKeyDown(KeyCode.Alpha1))
                unitToGiveOrder = 0;
            if (Input.GetKeyDown(KeyCode.Alpha2))
                unitToGiveOrder = 1;
            if (Input.GetKeyDown(KeyCode.Alpha3))
                unitToGiveOrder = 2;
        
        
            if (unitsInParty.Count <= unitToGiveOrder || unitsInParty[unitToGiveOrder].health < 0)
                return;
        
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
        
            if (unitsInParty[unitToGiveOrder].AiMovement)
                unitsInParty[unitToGiveOrder].AiMovement.FollowTargetOrder(transform);
            return;
        
            for (int i = 0; i < unitsInParty.Count; i++)
            {
                if (unitsInParty[i].AiMovement)
                    unitsInParty[i].AiMovement.FollowTargetOrder(transform);
            }
        }
    
        bool MoveOrder()
        {
            var ray = Game.Player.MainCamera.ScreenPointToRay(Input.mousePosition);

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
        
            if (unitsInParty[unitToGiveOrder].AiMovement)
                unitsInParty[unitToGiveOrder].AiMovement.MoveToPositionOrder(closestNavPoint);
        
            moveOrderCurrentPos = closestNavPoint;
            return true;
        }

        void RunOrder()
        {
            if (unitsInParty.Count <= unitToGiveOrder)
                return;
        
            if (unitsInParty[unitToGiveOrder].AiMovement)
                unitsInParty[unitToGiveOrder].AiMovement.RunOrder();
        
            return;
        
            for (int i = 0; i < unitsInParty.Count; i++)
            {
                if (unitsInParty[i].AiMovement)
                    unitsInParty[i].AiMovement.RunOrder();
            }
        }
    }
}