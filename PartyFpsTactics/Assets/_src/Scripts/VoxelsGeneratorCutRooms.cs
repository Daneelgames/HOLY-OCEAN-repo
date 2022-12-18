using System;
using System.Collections;
using System.Collections.Generic;
using Fraktalia.VoxelGen.Modify.Procedural;
using UnityEngine;
using Random = UnityEngine.Random;

public class VoxelsGeneratorCutRooms : MonoBehaviour
{
    [SerializeField] private Transform box;
    [SerializeField] private ColliderToVoxel _colliderToVoxel;

    private bool cuted = false;
    public void Cut()
    {
        if (box == null || box.gameObject.activeInHierarchy == false) return;
        if (cuted)
            return;

        cuted = true;
        box.transform.localPosition = Vector3.up * Random.Range(1, 20);
        box.transform.localScale = new Vector3(Random.Range(5, 20), Random.Range(5, 20), Random.Range(5, 20));
        _colliderToVoxel.ApplyProceduralModifier(true);
    }
}
