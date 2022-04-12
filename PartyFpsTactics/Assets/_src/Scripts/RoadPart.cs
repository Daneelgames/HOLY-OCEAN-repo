using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using UnityEngine;

public class RoadPart : MonoBehaviour
{
    public List<Transform> raycastTransforms;
    public Transform roadStart;
    public List<Transform> roadEnds;
    public GameObject visualGo;

    public List<BoxCollider> collidersToCheck;

    public void Init()
    {
        visualGo.SetActive(false);
        for (int i = 0; i < collidersToCheck.Count; i++)
        {
            RaycastHit[] hit = { };
            Physics.BoxCastNonAlloc(collidersToCheck[i].transform.position, collidersToCheck[i].size, collidersToCheck[i].transform.forward, hit, collidersToCheck[i].transform.rotation, 0.5f, GameManager.Instance.AllSolidsMask);
            
            for (int j = hit.Length - 1; j >= 0; j--)
            {
                var tile = hit[j].collider.gameObject.GetComponent<TileHealth>();
                if (tile)
                {
                    tile.Kill(DamageSource.Environment);
                }
                else
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }

        for (int i = collidersToCheck.Count - 1; i >= 0; i--)
        {
            Destroy(collidersToCheck[i].gameObject);  
        }
        
        collidersToCheck.Clear();
        visualGo.SetActive(true);
    }

    private void OnDestroy()
    {
        RoadGenerator.Instance.RemoveFromSpawnedParts(this);
    }
}
