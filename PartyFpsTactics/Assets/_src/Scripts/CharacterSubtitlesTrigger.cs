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
    [SerializeField] private CharacterSubtitlesData _characterSubtitlesData;
    [SerializeField] private bool triggerOnce = true;
    bool triggered = false;

    private void Awake()
    {
        if (roseInstance)
            RoseInstance = this;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && triggered) return;
        if (other.gameObject != Game.LocalPlayer.gameObject) return;
        
        if (CharacterSubtitles.Instance.TryToStartCharacterSubtitles(_characterSubtitlesData))
            triggered = true;
    }

    public void SetTriggeredOff()
    {
        triggered = false;
    }
}
