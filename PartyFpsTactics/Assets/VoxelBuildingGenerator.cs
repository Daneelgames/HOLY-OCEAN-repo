using System;
using System.Collections;
using System.Collections.Generic;
using Fraktalia.VoxelGen;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class VoxelBuildingGenerator : MonoBehaviour
{
    [SerializeField][ReadOnly]private int currentSeed;
    public static VoxelBuildingGenerator Instance;
    [SerializeField] private int floorsAmount = 5;
    [SerializeField] private VoxelBuildingFloor floorPrefab;
    [SerializeField] private List<VoxelBuildingFloor> _floors;
    public List<VoxelBuildingFloor> Floors => _floors;
    [SerializeField] private VoxelGenerator _voxelGenerator;

    private void Awake()
    {
        Instance = this;
    }

    public void SaveRandomSeedOnEachClient(int seed)
    {
        currentSeed = seed;
        StartCoroutine(StartGenerating());
    }
    
    private IEnumerator StartGenerating()
    {
        SpawnFloors();
        yield return null;
        Generate();
        yield return null;
        StartCoroutine(ContentPlacer.Instance.SpawnEnemiesInVoxelBuilding(_floors));
    }
    
    [Button]
    public void SpawnFloors()
    {
        foreach (var voxelBuildingFloor in _floors)
        {
            if (Application.isPlaying == false && Application.isEditor)
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
            newFloor.RandomizeSettings(i, currentSeed);
            _floors.Add(newFloor);
            
            spawnPos = newFloor.transform.position + newFloor.transform.up * newFloor.GetHeight;
            Random.InitState(currentSeed);
            float x = Random.Range(-5f * i, 5f * i);
            Random.InitState(currentSeed);
            float y = Random.Range(-5f * i, 5f * i);
            Random.InitState(currentSeed);
            float z = Random.Range(-5f * i, 5f * i);
            
            spawnRot += new Vector3(x, y, z);
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
