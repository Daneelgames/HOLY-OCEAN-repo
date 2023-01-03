using System;
using System.Collections;
using System.Collections.Generic;
using Fraktalia.VoxelGen;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class VoxelBuildingGenerator : MonoBehaviour
{
    [SerializeField] private int floorsAmount = 5;
    [SerializeField] private VoxelBuildingFloor floorPrefab;
    [SerializeField] private List<VoxelBuildingFloor> _floors;
    [SerializeField] private VoxelGenerator _voxelGenerator;

    private IEnumerator Start()
    {
        SpawnFloors();
        yield return null;
        Generate();
    }

    [Button]
    public void SpawnFloors()
    {
        foreach (var voxelBuildingFloor in _floors)
        {
            if (Application.isPlaying || Application.isEditor)
                DestroyImmediate(voxelBuildingFloor.gameObject);
            else
                Destroy(voxelBuildingFloor.gameObject);
        }

        _floors.Clear();
        
        Vector3 spawnPos = transform.position;
        Vector3 spawnRot = transform.eulerAngles;
        for (int i = 0; i < floorsAmount; i++)
        {
            var newFloor = Instantiate(floorPrefab);
            newFloor.transform.eulerAngles = spawnRot;
            newFloor.transform.position = spawnPos;
            newFloor.transform.parent = transform;
            newFloor.RandomizeSettings();
            _floors.Add(newFloor);
            
            spawnPos = newFloor.transform.position + newFloor.transform.up * newFloor.GetHeight;
            spawnRot += new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f));
        }
    }

    [Button]
    public void Generate()
    {
        foreach (var floor in _floors)
        {
            floor.CutVoxels(_voxelGenerator);
        }
    }

    [Button]
    public void Clear()
    {
        _voxelGenerator.CleanUp();
    }
}
