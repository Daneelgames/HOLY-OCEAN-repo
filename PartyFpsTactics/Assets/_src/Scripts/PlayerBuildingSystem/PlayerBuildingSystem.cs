using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using MrPink;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerBuildingSystem : NetworkBehaviour
{
    public static PlayerBuildingSystem Instance;
    [SerializeField] private float buildingDistanceMax = 100;
    [SerializeField] private float noRaycastBuildingMediumDistance = 50;
    [SerializeField] private float buildingDistanceMin = 20;
    [SerializeField] private float buildingRotationSpeed = 10;
    [SerializeField] private float tileMaxSpawnDistance = 10;
    public GameObject testBuildingPrefab;
    public GameObject testTilePrefab;
    private GameObject currentPlacingBuilding;
    public override void OnStartClient()
    {
        base.OnStartClient();

        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
            SpawnTile();
        if (Input.GetKeyDown(KeyCode.B))
            PlaceBuilding();
    }

    public void CancelInput()
    {
        if (currentPlacingBuilding != null)
        {
            StopCoroutine(placingBuildingCoroutine);
        }

        if (currentPlacingBuilding != null)
        {
            Destroy(currentPlacingBuilding);
        }
    }

    void SpawnTile()
    {
        Transform camTransform = Game.LocalPlayer.MainCamera.transform;
        if (Physics.Raycast(camTransform.position, camTransform.forward, out var hit, tileMaxSpawnDistance,
            GameManager.Instance.AllSolidsMask) == false)
            return;
        
        var newTile = Instantiate(testTilePrefab);
        var spawnPos = new Vector3(Mathf.RoundToInt(hit.point.x),Mathf.RoundToInt(hit.point.y), Mathf.RoundToInt(hit.point.z));
        newTile.transform.position = spawnPos;
        newTile.transform.Rotate(new Vector3(90 * Random.Range(0,4),90 * Random.Range(0,4),90 * Random.Range(0,4)));
        
        IslandSpawner.Instance.GetClosestTileBuilding(transform.position)?.AddToDisconnectedTilesFolder(newTile.transform);
    }

    void PlaceBuilding()
    {
        if (placingBuildingCoroutine != null && currentPlacingBuilding != null)
        {
            // place building;
            StopCoroutine(placingBuildingCoroutine);
            GameManager.Instance.SetLayerRecursively(currentPlacingBuilding.transform, 6); // solids
            currentPlacingBuilding.SetActive(true);
            currentPlacingBuilding = null;
            return;
        }
        
        // spawn new building
        currentPlacingBuilding = Instantiate(testBuildingPrefab);
        GameManager.Instance.SetLayerRecursively(currentPlacingBuilding.transform, 31); // ignore collision
        
        placingBuildingCoroutine = StartCoroutine(PlacingBuilding());
    }

    private Coroutine placingBuildingCoroutine;

    IEnumerator PlacingBuilding()
    {
        Transform camTransform = Game.LocalPlayer.MainCamera.transform;
        float t = 0.3f;
        while (true)
        {
            yield return null;
            
            t -= Time.deltaTime;
            if (t < 0)
            {
                t = 0.3f;
                currentPlacingBuilding.SetActive(!currentPlacingBuilding.activeInHierarchy);
            }
            
            if (Input.GetKey(KeyCode.R))
                currentPlacingBuilding.transform.Rotate(0,buildingRotationSpeed * Time.deltaTime, 0);
            
            if (Physics.Raycast(camTransform.position, camTransform.forward, out var hit, buildingDistanceMax,
                GameManager.Instance.AllSolidsMask) == false)
            {
                currentPlacingBuilding.transform.position = camTransform.position + camTransform.forward * noRaycastBuildingMediumDistance;
                continue;
            }
            
            if (Vector3.Distance(hit.point, camTransform.position) < buildingDistanceMin)
                continue;
            
            currentPlacingBuilding.transform.position = hit.point + Vector3.up;
            
            
        }
    }
}
