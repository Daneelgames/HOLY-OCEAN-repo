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
            if (Modifier != null)
            {
                networkConnection.Disconnect(true);
                Debug.Log("SOMEONE STARTED CONNECTION DURING THE GAME connection stopped");
                return;
            }
        }
        
        return;
        
        if (_voxelSaveSystem == null)
            return;
        
        Debug.Log("VOXEL new connection");
        // new client connected 
        newPlayers.Add(networkConnection);
        
        _voxelSaveSystem.Save();
    }

    /*
    [Server]
    void OnDataSaved()
    {
        Debug.Log("VOXEL OnDataSaved. NewPlayers: " + newPlayers.Count);
        var voxelData = SaveModule_ByteBuffer_V2.VoxelDictionary[_voxelSaveSystem.ModuleByteBuffer_V2.Key];
        foreach (var networkConnection in newPlayers)
        {
            if (networkConnection == null || networkConnection.IsValid == false)
            {
                Debug.Log("VOXEL null or invalid connection");
                continue;
            }
            
            Debug.Log("VOXEL send target rpc. voxelData length is " + voxelData.Length);
            RpcSendVoxelDataToClient(networkConnection, voxelData);
        }
        newPlayers.Clear();
    }

    [TargetRpc]
    void RpcSendVoxelDataToClient(NetworkConnection networkConnection, byte[] data)
    {
        if (SaveModule_ByteBuffer_V2.VoxelDictionary.ContainsKey(_voxelSaveSystem.ModuleByteBuffer_V2.Key))
        {
            Debug.Log("VOXEL contains key");
            SaveModule_ByteBuffer_V2.VoxelDictionary[_voxelSaveSystem.ModuleByteBuffer_V2.Key] = data;
        }
        else
        {
            SaveModule_ByteBuffer_V2.VoxelDictionary.Add(_voxelSaveSystem.ModuleByteBuffer_V2.Key, data);
            
            Debug.Log("VOXEL add new key");
        }
        _voxelSaveSystem.Load();
    }
    */

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
        if (Modifier)
        {
            //Debug.Log("MODIFIER MODIFY");
            Modifier.ModifyAtPos(pos);
        }
    }
}
