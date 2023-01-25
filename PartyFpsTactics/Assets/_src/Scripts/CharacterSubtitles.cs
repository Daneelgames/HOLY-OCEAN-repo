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
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] [ReadOnly] private CharacterSubtitlesData currentCharacterSubtitles;
    private static readonly int Active = Animator.StringToHash("Active");
    private bool playing = false;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool TryToStartCharacterSubtitles(CharacterSubtitlesData _characterSubtitlesData)
    {
        if (playing) return false;
        currentCharacterSubtitles = _characterSubtitlesData;
        StartCoroutine(PlayPhrases());
        return true;
    }

    IEnumerator PlayPhrases()
    {
        for (int i = 0; i < currentCharacterSubtitles.phrases.Count; i++)
        {
            subtitleText.text = currentCharacterSubtitles.phrases[i].messageText;
            dialogueVisualAnimator.SetBool(Active, true);
            
            _audioSource.clip = currentCharacterSubtitles.phrases[i].messageAudio;
            _audioSource.Play();
            
            yield return new WaitForSeconds(_audioSource.clip.length);
            dialogueVisualAnimator.SetBool(Active, false);
            yield return new WaitForSeconds(_audioSource.clip.length * .25f);
            if (currentCharacterSubtitles.phrases[i].RunEvent == false)
                continue;
            InteractableEventsManager.Instance.RunEvent(currentCharacterSubtitles.phrases[i].eventToRun);   
        }
        
        dialogueVisualAnimator.SetBool(Active, false);
    }
}
