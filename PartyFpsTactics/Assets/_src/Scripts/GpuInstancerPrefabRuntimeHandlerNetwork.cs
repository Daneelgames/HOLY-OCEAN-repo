using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using GPUInstancer;
using Sirenix.OdinInspector;
using UnityEngine;

public class GpuInstancerPrefabRuntimeHandlerNetwork : NetworkBehaviour
{
    [SerializeField] private List<GpuInstancerPrefabAndHandler> _gpuInstancerPrefabAndHandlers = new List<GpuInstancerPrefabAndHandler>();

    [Serializable]
    class GpuInstancerPrefabAndHandler
    {
        public GPUInstancerPrefab _gpuInstancerPrefab;
        public GPUInstancerPrefabRuntimeHandler _gpuInstancerPrefabRuntimeHandler; 
    }
    [Button]
    void GetRuntimeHandlers()
    {
        var _gpuInstancerPrefabs = gameObject.GetComponentsInChildren<GPUInstancerPrefab>();
        var _gpuInstancerPrefabRuntimeHandlers = gameObject.GetComponentsInChildren<GPUInstancerPrefabRuntimeHandler>();

        for (int i = 0; i < _gpuInstancerPrefabs.Length; i++)
        {
            GpuInstancerPrefabAndHandler newPrefab = new GpuInstancerPrefabAndHandler();
            newPrefab._gpuInstancerPrefab = _gpuInstancerPrefabs[i];
            newPrefab._gpuInstancerPrefabRuntimeHandler = _gpuInstancerPrefabRuntimeHandlers[i];
            _gpuInstancerPrefabAndHandlers.Add(newPrefab);
        }
    }
    public override void OnStartClient()
    {
        base.OnStartClient();

        Init();
    }

    void Init()
    {
        StartCoroutine(InitCoroutine());
    }

    IEnumerator InitCoroutine()
    {
        foreach (var gpuInstancerPrefabAndHandler in _gpuInstancerPrefabAndHandlers)
        {
            gpuInstancerPrefabAndHandler._gpuInstancerPrefab.enabled = false;
            gpuInstancerPrefabAndHandler._gpuInstancerPrefabRuntimeHandler.enabled = false;
        }
        yield return null;
        
        foreach (var gpuInstancerPrefabAndHandler in _gpuInstancerPrefabAndHandlers)
        {
            gpuInstancerPrefabAndHandler._gpuInstancerPrefab.enabled = true;
            gpuInstancerPrefabAndHandler._gpuInstancerPrefabRuntimeHandler.enabled = true;
        }
    }
}
