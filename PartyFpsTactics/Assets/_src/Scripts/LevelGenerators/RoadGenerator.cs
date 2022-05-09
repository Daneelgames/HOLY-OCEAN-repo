using System;
using System.Collections;
using System.Collections.Generic;
using MrPink;
using MrPink.PlayerSystem;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class RoadGenerator : MonoBehaviour
{
    public static RoadGenerator Instance;
    public List<RoadPart> roadPartsPrefabsStraight = new List<RoadPart>();
    public List<RoadPart> roadPartsPrefabsStraightRight = new List<RoadPart>();
    public List<RoadPart> roadPartsPrefabsStraightLeft = new List<RoadPart>();
    public List<RoadPart> roadPartsPrefabsLeadingUp = new List<RoadPart>();
    public List<RoadPart> roadPartsPrefabsLeadingUpRight = new List<RoadPart>();
    public List<RoadPart> roadPartsPrefabsLeadingUpLeft = new List<RoadPart>();
    public List<RoadPart> roadPartsPrefabsLeadingDown = new List<RoadPart>();
    public List<RoadPart> roadPartsPrefabsLeadingDownRight = new List<RoadPart>();
    public List<RoadPart> roadPartsPrefabsLeadingDownLeft = new List<RoadPart>();
    public bool showBrokenPrefabs = false;
    [ShowIf("showBrokenPrefabs", true)]
    public List<RoadPart> roadPartsBrokenPrefabsStraight = new List<RoadPart>();
    [ShowIf("showBrokenPrefabs", true)]
    public List<RoadPart> roadPartsBrokenPrefabsStraightRight = new List<RoadPart>();
    [ShowIf("showBrokenPrefabs", true)]
    public List<RoadPart> roadPartsBrokenPrefabsStraightLeft = new List<RoadPart>();
    [ShowIf("showBrokenPrefabs", true)]
    public List<RoadPart> roadPartsBrokenPrefabsLeadingUp = new List<RoadPart>();
    [ShowIf("showBrokenPrefabs", true)]
    public List<RoadPart> roadPartsBrokenPrefabsLeadingUpRight = new List<RoadPart>();
    [ShowIf("showBrokenPrefabs", true)]
    public List<RoadPart> roadPartsBrokenPrefabsLeadingUpLeft = new List<RoadPart>();
    [ShowIf("showBrokenPrefabs", true)]
    public List<RoadPart> roadPartsBrokenPrefabsLeadingDown = new List<RoadPart>();
    [ShowIf("showBrokenPrefabs", true)]
    public List<RoadPart> roadPartsBrokenPrefabsLeadingDownRight = new List<RoadPart>();
    [ShowIf("showBrokenPrefabs", true)]
    public List<RoadPart> roadPartsBrokenPrefabsLeadingDownLeft = new List<RoadPart>();
    
    public List<RoadPart> spawnedRoadParts = new List<RoadPart>();
    public List<GameObject> spawnedRoadPartsGO = new List<GameObject>();
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
            //DestroyImmediate(spawnedRoadParts[i].gameObject);
        }
        spawnedRoadParts.Clear();
        spawnedRoadPartsGO.Clear();
    }

    public void RemoveFromSpawnedParts(RoadPart roadPart)
    {
        spawnedRoadParts.Remove(roadPart);
        spawnedRoadPartsGO.Remove(roadPart.gameObject);
    }


    [Button("FORWARD")]
    public void SpawnStraight()
    {
        roadPartToSpawn = roadPartsPrefabsStraight[Random.Range(0, roadPartsPrefabsStraight.Count)];
        GenerateRoad();
    }
    [Button("FWD R")]
    public void SpawnStraightRight()
    {
        roadPartToSpawn = roadPartsPrefabsStraightRight[Random.Range(0, roadPartsPrefabsStraightRight.Count)];
        if (roadPartToSpawn == null)
            Debug.LogError("NO ROAD PARTS HERE");
        GenerateRoad();
    }
    [Button("FWD L")]
    public void SpawnStraightLeft()
    {
        roadPartToSpawn = roadPartsPrefabsStraightLeft[Random.Range(0, roadPartsPrefabsStraightLeft.Count)];
        if (roadPartToSpawn == null)
            Debug.LogError("NO ROAD PARTS HERE");
        GenerateRoad();
    }
    [Button("UP")]
    public void SpawnLeadingUp()
    {
        roadPartToSpawn = roadPartsPrefabsLeadingUp[Random.Range(0, roadPartsPrefabsLeadingUp.Count)];
        if (roadPartToSpawn == null)
            Debug.LogError("NO ROAD PARTS HERE");
        GenerateRoad();
    }
    [Button("UP R")]
    public void SpawnLeadingUpRIght()
    {
        roadPartToSpawn = roadPartsPrefabsLeadingUpRight[Random.Range(0, roadPartsPrefabsLeadingUpRight.Count)];
        if (roadPartToSpawn == null)
            Debug.LogError("NO ROAD PARTS HERE");
        GenerateRoad();
    }
    [Button("UP L")]
    public void SpawnLeadingUpLeft()
    {
        roadPartToSpawn = roadPartsPrefabsLeadingUpLeft[Random.Range(0, roadPartsPrefabsLeadingUpLeft.Count)];
        if (roadPartToSpawn == null)
            Debug.LogError("NO ROAD PARTS HERE");
        GenerateRoad();
    }
    [Button("DOWN")]
    public void SpawnLeadingDown()
    {
        roadPartToSpawn = roadPartsPrefabsLeadingDown[Random.Range(0, roadPartsPrefabsLeadingDown.Count)];
        if (roadPartToSpawn == null)
            Debug.LogError("NO ROAD PARTS HERE");
        GenerateRoad();
    }
    [Button("DOWN R")]
    public void SpawnLeadingDownRight()
    {
        roadPartToSpawn = roadPartsPrefabsLeadingDownRight[Random.Range(0, roadPartsPrefabsLeadingDownRight.Count)];
        if (roadPartToSpawn == null)
            Debug.LogError("NO ROAD PARTS HERE");
        GenerateRoad();
    }
    [Button("DOWN L")]
    public void SpawnLeadingDownLeft()
    {
        roadPartToSpawn = roadPartsPrefabsLeadingDownLeft[Random.Range(0, roadPartsPrefabsLeadingDownLeft.Count)];
        if (roadPartToSpawn == null)
            Debug.LogError("NO ROAD PARTS HERE");
        GenerateRoad();
    }
    [Button("RemoveLastSpawned")]
    public void RemoveLast()
    {
        if (spawnedRoadParts.Count <= 0)
            return;
        
        var i = spawnedRoadParts[spawnedRoadParts.Count - 1];
        spawnedRoadPartsGO.Remove(i.gameObject);
        spawnedRoadParts.Remove(i);
        DestroyImmediate(i.gameObject);
    }
    
    void GenerateRoad()
    {
        Debug.LogError("GenerateRoad is disabled. Uncomment stuff below to edit stuff");
        
        /*
        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;
        

        if (spawnedRoadParts.Count <= 0)
            spawnPos = transform.position;
        else
        {
            spawnPos = spawnedRoadParts[spawnedRoadParts.Count-1].roadEnds[0].position;
            spawnRot = spawnedRoadParts[spawnedRoadParts.Count-1].roadEnds[0].rotation;
        }

        var o = PrefabUtility.InstantiatePrefab(roadPartToSpawn.gameObject);
        GameObject go = o as GameObject;
        Debug.Log("GameObject go = o as GameObject: " + go +"; roadPartToSpawn " + roadPartToSpawn);
        var newPart = go.GetComponent<RoadPart>();
        
        newPart.transform.rotation = spawnRot;
        newPart.transform.position = spawnPos - newPart.roadStart.localPosition;

        spawnedRoadParts.Add(newPart);
        spawnedRoadPartsGO.Add(newPart.gameObject);
        newPart.transform.parent = transform;*/
    }

    private RoadPart roadPartToSpawn;
    public IEnumerator GenerateRoadCoroutine()
    {
        gizmoRaycastPositions.Clear();
        gizmoRaycastDirections.Clear();
        gizmoRaycastHitPoints.Clear();
        
        DestroyRoad();
        
        Vector3 spawnPos = transform.position;
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
            spawnedRoadPartsGO.Add(newPart.gameObject);
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

    public Transform GetPlayerPosOnRoadEnd()
    {
        Transform t = null;
        Debug.Log("spawnedRoadParts.Count: " + spawnedRoadParts.Count);
        for (int i = spawnedRoadParts.Count - 1; i >= 0; i--)
        {
            if (spawnedRoadParts[i].roadType == RoadPart.RoadType.Straight)
            {
                t = spawnedRoadParts[i].transform;
                break;
            }
        }

        return t;
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
