using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class BuildingResources : MonoBehaviour
{
    public static BuildingResources Instance;
    public enum Resource
    {
        Bones, Blood, Skin, Meat, Metal
    }

    [Serializable]
    private class PlayerBuildingResource
    {
        public Resource resource;
        public Sprite resourceSprite;
        public ResourcePlayerUi resourcePlayerUi;
        public int amount;
    }

    [SerializeField] private List<PlayerBuildingResource> _playerBuildingResources = new List<PlayerBuildingResource>();

    private void Awake()
    {
        Instance = this;

        foreach (var buildingResource in _playerBuildingResources)
        {
            buildingResource.resourcePlayerUi.SetResourceIcon(buildingResource.resourceSprite);
        }
    }

    public void AddResource(Resource rsc, int amount)
    {
        foreach (var buildingResource in _playerBuildingResources)
        {
            if (buildingResource.resource == rsc)
            {
                buildingResource.amount += amount;
                return;
            }
        }
        
        /*
        var newResource = new PlayerBuildingResource();
        newResource.resource = rsc;
        newResource.amount = amount;
        _playerBuildingResources.Add(newResource);*/
    }
    public void RemoveResource(Resource rsc, int amount)
    {
        foreach (var buildingResource in _playerBuildingResources)
        {
            if (buildingResource.resource == rsc)
            {
                buildingResource.amount = Mathf.Clamp(buildingResource.amount - amount, 0, buildingResource.amount);
                return;
            }
        }
    }
}