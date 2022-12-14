using System;
using System.Collections;
using System.Collections.Generic;
using MrPink;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ChooseLevelIntermission : MonoBehaviour
{
    [SerializeField] private List<Text> buttonDescriptions;
    private List<GameManager.LevelType> _levelTypesPool = new List<GameManager.LevelType>();

    private void Start()
    {
        ChooseLevelPool();
    }

    void ChooseLevelPool()
    {
        _levelTypesPool.Clear();
        for (int i = 0; i < 3; i++)
        {
            // dont include intermission
            var randomLevel = (GameManager.LevelType)Random.Range(1, Enum.GetValues(typeof(GameManager.LevelType)).Length);
            _levelTypesPool.Add(randomLevel);
            buttonDescriptions[i].text = randomLevel.ToString();
        }
    }

    public void LevelChosen(int index)
    {
        var levelToLoad = _levelTypesPool[index];
        GameManager.Instance.StartLevel(levelToLoad);
    }
}
