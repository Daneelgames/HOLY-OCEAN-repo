using System;
using System.Collections;
using System.Collections.Generic;
using Fraktalia.VoxelGen;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class VoxelBuildingGenerator : MonoBehaviour
{
    [SerializeField]private int currentSeed;
    public static VoxelBuildingGenerator Instance;
    [SerializeField] private int floorsAmount = 5;
    [SerializeField] private VoxelBuildingFloor floorPrefab;
    [SerializeField] private float randomAngleMax;
    [SerializeField] private List<VoxelBuildingFloor> _floors;
    public List<VoxelBuildingFloor> Floors => _floors;
    [SerializeField] private VoxelGenerator _voxelGenerator;

    private void Awake()
    {
        Instance = this;
    }

    public void SaveRandomSeedOnEachClient(int seed, List<VoxelBuildingFloor.VoxelFloorRandomSettings> voxelFloorRandomSettings)
    {
        currentSeed = seed;
        StartCoroutine(StartGenerating(voxelFloorRandomSettings));
    }
    
    private IEnumerator StartGenerating(List<VoxelBuildingFloor.VoxelFloorRandomSettings> voxelFloorRandomSettings)
    {
        Debug.Log("StartGenerating voxelFloorRandomSettings " + voxelFloorRandomSettings.Count);
        SpawnFloors(voxelFloorRandomSettings);
        yield return null;
        Generate();
        yield return null;
        yield return StartCoroutine(ContentPlacer.Instance.SpawnPropsInVoxelBuilding(_floors));
        yield return StartCoroutine(ContentPlacer.Instance.SpawnEnemiesInVoxelBuilding(_floors));
    }
    
    public List<VoxelBuildingFloor.VoxelFloorRandomSettings> RandomizeSettingsOnHost()
    {
        List<VoxelBuildingFloor.VoxelFloorRandomSettings> newFloorsRandomSettings = new List<VoxelBuildingFloor.VoxelFloorRandomSettings>();
        for (int i = 0; i < floorsAmount; i++)
        {
            var newRandomSettings = new VoxelBuildingFloor.VoxelFloorRandomSettings();
            newRandomSettings.floorHeight = Random.Range(3, 20);
            newRandomSettings.floorSizeX = Random.Range(10, 50);
            newRandomSettings.floorSizeZ = Random.Range(10, 50);
            newRandomSettings.innerWallsAmountX = Random.Range(1, 5);
            newRandomSettings.innerWallsAmountZ = Random.Range(1, 5);
            newRandomSettings.holesAmountF = Random.Range(1, 10);
            newRandomSettings.holesAmountR = Random.Range(1, 10);
            newRandomSettings.holesAmountB = Random.Range(1, 10);
            newRandomSettings.holesAmountL = Random.Range(1, 10);   
            
            newFloorsRandomSettings.Add(newRandomSettings);
        }
        
        return newFloorsRandomSettings;
    }
    
    [Button]
    public void SpawnFloors(List<VoxelBuildingFloor.VoxelFloorRandomSettings> voxelFloorsRandomSettingsList)
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
            newFloor.SetSettings(voxelFloorsRandomSettingsList[i]);
            _floors.Add(newFloor);
            
            spawnPos = newFloor.transform.position + newFloor.transform.up * newFloor.GetHeight;
            Random.InitState(currentSeed);
            float x = Random.Range(-randomAngleMax * i, randomAngleMax * i);
            Random.InitState(currentSeed);
            float y = Random.Range(-randomAngleMax * i, randomAngleMax * i);
            Random.InitState(currentSeed);
            float z = Random.Range(-randomAngleMax * i, randomAngleMax * i);
            
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
