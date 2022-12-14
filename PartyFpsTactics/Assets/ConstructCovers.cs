using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using Sirenix.OdinInspector;
using UnityEngine;

public class ConstructCovers : MonoBehaviour
{
    [SerializeField] private List<TileHealth> tilesList;
    [SerializeField] private Vector2 posYMinMax = new Vector2(2.5f, 3.5f);

    [Button]
    public void GetTiles()
    {
        var tiles = GetComponentsInChildren<TileHealth>();
        tilesList = new List<TileHealth>();
        foreach (var tileHealth in tiles)
        {
            tilesList.Add(tileHealth);
        }
    }
    [Button]
    public void RemoveTilesOutOfBounds()
    {
        for (int i = tilesList.Count - 1; i >= 0; i--)
        {
            if (tilesList[i].transform.position.y < posYMinMax.x || tilesList[i].transform.position.y > posYMinMax.y)
            {
                tilesList.RemoveAt(i);
            }
        }
    }

    [Button]
    public void MakeCovers()
    {
        foreach (var tileHealth in tilesList)
        {
            var newCover = tileHealth.gameObject.GetComponent<Cover>();
            if (!newCover)
                newCover = tileHealth.gameObject.AddComponent<Cover>();
            newCover.ConstructSpots();
        }
    }
}
