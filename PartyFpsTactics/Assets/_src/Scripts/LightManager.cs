using System;
using System.Collections;
using System.Collections.Generic;
using MrPink;
using UnityEngine;
using FogMode = UnityEngine.FogMode;
using Random = UnityEngine.Random;

public class LightManager : MonoBehaviour
{
    private float windLifeTime = 0;
    private float windCooldown = 0;
    public Vector2 windLifeTimeMinMax = new Vector2(10, 60); 
    public Vector2 windCooldownMinMax = new Vector2(30, 120); 
    public ParticleSystem rustWind;
    public AudioSource windAu;
    public float windActiveRate = 50;
    public ParticleSystem.EmissionModule rustWindEmission;

    private void Start()
    {
        return;
        StartCoroutine(Wind());
    }

    IEnumerator Wind()
    {
        rustWindEmission = rustWind.emission;
        windCooldown = RandomWindCooldown();
        windLifeTime = 0;
        
        while (true)
        {
            yield return null;
            
            if (windCooldown <= 0)
            {
                windCooldown = RandomWindCooldown();
                windLifeTime = RandomWindLifeTime();
                windAu.volume = 0;
                windAu.Play();
            }

            if (windLifeTime > 0)
            {
                rustWind.transform.parent.position = Game.Player.Position;
                rustWindEmission.rateOverTime = Mathf.Lerp(rustWindEmission.rateOverTime.constant, windActiveRate, 10 *Time.deltaTime);
                windAu.volume = Mathf.Lerp(windAu.volume, 1, 0.1f *Time.deltaTime);
                windLifeTime -= Time.deltaTime;
            }
            else if (windCooldown > 0)
            {
                rustWindEmission.rateOverTime = Mathf.Lerp(rustWindEmission.rateOverTime.constant, 0, 10 *Time.deltaTime);
                windAu.volume = Mathf.Lerp(windAu.volume, 0, 0.1f *Time.deltaTime);
                if (windAu.volume <= 0)
                    windAu.Stop();
                windCooldown -= Time.deltaTime;
            }
        }
    }

    float RandomWindCooldown()
    {
        return Random.Range(windCooldownMinMax.x, windCooldownMinMax.y);
    }
    float RandomWindLifeTime()
    {
        return Random.Range(windLifeTimeMinMax.x, windLifeTimeMinMax.y);
    }
}