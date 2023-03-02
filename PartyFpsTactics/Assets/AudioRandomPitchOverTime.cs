using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioRandomPitchOverTime : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Vector2 pitchMinMax;
    [SerializeField] private Vector2 transitionTimeMinMax;
    void OnEnable()
    {
        if (pitchCoroutine != null)
            StopCoroutine(pitchCoroutine);
        pitchCoroutine = StartCoroutine(RandomizePitchCoroutine());
    }


    private Coroutine pitchCoroutine;

    IEnumerator RandomizePitchCoroutine()
    {
        while (true)
        {
            yield return null;
            float t = 0;
            float transitionTime = Random.Range(transitionTimeMinMax.x, transitionTimeMinMax.y);
            float startPitch = _audioSource.pitch;
            float newPitch = Random.Range(pitchMinMax.x, pitchMinMax.y);
        
            while (t < transitionTime)
            {
                yield return null;
                t += Time.deltaTime;
                _audioSource.pitch = Mathf.Lerp(startPitch, newPitch, t / transitionTime);
            }
        }
    }
}
