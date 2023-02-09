using System.Collections;
using System.Collections.Generic;
using _src.Scripts.Data;
using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance;
    public List<ProcLevelData> levelDatas;
    
    public int currentLevelIndex = 0;
    
    public ProcLevelData CurrentLevel => levelDatas[currentLevelIndex];
    public ProcLevelData RandomLevel => levelDatas[Random.Range(0, levelDatas.Count)];
    void Awake()
    {
        if (Instance != null)
            return;
        
        Instance = this;
    }

    public void LevelCompleted()
    {
        currentLevelIndex = Mathf.Clamp(currentLevelIndex + 1, 0, levelDatas.Count - 1);
        CharacterSubtitles.Instance.TryToStartCharacterSubtitles(CurrentLevel.LevelStartCharacterSubtitlesData);
    }

    public void SetCurrentLevel(int index)
    {
        currentLevelIndex = Mathf.Clamp(index, 0, levelDatas.Count - 1);
    }
}