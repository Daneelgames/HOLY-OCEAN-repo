using System.Collections;
using System.Collections.Generic;
using MrPink;
using Unity.AI.Navigation;
using UnityEditor.AI;
using UnityEngine;
using NavMeshBuilder = UnityEngine.AI.NavMeshBuilder;

public class NavMeshSurfaceAutoUpdate : MonoBehaviour
{
    [SerializeField] private NavMeshSurface _navMeshSurface;
    [SerializeField] private float delay = 10;
    [SerializeField] private float updateCooldown = 1;
    void Start()
    {
        StartCoroutine(UpdateNavmesh());
    }


    IEnumerator UpdateNavmesh()
    {
        yield return new WaitForSeconds(delay);
        while (Game._instance == null || Game.LocalPlayer == null)
        {
            yield return null;
        }
        
        // DONT RUN ON CLIENT
        if (Game.LocalPlayer.IsHost == false)
        {
            Debug.LogWarning("RUN UPDATE NAVMESH ONLY ON HOST");
            yield break;
        }
        
        _navMeshSurface.BuildNavMesh();
        
        while (true)
        {
            yield return new WaitForSeconds(updateCooldown);
            if (_navMeshSurface.navMeshData == null)
            {
                Debug.Log("BUILD NAV MESH");
                _navMeshSurface.BuildNavMesh();
                continue;
            }
            
            _navMeshSurface.UpdateNavMesh(_navMeshSurface.navMeshData);
        }
    }
}
