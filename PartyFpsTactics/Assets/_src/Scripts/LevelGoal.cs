using System;
using System.Collections;
using System.Collections.Generic;
using MrPink;
using MrPink.PlayerSystem;
using UnityEngine;

public class LevelGoal : MonoBehaviour
{
    public static LevelGoal Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("TRYING TO ADD MORE LEVEL GOALS? DISCUSS THIS WITH A DIRECTOR");
            return;
        }

        Instance = this;
    }
}