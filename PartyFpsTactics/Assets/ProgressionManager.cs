using System.Collections;
using System.Collections.Generic;
using _src.Scripts.Data;
using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance;
    public List<ProcLevelData> levelDatas;
    
    public int currentLevel = 0;
    void Awake()
    {
        if (Instance != null)
            return;
        
        Instance = this;
    }

    public void SetCurrentLevel(int index)
    {
        currentLevel = index;
    }
}


