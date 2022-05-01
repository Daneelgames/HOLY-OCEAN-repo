using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityDebug;
using MrPink.PlayerSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MrPink
{
    public class GrindRail : MonoBehaviour
    {
        public List<Transform> nodes;

        private List<Transform> nodesInOrderOfRide = new List<Transform>();
        private int currentTargetNode = 0;
        [Range(2,10)]
        public int nodesAmount = 5;
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            if (nodes.Count < 2)
                return;
        
            for (int i = 0; i < nodes.Count-1; i++)
            {
                Gizmos.DrawLine(nodes[i].position, nodes[i+ 1].position);
            }
        }

        private void Start()
        {
            GenerateNodes();
        }

        public void GenerateNodes(bool constructRail = false)
        {
            return;
        
            for (int i = 0; i < nodesAmount; i++)
            {
                var t = new GameObject("Node " + i);
                t.transform.parent = transform;
                t.transform.localPosition = Vector3.zero;
                nodes.Add(t.transform);
            }

            var firstNode = nodes[0];
            var lastNode = nodes[nodes.Count - 1];
            lastNode.transform.position = BuildingGenerator.Instance.levelGoalSpawned.transform.position + Random.insideUnitSphere * 100f;
            Vector3 direction = (lastNode.position - firstNode.position).normalized;
            float distance = Vector3.Distance(firstNode.position, lastNode.position);
            for (int i = 1; i < nodes.Count-1; i++)
            {
                var _node = nodes[i];
                _node.position = firstNode.position + direction * (distance / nodesAmount) * i;
                _node.position += Random.onUnitSphere * Random.Range(10f,50f);
            }
            // place the first node
            // then place the final node at random pos
            // then try to connect nodes with raycasts
        
            if (constructRail)
                ConstructRail();
        }
    
        [ContextMenu("ConstructRail")]
        public void ConstructRail()
        {
            for (int i = 0; i < nodes.Count-1; i++)
            {
                var newRail = GameObject.CreatePrimitive(PrimitiveType.Cube);
                newRail.transform.position = (nodes[i].position +nodes[i+1].position) / 2;
                newRail.transform.LookAt(nodes[i+1]);
                newRail.transform.localScale = new Vector3(0.3f, 0.1f, Vector3.Distance(nodes[i].position, nodes[i + 1].position));
            
                // also destroy interconnected tiles ? 
            }
        }

        [ContextMenu("RidePlayer")]
        public void RidePlayer()
        {
            //decide what direction player should go
            nodesInOrderOfRide = new List<Transform>(nodes);
            currentTargetNode = 1;
        
            Game.Player.Movement.SetGrindRail(this);
        }

        public Transform GetTargetNode()
        {
            if (Vector3.Distance(Game.Player.Position, nodesInOrderOfRide[currentTargetNode].position) < 0.5f)
                currentTargetNode++;
        
            if (currentTargetNode < nodesInOrderOfRide.Count)
                return nodesInOrderOfRide[currentTargetNode];

            return null;
        }
    }
}