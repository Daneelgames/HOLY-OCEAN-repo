using System.Collections;
using System.Collections.Generic;
using MrPink;
using Unity.AI.Navigation;
using UnityEngine;

public class NavMeshSurfaceAutoUpdate : MonoBehaviour
{
    [SerializeField] private NavMeshSurface _navMeshSurface;
    [SerializeField] private float updateCooldown = 1;
    void Start()
    {
        StartCoroutine(UpdateNavmesh());
    }


    IEnumerator UpdateNavmesh()
    {
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
