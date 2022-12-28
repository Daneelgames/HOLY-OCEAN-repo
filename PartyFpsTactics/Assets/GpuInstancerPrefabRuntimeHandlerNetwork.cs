using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using GPUInstancer;
using Sirenix.OdinInspector;
using UnityEngine;

public class GpuInstancerPrefabRuntimeHandlerNetwork : NetworkBehaviour
{
    [SerializeField] private GPUInstancerPrefab _gpuInstancerPrefab;
    [SerializeField] private GPUInstancerPrefabRuntimeHandler _gpuInstancerPrefabRuntimeHandler;

    [Button]
    void GetRuntimeHandler()
    {
        _gpuInstancerPrefab = gameObject.GetComponent<GPUInstancerPrefab>();
        _gpuInstancerPrefabRuntimeHandler = gameObject.GetComponent<GPUInstancerPrefabRuntimeHandler>();
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
        _gpuInstancerPrefab.enabled = false;
        _gpuInstancerPrefabRuntimeHandler.enabled = false;
        yield return null;
        
        _gpuInstancerPrefab.enabled = true;
        _gpuInstancerPrefabRuntimeHandler.enabled = true;
        _gpuInstancerPrefabRuntimeHandler.InitOnClient();
    }
}
