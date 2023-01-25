using System;
using System.Collections;
using System.Collections.Generic;
using _src.Scripts.Data;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class CharacterSubtitles : MonoBehaviour
{
    public static CharacterSubtitles Instance;
    
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private Animator dialogueVisualAnimator;
    [SerializeField] [ReadOnly] private CharacterSubtitlesData currentCharacterSubtitles;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void StartCharacterSubtitles(CharacterSubtitlesData _characterSubtitlesData)
    {
        currentCharacterSubtitles = _characterSubtitlesData;
    }
}
