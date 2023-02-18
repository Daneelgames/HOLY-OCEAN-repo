using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class DamageUnitsOnCollision : MonoBehaviour
{
    [SerializeField] private int damage = 50;

    public UnityAction OnPlayerDamaged;

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer != 7)
            return;
        
        var Health = GetHealthController(collision.gameObject);
        if (Health == null)
            return;
        
        Health.Damage(damage, DamageSource.Environment);
        if (Health.IsPlayer && OnPlayerDamaged != null)
            OnPlayerDamaged.Invoke();
    }

    HealthController GetHealthController(GameObject go)
    {
        var Health = go.GetComponent<HealthController>();
        if (Health == null)
        {
            var bodyPart = go.GetComponent<BodyPart>();
            if (bodyPart)
            {
                Health = bodyPart.HealthController;
            }
        }

        return Health;
    }
}