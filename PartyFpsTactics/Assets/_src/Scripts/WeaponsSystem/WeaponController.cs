using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using MrPink.PlayerSystem;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public Transform shotHolder;
    public ProjectileController projectilePrefab;
    public float cooldown = 1;
    bool onCooldown = false;

    public Transform raycastStartPoint;
    public Transform raycastEndPoint;

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
    
    public void Shot(HealthController ownerHc)
    {
        Shot(shotHolder.forward, ownerHc);
    }

    public void Shot(Vector3 direction, HealthController ownerHc)
    {
        var newProjectile = Instantiate(projectilePrefab, shotHolder.position, Quaternion.LookRotation(direction));
        ScoringActionType action = ScoringActionType.NULL;
        if (ownerHc == Player.Health)
            action = Player.Movement.GetCurrentScoringAction();
        
        newProjectile.Init(ownerHc, action);
        StartCoroutine(Cooldown());
    }

    IEnumerator Cooldown()
    {
        OnCooldown = true;
        yield return new WaitForSeconds(cooldown);
        OnCooldown = false;
    }
}