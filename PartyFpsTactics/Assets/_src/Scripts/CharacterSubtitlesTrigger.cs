using System;
using System.Collections;
using System.Collections.Generic;
using _src.Scripts.Data;
using MrPink;
using UnityEngine;

public class CharacterSubtitlesTrigger : MonoBehaviour
{
    public static CharacterSubtitlesTrigger RoseInstance { get; private set; }
    [SerializeField] private bool roseInstance = false;
    [SerializeField] private bool triggerOnce = true;
    bool triggered = false;

    private void Awake()
    {
        if (roseInstance)
            RoseInstance = this;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Game.LocalPlayer == null) return;
        if (triggerOnce && triggered) return;
        if (other.gameObject != Game.LocalPlayer.gameObject) return;
        
        if (CharacterSubtitles.Instance.TryToStartCharacterSubtitles(ProgressionManager.Instance.CurrentLevel.LevelStartCharacterSubtitlesData))
            triggered = true;
    }

    public void LevelCompleted()
    {
        //triggered = false;
        // start next dialogue right away
    }
}