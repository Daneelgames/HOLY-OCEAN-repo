using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using UnityEngine;
using UnityEngine.Events;

public class DamageZone : MonoBehaviour
{
    [SerializeField] private DamageSource damageSource = DamageSource.Environment;
    [SerializeField] private int damage = 50;
    [SerializeField] private float damageCooldown = 0.5f;
    [SerializeField] private float maxDamageDistance = 15f;
    public UnityAction OnPlayerDamaged;
    private List<UnitDamageCoroutine> _unitDamageCoroutines = new List<UnitDamageCoroutine>();
    [SerializeField] private List<Collider> collidersToIgnore = new List<Collider>();
    struct UnitDamageCoroutine
    {
        public Coroutine damageCoroutine;
        public HealthController hc;
        public GameObject go;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != 7) // units only
            return;
        if (collidersToIgnore.Contains(other))
            return;
        
        var Health = GetHealthController(other.gameObject);
        if (Health == null)
            return;
        
        foreach (var unitDamageCoroutine in _unitDamageCoroutines)
        {
            if (unitDamageCoroutine.hc == Health)
                return;
        }
        UnitDamageCoroutine newUnitDamageCoroutine = new UnitDamageCoroutine();
        newUnitDamageCoroutine.hc = Health;
        newUnitDamageCoroutine.go = other.gameObject;
        newUnitDamageCoroutine.damageCoroutine = StartCoroutine(DamageCoroutine(Health));
        _unitDamageCoroutines.Add(newUnitDamageCoroutine);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != 7) // units only
            return;

        if (collidersToIgnore.Contains(other))
            return;
        
        var Health = GetHealthController(other.gameObject);
        if (Health == null)
            return;
        
        foreach (var unitDamageCoroutine in _unitDamageCoroutines)
        {
            if (unitDamageCoroutine.hc == Health)
            {
                if (unitDamageCoroutine.damageCoroutine != null)
                    StopCoroutine(unitDamageCoroutine.damageCoroutine);
                _unitDamageCoroutines.Remove(unitDamageCoroutine);
                return;
            }
        }
    }

    
    IEnumerator DamageCoroutine(HealthController hc)
    {
        while (true)
        {
            if (Vector3.Distance(transform.position, hc.transform.position) > maxDamageDistance)
                break;
            var resultHealth = hc.Damage(damage, damageSource);
            if (resultHealth < 1)
            {
                break;
            }
            if (hc.IsPlayer && OnPlayerDamaged != null)
                OnPlayerDamaged.Invoke();   
            
            yield return new WaitForSeconds(damageCooldown);
        }    
        foreach (var unitDamageCoroutine in _unitDamageCoroutines)
        {
            if (unitDamageCoroutine.hc == hc)
            {
                if (unitDamageCoroutine.damageCoroutine != null)
                    StopCoroutine(unitDamageCoroutine.damageCoroutine);
                _unitDamageCoroutines.Remove(unitDamageCoroutine);
                yield break;
            }
        }
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
