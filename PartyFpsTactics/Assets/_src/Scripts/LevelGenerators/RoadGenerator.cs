using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.PlayerSystem;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoadGenerator : MonoBehaviour
{
    public static RoadGenerator Instance;
    public List<RoadPart> roadPartsPrefabsStraight = new List<RoadPart>();
    public List<RoadPart> roadPartsPrefabsLeadingUp = new List<RoadPart>();
    public List<RoadPart> roadPartsPrefabsLeadingDown = new List<RoadPart>();
    public List<RoadPart> roadPartsBrokenPrefabsStraight = new List<RoadPart>();
    public List<RoadPart> roadPartsBrokenPrefabsLeadingUp = new List<RoadPart>();
    public List<RoadPart> roadPartsBrokenPrefabsLeadingDown = new List<RoadPart>();
    List<RoadPart> spawnedRoadParts = new List<RoadPart>();
    public int roadPartsToSpawn = 20;
    public LayerMask allSolidsMask;

    private void Awake()
    {
        Instance = this;
    }

    [ContextMenu("DestroyRoad")]
    public void DestroyRoad()
    {
        for (int i = 0; i < spawnedRoadParts.Count; i++)
        {
            if (spawnedRoadParts[i] == null)
                continue;
            
            Destroy(spawnedRoadParts[i].gameObject);
        }
        spawnedRoadParts.Clear();
    }

    public void RemoveFromSpawnedParts(RoadPart roadPart)
    {
        spawnedRoadParts.Remove(roadPart);
    }
    
    [ContextMenu("GenerateRoad")]
    public void GenerateRoad()
    {
        Console.Clear();
        StartCoroutine(GenerateRoadCoroutine());
    }

    private RoadPart roadPartToSpawn;
    IEnumerator GenerateRoadCoroutine()
    {
        gizmoRaycastPositions.Clear();
        gizmoRaycastDirections.Clear();
        gizmoRaycastHitPoints.Clear();
        
        DestroyRoad();
        
        Vector3 spawnPos = new Vector3(Player.Movement.transform.position.x, -0.5f, Player.Movement.transform.position.z);
        Quaternion spawnRot = Quaternion.identity;
        
        for (int i = 0; i < roadPartsToSpawn; i++)
        {
           FindPrefabToSpawn(spawnPos, spawnRot);

            if (roadPartToSpawn == null)
            {
                Debug.Log("Can't spawn any RoadPart; spawnPos = " + spawnPos +"; spawnRot = " + spawnRot);

                var roadPartsBrokenPrefabsCurrent = new List<RoadPart>(GetCurrentRoadParts(spawnPos.y,
                    roadPartsBrokenPrefabsStraight, roadPartsBrokenPrefabsLeadingDown,
                    roadPartsBrokenPrefabsLeadingUp));
                
                
                roadPartToSpawn = roadPartsBrokenPrefabsCurrent[Random.Range(0, roadPartsBrokenPrefabsCurrent.Count)];
            }
            var newPart = Instantiate(roadPartToSpawn);
            newPart.name += " " + i;
            newPart.transform.rotation = spawnRot;
            newPart.transform.position = spawnPos - newPart.roadStart.localPosition;

            spawnPos = newPart.roadEnds[0].position;
            spawnRot = newPart.roadEnds[0].rotation;
            spawnedRoadParts.Add(newPart);
            newPart.transform.parent = transform;
            newPart.Init();
            yield return null;
        }
    }

    List<RoadPart> GetCurrentRoadParts(float spawnHeight, List<RoadPart> straight,List<RoadPart> down,List<RoadPart> up)
    {
        List<RoadPart> listToFill = new List<RoadPart>();
        
        for (int j = 0; j < straight.Count; j++)
        {
            listToFill.Add(straight[j]);
        }
        if (spawnHeight > 20)
        {
            for (int j = 0; j < down.Count; j++)
            {
                listToFill.Add(down[j]);
            }
        }
        if (spawnHeight < 100)
        {
            for (int j = 0; j < up.Count; j++)
            {
                listToFill.Add(up[j]);
            }
        }

        return listToFill;
    }
    
    void FindPrefabToSpawn(Vector3 spawnPos, Quaternion spawnRot)
    {
        roadPartToSpawn = null;
        
        List<RoadPart> prefabsTemp = GetCurrentRoadParts(spawnPos.y, roadPartsPrefabsStraight, roadPartsPrefabsLeadingDown, roadPartsPrefabsLeadingUp);
        var prefabToSpawn = prefabsTemp[Random.Range(0, prefabsTemp.Count)];

        bool prefabFound = false;

        while (!prefabFound)
        {
            if (prefabsTemp.Count == 0)
            {
                Debug.Log("No RoadPart found. prefabsTemp.Count == 0");
                return;
            }
            
            prefabToSpawn = prefabsTemp[Random.Range(0, prefabsTemp.Count)];
            prefabFound = true;

            GameObject roadPartTemp = new GameObject("RoadPartTemp");
            roadPartTemp.transform.position = spawnPos - prefabToSpawn.roadStart.transform.localPosition;
            roadPartTemp.transform.rotation = spawnRot;
            
            // check if this place is free
            for (int i = 0; i < prefabToSpawn.raycastTransforms.Count; i++)
            {
                Transform raycastTransform = new GameObject("RaycastTransform " + i).transform;
                raycastTransform.parent = roadPartTemp.transform;
                raycastTransform.localPosition = prefabToSpawn.raycastTransforms[i].localPosition * prefabToSpawn.transform.localScale.x;
                raycastTransform.rotation = prefabToSpawn.raycastTransforms[i].rotation;
                
                if (Physics.Raycast(raycastTransform.position - raycastTransform.forward * 1000,
                    raycastTransform.forward, out var hit, Mathf.Infinity, allSolidsMask))
                {
                    if (GameManager.Instance.terrainAndIslandsColliders.Contains(hit.collider) == false)
                    {
                        Debug.Log("Raycast RoadParts. Found " + hit.collider.gameObject.name + ". Want to prefabToSpawn: " + prefabToSpawn);
                        gizmoRaycastPositions.Add(raycastTransform.position/* - raycastTransform.forward * 200*/);
                        gizmoRaycastDirections.Add(raycastTransform.forward);
                        gizmoRaycastHitPoints.Add(hit.point);
                        prefabFound = false;
                        prefabsTemp.Remove(prefabToSpawn);
                        break;
                    }
                }
            }
            Destroy(roadPartTemp);
        }

        roadPartToSpawn = prefabToSpawn;
    }

    private List<Vector3> gizmoRaycastPositions = new List<Vector3>();
    private List<Vector3> gizmoRaycastDirections = new List<Vector3>();
    private List<Vector3> gizmoRaycastHitPoints = new List<Vector3>();
    
    private void OnDrawGizmos()
    {
        
        for (int i = 0; i < gizmoRaycastDirections.Count; i++)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(gizmoRaycastPositions[i], gizmoRaycastPositions[i] + gizmoRaycastDirections[i] * Mathf.Infinity);
            Gizmos.color = Color.red;    
            Gizmos.DrawWireSphere(gizmoRaycastHitPoints[i], 1);
        }
    }
}
