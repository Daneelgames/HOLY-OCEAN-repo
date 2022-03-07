using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorTriggerReciever : MonoBehaviour
{
    public AudioSource au;
    public void PlayAudio()
    {
        au.pitch = Random.Range(0.75f, 1.25f);
        au.Play();
    }
}
