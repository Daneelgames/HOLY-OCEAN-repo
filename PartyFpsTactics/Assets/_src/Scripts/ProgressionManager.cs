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
    void Awake()
    {
        if (Instance != null)
            return;
        
        Instance = this;
    }

    public void SetCurrentLevel(int index)
    {
        currentLevelIndex = Mathf.Clamp(index, 0, levelDatas.Count - 1);
    }
}