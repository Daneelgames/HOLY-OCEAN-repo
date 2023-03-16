using System;
using System.Collections;
using System.Collections.Generic;
using _src.Scripts.Data;
using MrPink.Health;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "ScriptableObjects/BuildingData", order = 1)]
public class BuildingData : ScriptableObject
{
    public List<BuildingData> BuildingsToUnlock;
    public GameObject BuildingPrefab;
    public List<ResourceNeed> Recipe;

    [Serializable]
    public class ResourceNeed
    {
        public BuildingResources.Resource Resource;
        public int Amount;
    }
}