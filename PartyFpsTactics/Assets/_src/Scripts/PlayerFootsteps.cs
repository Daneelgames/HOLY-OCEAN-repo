using System;
using System.Collections;
using System.Collections.Generic;
using MrPink;
using MrPink.PlayerSystem;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerFootsteps : MonoBehaviour
{
    public static PlayerFootsteps Instance;
    public AudioSource stepsAu;
    public List<AudioClip> stepClips;
    public List<AudioClip> climbClips;

    public float moveStepCooldown = 0.5f; 
    public float runStepCooldown = 0.7f; 
    float ttt = 1;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        StartCoroutine(GetSteps());
    }
    
    private IEnumerator GetSteps()
    {
        while (Game._instance == null || Game.LocalPlayer == null)
        {
            yield return null;
        }
        yield return null;
        
        var pm = Game.LocalPlayer.Movement;
        while (true)
        {
            yield return null;
            
            if ((!pm.State.IsGrounded && !pm.State.IsClimbing) || Game.LocalPlayer.VehicleControls.controlledMachine || Game.LocalPlayer.Health.health <= 0)
                continue;

            if (pm.State.IsMoving)
                ttt -= moveStepCooldown * Time.deltaTime;
            
            if (pm.State.IsRunning)
                ttt -= runStepCooldown * Time.deltaTime;

            if (ttt <= 0)
            {
                ttt = 1;
                stepsAu.pitch = Random.Range(0.75f, 1.25f);
                stepsAu.clip = pm.State.IsClimbing ? climbClips[Random.Range(0, climbClips.Count)] : stepClips[Random.Range(0, stepClips.Count)];
                stepsAu.Play();
                if (pm.State.IsRunning || (pm.State.IsMoving && pm.State.IsCrouching == false))
                    NoiseSystem.Instance.StepsNoise(pm.transform.position);
            }
        }
    }


    private Vector2 landingCooldown = new Vector2( 0.75f, 1f);
    private float currentLandingCooldown = 0;
    public void PlayLanding()
    {
        if (currentLandingCooldown > 0)
            return;
        
        if (stepsAu)
        {
            stepsAu.pitch = Random.Range(0.75f, 1.25f);
            stepsAu.clip = stepClips[Random.Range(0, stepClips.Count)];
            stepsAu.Play();
        }
        currentLandingCooldown = Random.Range(landingCooldown.x, landingCooldown.y);
        StartCoroutine(CooldownCoroutine());
    }

    IEnumerator CooldownCoroutine()
    {
        while (currentLandingCooldown > 0)
        {
            currentLandingCooldown -= Time.deltaTime;
            yield return null;
        }
    }
}
