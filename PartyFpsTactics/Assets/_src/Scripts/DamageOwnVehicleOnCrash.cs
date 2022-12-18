using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using Sirenix.OdinInspector;
using UnityEngine;

public class DamageOwnVehicleOnCrash : MonoBehaviour
{
    [SerializeField] private HealthController carHc;
    [SerializeField] private float minCrashDamage = 10;
    [SerializeField] private float minRbVelocityToCrash = 10;
    [SerializeField] [ReadOnly] private float rbCurrVelocity;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private List<Collider> ownColliders;

    [SerializeField] private float ownDamageCooldown = 0.5f;
    private float ownDamageCooldownCurrent;
    private void Update()
    {
        if (ownDamageCooldownCurrent > 0)
            ownDamageCooldownCurrent -= Time.deltaTime;
        
        rbCurrVelocity = rb.velocity.magnitude;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (ownDamageCooldownCurrent > 0)
            return;
        
        if (rb.velocity.magnitude < minRbVelocityToCrash)
            return;

        if (other.gameObject.layer != 6 && other.gameObject.layer != 12 && other.gameObject.layer != 11) // if not solid, nonNavSolid and interactable
            return;    
        
        foreach (var collider1 in ownColliders)
        {
            if (other == collider1)
                return;
        }

        var dmg = Mathf.RoundToInt(rbCurrVelocity * minCrashDamage);
        Debug.Log("CRASH HAPPENED. CUR VEL " + rbCurrVelocity + "; CRASH DMG " + dmg);
        carHc.Damage(dmg, DamageSource.Environment);
    }
}
