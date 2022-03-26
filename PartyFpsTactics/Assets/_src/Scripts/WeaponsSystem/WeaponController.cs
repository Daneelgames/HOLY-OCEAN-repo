using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using MrPink.PlayerSystem;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class WeaponController : MonoBehaviour
{
    public Transform shotHolder;
    public ProjectileController projectilePrefab;
    public float delay = 0;
    public float cooldown = 1;
    bool onCooldown = false;

    public Transform raycastStartPoint;
    public Transform raycastEndPoint;

    public AudioSource attackSignalAu;
    
    private Quaternion _initLocalRotation;
    public Quaternion InitLocalRotation
    {
        get { return _initLocalRotation; }
    }

    private void Awake()
    {
        _initLocalRotation = transform.localRotation;
    }

    public bool OnCooldown
    {
        get { return onCooldown; }
        set { onCooldown = value; }
    }
    
    public void Shot(HealthController ownerHc, Transform aiAimTransform = null)
    {
        StartCoroutine(Shot(shotHolder.forward, ownerHc, aiAimTransform));
    }

    public IEnumerator Shot(Vector3 direction, HealthController ownerHc, Transform aiAimTransform = null)
    {
        OnCooldown = true;
        if (attackSignalAu)
        {
            attackSignalAu.pitch = Random.Range(0.75f, 1.25f);
            attackSignalAu.Play();
        }

        yield return new WaitForSeconds(delay);

        if (ownerHc.health <= 0)
            yield break;

        if (aiAimTransform != null)
        {
            transform.LookAt(aiAimTransform.position);
            direction = (aiAimTransform.position - shotHolder.position).normalized;
        }
        
        var newProjectile = Instantiate(projectilePrefab, shotHolder.position, Quaternion.LookRotation(direction));
        ScoringActionType action = ScoringActionType.NULL;
        if (ownerHc == Player.Health)
            action = Player.Movement.GetCurrentScoringAction();

        newProjectile.Init(ownerHc, action);
        yield return new WaitForSeconds(cooldown);
        OnCooldown = false;
    }
}