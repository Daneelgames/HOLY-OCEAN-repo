using System.Collections;
using System.Collections.Generic;
using _src.Scripts.LevelGenerators;
using MrPink.Health;
using UnityEngine;

public class GeneratedBuildingSupportsManager : MonoBehaviour
{
    public static GeneratedBuildingSupportsManager Instance;
    private List<Level> levels = new List<Level>();
    void Awake()
    {
        Instance = this;
    }

    public void Init()
    {
        levels = new List<Level>(LevelGenerator.Instance.spawnedMainBuildingLevels);
        StartCoroutine(CheckLevelsSupports());
    }

    IEnumerator CheckLevelsSupports()
    {
        yield return null;
        
        /*
        while (true)
        {
            yield return null;
            
            if (levels.Count <= 1)
                continue;
            
            for (int i = levels.Count - 1; i > 0; i--)
            {
                var topLevel = levels[i];
                var bottomLevel = levels[i - 1];

                for (int j = 0; j < topLevel.tilesFloor.Count; j++)
                {
                    var floorTile = topLevel.tilesFloor[j];
                    if (!floorTile)
                        continue;

                    if (TileHasSupport())
                    {
                        
                    }
                }
            }
        }
        */
    }

}
