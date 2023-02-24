using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private List<AudioClip> music;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
    }

    public void PlayIslandMusic()
    {
        _audioSource.clip = music[Random.Range(0, music.Count)];
        _audioSource.pitch = Random.Range(0.85f, 1.15f);
        _audioSource.Play();
    }

    public void StopMusic()
    {
        _audioSource.Stop();
    }
}