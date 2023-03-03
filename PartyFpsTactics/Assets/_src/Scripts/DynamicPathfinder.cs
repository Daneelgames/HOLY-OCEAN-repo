using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using MrPink;
using MrPink.Units;
using Sirenix.OdinInspector;
using UnityEngine;
using Unity.EditorCoroutines.Editor;
using Unity.VisualScripting;

public class DynamicPathfinder : MonoBehaviour
{
    public static DynamicPathfinder Instance;
    [Header("DEBUG ")][SerializeField] private Transform a, b;
    [SerializeField] private float heightThreshold = 5;
    [SerializeField] private float unitCooldownAfterFindingPath = 3;
    [SerializeField] private int maxRecastAmount = 10;
    [SerializeField] private float spherecastDistance = 50;
    [SerializeField] private float spherecastRadius = 10;
    [SerializeField] private float dontStartPathfindingIfCloserThan = 5;
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(UpdateCoroutine());
    }

    IEnumerator UpdateCoroutine()
    {
        while (Game._instance == null || Game.LocalPlayer == null)
        {
            yield return null;
        }
        
        while (true)
        {
            yield return null;

            int unitsAmount = UnitsManager.Instance.HcInGame.Count;
            if (unitsAmount < 1)
                continue;
            
            for (int i = unitsAmount - 1; i >= 0; i--)
            {
                yield return null;
                var unit = UnitsManager.Instance.HcInGame[i];
                if (unit == null || unit.health < 1 || unit.gameObject.activeInHierarchy == false)
                    continue;
                
                Debug.Log("Dynamic pathfinding 0");
                if (unit.selfUnit == null || unit.selfUnit.UnitMovement == null)
                    continue;

                Debug.Log("Dynamic pathfinding 1");
                var targetPos = unit.selfUnit.UnitMovement.GetTargetPositionToReach;
                if (Vector3.Distance(targetPos, unit.transform.position) < dontStartPathfindingIfCloserThan)
                    continue;

                Debug.Log("Dynamic pathfinding 2");
                AskForPath(unit.selfUnit.UnitMovement, targetPos);
            }
        }
    }

    [Button]
    public void DebugPathfinding()
    {
        var hunterUnitEyes = a.position + Vector3.up; 
        var preyUnitEyes = b.position + Vector3.up; 
        if (Application.isEditor && Application.isPlaying == false)
            EditorCoroutineUtility.StartCoroutine(AskForPath(hunterUnitEyes, preyUnitEyes), this);
        else
            StartCoroutine(AskForPath(hunterUnitEyes, preyUnitEyes));
    }

    [Serializable]
    public struct Path
    {
        public List<Vector3> points;
    }

    void AskForPath(UnitMovement askingUnitMovement, Vector3 endPos)
    {
        if (_unitMovementsInQueue.Contains(askingUnitMovement))
            return;
        
        _unitMovementsInQueue.Add(askingUnitMovement);
        StartCoroutine(AskForPath(askingUnitMovement.transform.position + Vector3.up, endPos, askingUnitMovement));
    }

    private List<UnitMovement> _unitMovementsInQueue = new List<UnitMovement>();

    IEnumerator AskForPath(Vector3 startPos, Vector3 endPos, UnitMovement askingUnitMovement = null)
    {
       List<Vector3> path = new List<Vector3>();
       float distance = Vector3.Distance(startPos, endPos);
       var direction = (endPos - startPos).normalized;
       float step = 1;

       // distance = 100
       int raycastsAmount = Mathf.RoundToInt(distance / step);
       int recastAmount = 0;
       Vector3 raycastPosOnPath = startPos;
       for (int i = 0; i < raycastsAmount; i++)
       {
           bool found = false;
           raycastPosOnPath += direction * step;
           while (!found)
           {
               yield return null;
               
               if (Physics.SphereCast(raycastPosOnPath, spherecastRadius, Vector3.down, out var hit, spherecastDistance, 1 << 6, QueryTriggerInteraction.Ignore)) // solids only
               {
                   if (path.Count > 0)
                   {
                       var prevPoint = path[path.Count - 1];
                       if (hit.point.y < prevPoint.y - heightThreshold)
                       {
                           // sudden drop, let's recast again from prev position
                           recastAmount++;
                           if (recastAmount < maxRecastAmount)
                           {
                               raycastPosOnPath += Vector3.up * recastAmount;
                               direction = (endPos - raycastPosOnPath).normalized;
                               Debug.DrawLine(prevPoint, raycastPosOnPath, Color.red, 0.5f, false);
                               continue;
                           }
                           Debug.LogWarning("CANT FIND WAY FROM POINT " + raycastPosOnPath);
                       }
                       Debug.DrawLine(prevPoint, hit.point, Color.blue, 10, false);
                   }
               
                   recastAmount = 0;
                   path.Add(hit.point);
                   //direction = (endPos - hit.point).normalized;
                   //raycastPosOnPath = hit.point + direction * step;
                   found = true;
               }
               else
               {
                   raycastPosOnPath += direction * step;
                   found = true;
               }
           }
       }

       Path newPath = new Path();
       newPath.points = new List<Vector3>(path);
       
       if (askingUnitMovement && askingUnitMovement.gameObject.activeInHierarchy)
       {
           askingUnitMovement.SetNewPath(newPath);

           yield return new WaitForSeconds(unitCooldownAfterFindingPath);

           _unitMovementsInQueue.Remove(askingUnitMovement);
       }
    }
}