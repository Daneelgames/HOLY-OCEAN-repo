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
    public static Sprite GetResourceSprite(Resource resource) => Instance.GetSpriteByResource(resource);
    
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

    public Sprite GetSpriteByResource(Resource resource)
    {
        foreach (var buildingResource in _playerBuildingResources)
        {
            if (buildingResource.resource != resource)
                continue;
            return buildingResource.resourceSprite;
        }

        return null;
    }
    
    public void AddResource(Resource rsc, int amount)
    {
        foreach (var buildingResource in _playerBuildingResources)
        {
            if (buildingResource.resource == rsc)
            {
                buildingResource.amount = buildingResource.amount + amount;
                buildingResource.resourcePlayerUi.SetAmount(buildingResource.amount);
                buildingResource.resourcePlayerUi.UpdateResource();
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
                buildingResource.resourcePlayerUi.SetAmount(buildingResource.amount);
                buildingResource.resourcePlayerUi.UpdateResource();
                return;
            }
        }
    }

    public void EnterBuildingMode()
    {
        foreach (var buildingResource in _playerBuildingResources)
        {
            buildingResource.resourcePlayerUi.Show();
        }
    }
    public void ExitBuildingMode()
    {
        foreach (var buildingResource in _playerBuildingResources)
        {
            buildingResource.resourcePlayerUi.Hide();
        }
    }
}