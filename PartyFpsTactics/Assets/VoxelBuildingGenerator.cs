using System;
using System.Collections;
using System.Collections.Generic;
using Fraktalia.VoxelGen;
using Sirenix.OdinInspector;
using UnityEngine;

public class VoxelBuildingGenerator : MonoBehaviour
{
    [SerializeField] private VoxelBuildingFloor floorPrefab;
    [SerializeField] private List<VoxelBuildingFloor> _floors;
    [SerializeField] private VoxelGenerator _voxelGenerator;

    private void Start()
    {
        Generate();
    }

    [Button]
    public void Generate()
    {
        foreach (var floor in _floors)
        {
            floor.CutVoxels();
        }
    }

    [Button]
    public void Clear()
    {
        _voxelGenerator.CleanUp();
    }
}
