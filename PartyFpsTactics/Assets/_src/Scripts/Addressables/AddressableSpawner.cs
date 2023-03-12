using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Random = UnityEngine.Random;

public class AddressableSpawner : MonoBehaviour
{
    public static AddressableSpawner Instance;

    private void Awake()
    {
        Instance = this;
    }

    private readonly Dictionary<AssetReference, List<GameObject>> _spawnedParticleSystems = 
        new Dictionary<AssetReference, List<GameObject>>();
    
    /// The Queue holds requests to spawn an instanced that were made while we are already loading the asset
    /// They are spawned once the addressable is loaded, in the order requested
    private readonly Dictionary<AssetReference, Queue<Vector3>> _queuedSpawnRequests = 
        new Dictionary<AssetReference, Queue<Vector3>>();
    
    private readonly Dictionary<AssetReference, AsyncOperationHandle<GameObject>> _asyncOperationHandles = 
        new Dictionary<AssetReference, AsyncOperationHandle<GameObject>>();

    public void Spawn(AssetReference assetReference, Vector3 pos)
    {
        if (assetReference.RuntimeKeyIsValid() == false)
        {
            Debug.Log("Invalid Key " + assetReference.RuntimeKey.ToString());
            return;
        }

        if (_asyncOperationHandles.ContainsKey(assetReference))
        {
            if (_asyncOperationHandles[assetReference].IsDone)
                SpawnFromLoadedReference(assetReference, pos);
            else
                EnqueueSpawnForAfterInitialization(assetReference, pos);
            
            return;
        }

        LoadAndSpawn(assetReference, pos);
    }

    private void LoadAndSpawn(AssetReference assetReference, Vector3 pos)
    {
        var op = Addressables.LoadAssetAsync<GameObject>(assetReference);
        _asyncOperationHandles[assetReference] = op;
        op.Completed += (operation) =>
        {
            SpawnFromLoadedReference(assetReference, pos);
            if (_queuedSpawnRequests.ContainsKey(assetReference))
            {
                while (_queuedSpawnRequests[assetReference]?.Any() == true)
                {
                    var position = _queuedSpawnRequests[assetReference].Dequeue();
                    SpawnFromLoadedReference(assetReference, position);
                }
            }
        };
    }

    private void EnqueueSpawnForAfterInitialization(AssetReference assetReference, Vector3 pos)
    {
        if (_queuedSpawnRequests.ContainsKey(assetReference) == false)
            _queuedSpawnRequests[assetReference] = new Queue<Vector3>();
        _queuedSpawnRequests[assetReference].Enqueue(pos);
    }

    private void SpawnFromLoadedReference(AssetReference assetReference, Vector3 position)
    {
        assetReference.InstantiateAsync(position, Quaternion.identity).Completed += (asyncOperationHandle) =>
        {
            if (_spawnedParticleSystems.ContainsKey(assetReference) == false)
            {
                _spawnedParticleSystems[assetReference] = new List<GameObject>();
            }
            
            _spawnedParticleSystems[assetReference].Add(asyncOperationHandle.Result);
            var notify = asyncOperationHandle.Result.AddComponent<NotifyOnDestroy>();
            notify.Destroyed += Remove;
            notify.AssetReference = assetReference;
            ProceedSpawnedObject(asyncOperationHandle.Result);
        };
    }

    private void Remove(AssetReference assetReference, NotifyOnDestroy obj)
    {
        Addressables.ReleaseInstance(obj.gameObject);
        
        _spawnedParticleSystems[assetReference].Remove(obj.gameObject);
        if (_spawnedParticleSystems[assetReference].Count == 0)
        {
            Debug.Log($"Removed all {assetReference.RuntimeKey.ToString()}");
            
            if (_asyncOperationHandles[assetReference].IsValid())
                Addressables.Release(_asyncOperationHandles[assetReference]);

            _asyncOperationHandles.Remove(assetReference);
        }
    }

    void ProceedSpawnedObject(GameObject spawned)
    {
        if (spawned.TryGetComponent<Island>(out var island))
        {
            IslandSpawner.Instance.AddressableIslandInstantiated(island);
        }
    }
}