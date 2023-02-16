using System.Collections;
using System.Collections.Generic;
using Fraktalia.VoxelGen.Modify.Procedural;
using Sirenix.OdinInspector;
using UnityEngine;

public class ColliderToVoxelManualBaker : MonoBehaviour
{
    public ColliderToVoxel _colliderToVoxel;
    
    
    [BoxGroup("BAKING")] [SerializeField] [ReadOnly] private int lastPieceIndex = 0;
		
    [BoxGroup("BAKING")] [Button]
    public void HideAllColliders()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        lastPieceIndex = 0;
    }

    [BoxGroup("BAKING")] [Button]
    public void Bake(int amount, bool cut = false)
    {
        int t = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            var collider = child.gameObject.GetComponent<Collider>();
            if (collider == null)
            {
                Debug.LogError("PUT COLLIDERS RIGHT UNDER THIS PARENT");
                return;
            }
            if (i < lastPieceIndex)
            {
                child.gameObject.SetActive(false);
                continue;
            }
				
            child.gameObject.SetActive(true);
            t++;
            lastPieceIndex++;
            if (t >= amount)
                break;
        }
        _colliderToVoxel.GetActiveCollidersAndApply(cut);
    }

    [BoxGroup("BAKING")][Button]
    public void Clear()
    {
        _colliderToVoxel.TargetGenerator.CleanUp(); 
    }
}
