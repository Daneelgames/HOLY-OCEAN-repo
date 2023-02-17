using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using UnityEngine;

public class DamageUnitsTrigger : MonoBehaviour
{
    [SerializeField] private int damagePerUpdate = 50;
    [SerializeField] private float updateTime = 1;

    private List<GameObject> goInside = new List<GameObject>();
    private List<HealthController> hcInside = new List<HealthController>();

    private void OnEnable()
    {
        StartCoroutine(UpdateCoroutine());
    }

    IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateTime);

            foreach (var healthController in hcInside)
            {
                if (healthController != null && healthController.health > 0)
                    healthController.Damage(damagePerUpdate, DamageSource.Environment);
            }
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.layer != 7)
            return;
        
        if (goInside.Contains(collision.gameObject))
            return;

        var Health = GetHealthController(collision.gameObject);
        if (Health == null)
            return;
        if (hcInside.Contains(Health))
            return;
        goInside.Add(Health.gameObject);
        hcInside.Add(Health);
    }

    private void OnTriggerExit(Collider other)
    {
        if (goInside.Contains(other.gameObject) == false)
            return;
        
        var Health = GetHealthController(other.gameObject);
        if (Health == null)
            return;
        if (hcInside.Contains(Health))
            hcInside.Remove(Health);
        if (goInside.Contains(Health.gameObject))
            goInside.Remove(Health.gameObject);
        
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