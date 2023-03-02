using System.Collections;
using System.Collections.Generic;
using MrPink;
using UnityEngine;
using UnityEngine.Audio;

public class UnderwaterSound : MonoBehaviour
{
    [SerializeField] private AudioMixerSnapshot aboveWaterSnapshot;
    [SerializeField] private AudioMixerSnapshot underWaterSnapshot;
    [SerializeField] private List<AudioClip> waterSplashes = new List<AudioClip>();
    [SerializeField] private AudioSource waterSplashAu;
    private bool playingUnderwater = false;
    void Start()
    {
        StartCoroutine(GetPlayerUnderwater());
    }

    IEnumerator GetPlayerUnderwater()
    {
        while (true)
        {
            yield return null;
            if (Game._instance == null || Game.LocalPlayer == null)
                continue;
            var playerUnderwater = Game.LocalPlayer.Movement.State.HeadIsUnderwater;
            
            if (playerUnderwater != playingUnderwater)
                SetUnderwater(playerUnderwater);
        }
    }

    void SetUnderwater(bool isUnderwater)
    {
        playingUnderwater = isUnderwater;
        
        if (isUnderwater)
            underWaterSnapshot.TransitionTo(0.5f);
        else
            aboveWaterSnapshot.TransitionTo(0.5f);
        
        waterSplashAu.clip = waterSplashes[Random.Range(0, waterSplashes.Count)];
        waterSplashAu.pitch = Random.Range(0.8f, 1.2f);
        waterSplashAu.Play();
    }
}
