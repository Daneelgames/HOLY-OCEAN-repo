using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Transporting;
using Fraktalia.VoxelGen;
using Fraktalia.VoxelGen.Modify;
using Fraktalia.VoxelGen.SaveSystem.Modules;
using UnityEngine;
using UnityEngine.Events;

public class GameVoxelModifier : NetworkBehaviour
{
    public static GameVoxelModifier Instance;
    
    [SerializeField] VoxelModifier Modifier;
    [SerializeField] private VoxelSaveSystem _voxelSaveSystem;

    private List<NetworkConnection> newPlayers = new List<NetworkConnection>();

    private UnityAction onDataSavedOnServer;

    public override void OnStartClient() { 
        base.OnStartClient();
        // Your code here..
        
        Instance = this;

        if (IsServer)
        {
            _voxelSaveSystem.OnDataSaved.AddListener(OnDataSaved);
                
            // on spawn, client asks server for saved voxel data
            ServerManager.OnRemoteConnectionState += CheckIfNewPlayerConnected;
        }
    }

    [Server]
    void CheckIfNewPlayerConnected(NetworkConnection networkConnection, RemoteConnectionStateArgs remoteConnectionStateArgs)
    {
        if (remoteConnectionStateArgs.ConnectionState == RemoteConnectionState.Stopped)
            return;
        
        // new client connected 
        newPlayers.Add(networkConnection);
        
        _voxelSaveSystem.Save();
    }

    [Server]
    void OnDataSaved()
    {
        var voxelData = SaveModule_ByteBuffer_V2.VoxelDictionary[_voxelSaveSystem.ModuleByteBuffer_V2.Key];
        foreach (var networkConnection in newPlayers)
        {
            RpcSendVoxelDataToClient(networkConnection, voxelData);
        }
        newPlayers.Clear();
    }

    [TargetRpc]
    void RpcSendVoxelDataToClient(NetworkConnection networkConnection, byte[] data)
    {
        if (SaveModule_ByteBuffer_V2.VoxelDictionary.ContainsKey(_voxelSaveSystem.ModuleByteBuffer_V2.Key))
            SaveModule_ByteBuffer_V2.VoxelDictionary[_voxelSaveSystem.ModuleByteBuffer_V2.Key] = data;
        else
            SaveModule_ByteBuffer_V2.VoxelDictionary.Add(_voxelSaveSystem.ModuleByteBuffer_V2.Key, data);
        _voxelSaveSystem.Load();
    }

    

    public void DestructionInWorld(Vector3 pos)
    {
        if (IsServer)
        {
            // tell other clients
            RpcModifyLocally(pos);
        }
        else
        {
            // tell server to tell other clients
            //RpcModifyOnServer(pos);
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
        Debug.Log("MODIFIER MODIFY");
        Modifier.ModifyAtPos(pos);
    }
}
