using System;
using System.Collections;
using System.Collections.Generic;
using MrPink;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

public class IslandSpawner : MonoBehaviour
{
    [SerializeField] private List<AssetReference> islandPrefabs;

    private void Start()
    {
        StartCoroutine(LeadPlayer());
    }

    IEnumerator LeadPlayer()
    {
        while (Game._instance == null || Game.LocalPlayer == null)
        {
            yield return null;
        }
        // spawn target point in 1km around player
        yield return null;
        // then spawn another one
        var playerPos = Game.LocalPlayer.transform.position;
        var circle = Random.insideUnitCircle;
        AddressableSpawner.Instance.Spawn(islandPrefabs[Random.Range(0, islandPrefabs.Count)], new Vector3(playerPos.x, 0, playerPos.z) + new Vector3(circle.x, 0, circle.y) * 500);
    }
}
