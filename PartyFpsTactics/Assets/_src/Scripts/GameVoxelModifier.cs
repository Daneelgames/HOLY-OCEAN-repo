using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using Fraktalia.VoxelGen;
using Fraktalia.VoxelGen.Modify;
using Fraktalia.VoxelGen.SaveSystem.Modules;
using MrPink.Health;
using UnityEngine;
using UnityEngine.Events;

public class GameVoxelModifier : NetworkBehaviour
{
    public static GameVoxelModifier Instance;
    
    [SerializeField] List<VoxelModifier> modifiers = new List<VoxelModifier>();

    public override void OnStartClient() { 
        base.OnStartClient();
        // Your code here..
        
        Instance = this;

        if (IsServer)
        {
            //_voxelSaveSystem.OnDataSaved.AddListener(OnDataSaved);
                
            // on spawn, client asks server for saved voxel data
            ServerManager.OnRemoteConnectionState += CheckIfNewPlayerConnected;
        }
    }

    [Server]
    void CheckIfNewPlayerConnected(NetworkConnection networkConnection, RemoteConnectionStateArgs remoteConnectionStateArgs)
    {
        if (networkConnection == null)
        {
            Debug.Log("SOMEONE TRIED TO CONNET null connection");
            return;
        }
        if (remoteConnectionStateArgs.ConnectionState == RemoteConnectionState.Stopped)
        {
            Debug.Log("SOMEONE STOPPED CONNECTION connection stopped");
            return;
        }
        if (remoteConnectionStateArgs.ConnectionState == RemoteConnectionState.Started)
        {
            if (modifiers.Count > 0)
            {
                networkConnection.Disconnect(true);
                Debug.Log("SOMEONE STARTED CONNECTION DURING THE GAME connection stopped");
                return;
            }
        }
    }

    public void TileDestroyedInWorld(Vector3 tilePos)
    {
        if (base.IsServer)
        {
            RpcTileDestroyedInWorldClient(tilePos);
        }
        else
        {
            RpcTileDestroyedInWorldServer(tilePos);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void RpcTileDestroyedInWorldServer(Vector3 tilePos)
    {
        RpcTileDestroyedInWorldClient(tilePos);
    }

    private Collider[] tileDestroyedInWorld = new Collider[1];
    
    [SerializeField] private LayerMask tilesLayerMask;
    
    [ObserversRpc(IncludeOwner = false)]
    void RpcTileDestroyedInWorldClient(Vector3 tilePos)
    {
        tileDestroyedInWorld[0] = null;
        Physics.OverlapSphereNonAlloc(tilePos, 0.25f, tileDestroyedInWorld, tilesLayerMask);
        
        if (tileDestroyedInWorld[0] == null)
            return;

        var tileHealth = tileDestroyedInWorld[0].gameObject.GetComponent<TileHealth>();
        
        if (tileHealth == null || tileHealth.IsDead)
            return;
            
        tileHealth.Death(DamageSource.Environment, true, true, false);
    }
    
    public void DestructionInWorld(Vector3 pos)
    {
        if (IsServer || IsHost)
        {
            // tell other clients
            RpcModifyLocally(pos);
        }
        else
        {
            // tell server to tell other clients
            RpcModifyOnServer(pos);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void RpcModifyOnServer(Vector3 pos)
    {
        RpcModifyLocally(pos);
    }

    [ObserversRpc]
    void RpcModifyLocally(Vector3 pos)
    {
        foreach (var modifier in modifiers)
        {
            var voxelGen = modifier.ReferenceGenerator;
            var voxelGenCenterWorldPos = voxelGen.transform.position + (Vector3.one * voxelGen.RootSize) / 2;
            
            if (Vector3.Distance(pos, voxelGenCenterWorldPos) > voxelGen.RootSize)
                continue;
            
            modifier.ModifyAtPos(pos);
        }
    }

    public void AddIslandModifier(VoxelModifier modifier)
    {
        if (modifiers.Contains(modifier))
            return;
        
        modifiers.Add(modifier);
    }
    public void RemoveIslandModifier(VoxelModifier modifier)
    {
        if (modifiers.Contains(modifier) == false)
            return;
        
        modifiers.Remove(modifier);
    }
}
