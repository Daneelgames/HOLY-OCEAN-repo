using System;
using System.Collections;
using System.Collections.Generic;
using _src.Scripts.Data;
using MrPink;
using UnityEngine;

public class CharacterSubtitlesTrigger : MonoBehaviour
{
    [SerializeField] private CharacterSubtitlesData _characterSubtitlesData;
    [SerializeField] private bool triggerOnce = true;
    bool triggered = false;
    
    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && triggered) return;
        if (other.gameObject != Game.LocalPlayer.gameObject) return;
        triggered = true;
        CharacterSubtitles.Instance.StartCharacterSubtitles(_characterSubtitlesData);
    }
}
