using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using Fraktalia.VoxelGen.Modify;
using UnityEngine;

public class GameVoxelModifier : NetworkBehaviour
{
    public static GameVoxelModifier Instance;
    
    [SerializeField] VoxelModifier Modifier;

    public override void OnStartClient() { 
        base.OnStartClient();
        // Your code here..
        
        Instance = this;
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
            RpcModifyOnServer(pos);
        }
    }

    [ServerRpc]
    void RpcModifyOnServer(Vector3 pos)
    {
        RpcModifyLocally(pos);
    }

    [ObserversRpc]
    void RpcModifyLocally(Vector3 pos)
    {
        Modifier.ModifyAtPos(pos);
    }
}
