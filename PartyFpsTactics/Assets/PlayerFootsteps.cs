using System;
using System.Collections;
using System.Collections.Generic;
using MrPink;
using MrPink.PlayerSystem;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerFootsteps : MonoBehaviour
{
    public AudioSource stepsAu;
    public List<AudioClip> stepClips;
    public List<AudioClip> climbClips;

    public float moveStepCooldown = 0.5f; 
    public float runStepCooldown = 0.7f; 
    float ttt = 1;

    private void OnEnable()
    {
        StartCoroutine(GetSteps());
    }
    
    private IEnumerator GetSteps()
    {
        yield return null;
        
        var pm = Game.Player.Movement;
        while (true)
        {
            yield return null;
            
            if ((!pm.State.IsGrounded && !pm.State.IsClimbing) || Game.Player.VehicleControls.controlledMachine || Game.Player.Health.health <= 0)
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
            }
        }
    }
}
