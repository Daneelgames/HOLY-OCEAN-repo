using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayRandomTrackOnStart : MonoBehaviour
{
    public AudioSource au;
    public List<AudioClip> clips;
    public Vector2 pitchMinMax = new Vector2(0.75f, 1.2f); 
    void Start()
    {
        au.clip = clips[Random.Range(0, clips.Count)];
        au.pitch = Random.Range(pitchMinMax.x, pitchMinMax.y);
        au.Play();
    }
}