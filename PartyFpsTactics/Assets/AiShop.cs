using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Tools;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class AiShop : MonoBehaviour
{
    public bool randomize = false;
    [ShowIf("randomize", true)] public List<Tool> toolsToRandomize = new List<Tool>();
    [ShowIf("randomize", true)] private Vector2Int toolsMinMax = new Vector2Int(1, 4);
    public List<Tool> toolsToSell = new List<Tool>();

    private void Start()
    {
        if (randomize)
        {
            int r = Random.Range(toolsMinMax.x, toolsMinMax.y);

            toolsToSell.Clear();
            var tempList = new List<Tool>(toolsToRandomize);
            for (int i = 0; i < r; i++)
            {
                int rrr = Random.Range(0, tempList.Count);
                toolsToSell.Add(tempList[rrr]);
                tempList.RemoveAt(rrr);
            }
        }
    }
}
