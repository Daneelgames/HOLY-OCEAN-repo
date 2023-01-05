using System;
using System.Collections;
using System.Collections.Generic;
using MrPink;
using MrPink.PlayerSystem;
using Unity.AI.Navigation;
using UnityEditor.AI;
using UnityEngine;
using UnityEngine.AI;
using NavMeshBuilder = UnityEngine.AI.NavMeshBuilder;

public class NavMeshSurfaceUpdate : MonoBehaviour
{
    [SerializeField]
    private NavMeshSurface Surface;
    [SerializeField]
    private float UpdateRate = 0.1f;
    [SerializeField]
    private float MovementThreshold = 3f;
    [SerializeField]
    private Vector3 NavMeshSize = new Vector3(20, 20, 20);

    private Vector3 WorldAnchor;
    private NavMeshData NavMeshData;
    private List<NavMeshBuildSource> Sources = new List<NavMeshBuildSource>();

    private void Start()
    {
        NavMeshData = new NavMeshData();
        NavMesh.AddNavMeshData(NavMeshData);
        //BuildNavMesh(false);
        StartCoroutine(CheckPlayerMovement());
    }

    private IEnumerator CheckPlayerMovement()
    {
        while (Game._instance == null || Game.LocalPlayer == null)
        {
            yield return null;
        }
        WaitForSeconds Wait = new WaitForSeconds(UpdateRate);

        while (true)
        {
            if (Vector3.Distance(WorldAnchor, Game.LocalPlayer.transform.position) > MovementThreshold)
            {
                BuildNavMesh(true);
                WorldAnchor = Game.LocalPlayer.transform.position;
            }

            yield return Wait;
        }
    }

    private void BuildNavMesh(bool Async)
    {
        Bounds navMeshBounds = new Bounds(Game.LocalPlayer.transform.position, NavMeshSize);
        List<NavMeshBuildMarkup> markups = new List<NavMeshBuildMarkup>();

        List<NavMeshModifier> modifiers;
        if (Surface.collectObjects == CollectObjects.Children)
        {
            modifiers = new List<NavMeshModifier>(GetComponentsInChildren<NavMeshModifier>());
        }
        else
        {
            modifiers = NavMeshModifier.activeModifiers;
        }

        for (int i = 0; i < modifiers.Count; i++)
        {
            if (((Surface.layerMask & (1 << modifiers[i].gameObject.layer)) != 0)
                && modifiers[i].AffectsAgentType(Surface.agentTypeID))
            {
                markups.Add(new NavMeshBuildMarkup()
                {
                    root = modifiers[i].transform,
                    overrideArea = modifiers[i].overrideArea,
                    area = modifiers[i].area,
                    ignoreFromBuild = modifiers[i].ignoreFromBuild
                });
            }
        }

        if (Surface.collectObjects == CollectObjects.Children)
        {
            NavMeshBuilder.CollectSources(transform, Surface.layerMask, Surface.useGeometry, Surface.defaultArea, markups, Sources);
        }
        else
        {
            NavMeshBuilder.CollectSources(navMeshBounds, Surface.layerMask, Surface.useGeometry, Surface.defaultArea, markups, Sources);
        }

        // Ensure units are not on bakeble solid layer 
        //Sources.RemoveAll(source => source.component != null && source.component.gameObject.GetComponent<NavMeshAgent>() != null);

        if (Async)
        {
            NavMeshBuilder.UpdateNavMeshDataAsync(NavMeshData, Surface.GetBuildSettings(), Sources, new Bounds(Game.LocalPlayer.transform.position, NavMeshSize));
        }
        else
        {
            NavMeshBuilder.UpdateNavMeshData(NavMeshData, Surface.GetBuildSettings(), Sources, new Bounds(Game.LocalPlayer.transform.position, NavMeshSize));
        }
    }
}