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
    [SerializeField] private int floorsAmount = 5;
    [SerializeField] private VoxelBuildingFloor floorPrefab;
    [SerializeField] private float randomAngleMax;
    [SerializeField] private List<VoxelBuildingFloor> _floors;
    public List<VoxelBuildingFloor> Floors => _floors;
    [SerializeField] private VoxelGenerator _voxelGenerator;
    [SerializeField] private Transform firstFloorTransform;
    public void SaveRandomSeedOnEachClient(int seed, List<VoxelFloorSettingsRaw> voxelFloorRandomSettings)
    {
        currentSeed = seed;
        StartCoroutine(StartGenerating(voxelFloorRandomSettings));
    }
    
    private IEnumerator StartGenerating(List<VoxelFloorSettingsRaw> voxelFloorRandomSettings)
    {
        Debug.Log("StartGenerating voxelFloorRandomSettings " + voxelFloorRandomSettings.Count);
        SpawnFloors(voxelFloorRandomSettings);
        yield return null;
        Generate();
        yield break;
        yield return null;
    }

    [Serializable]
    public class VoxelFloorSettingsRaw
    {
        [SerializeField]
        public List<int> settings;
    }
    
    public List<VoxelFloorSettingsRaw> RandomizeSettingsOnHost()
    {
        List<VoxelFloorSettingsRaw> newFloorsRandomSettings = new List<VoxelFloorSettingsRaw>();
        for (int i = 0; i < floorsAmount; i++)
        {
            var newRandomSettings = new VoxelFloorSettingsRaw
            {
                settings = new List<int>(9)
                {
                    [0] = Random.Range(3, 20),
                    [1] = Random.Range(10, 50),
                    [2] = Random.Range(10, 50),
                    [3] = Random.Range(1, 5),
                    [4] = Random.Range(1, 5),
                    [5] = Random.Range(1, 10),
                    [6] = Random.Range(1, 10),
                    [7] = Random.Range(1, 10),
                    [8] = Random.Range(1, 10)
                }
            };

            newFloorsRandomSettings.Add(newRandomSettings);
        }
        
        return newFloorsRandomSettings;
    }

    [Button]
    public void SpawnFloorsEditor()
    {
        SpawnFloors(RandomizeSettingsOnHost());
    }
    public void SpawnFloors(List<VoxelFloorSettingsRaw> voxelFloorsRandomSettingsList)
    {
        foreach (var voxelBuildingFloor in _floors)
        {
            if (Application.isPlaying == false && Application.isEditor)
                DestroyImmediate(voxelBuildingFloor.gameObject);
            else
                Destroy(voxelBuildingFloor.gameObject);
        }

        _floors.Clear();
        
        Vector3 spawnPos = firstFloorTransform ? firstFloorTransform.position : transform.position;
        Vector3 spawnRot = firstFloorTransform ? firstFloorTransform.eulerAngles : transform.eulerAngles;
        for (int i = 0; i < floorsAmount; i++)
        {
            var newFloor = Instantiate(floorPrefab);
            newFloor.transform.eulerAngles = spawnRot;
            newFloor.transform.position = spawnPos;
            newFloor.transform.parent = transform;
            newFloor.SetSettings(voxelFloorsRandomSettingsList[i].settings);
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
