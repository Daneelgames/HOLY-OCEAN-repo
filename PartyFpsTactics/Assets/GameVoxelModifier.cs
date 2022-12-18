using System;
using System.Collections;
using System.Collections.Generic;
using Fraktalia.VoxelGen.Modify;
using UnityEngine;

public class GameVoxelModifier : MonoBehaviour
{
    public static GameVoxelModifier Instance;
    
    [SerializeField] VoxelModifier Modifier;

    private void Awake()
    {
        Instance = this;
    }

    public void DestructionInWorld(Vector3 pos)
    {
        Modifier.ModifyAtPos(pos);
    }
}
