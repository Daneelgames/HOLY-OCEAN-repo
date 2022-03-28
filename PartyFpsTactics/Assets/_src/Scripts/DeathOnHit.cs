using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using UnityEngine;

public class DeathOnHit : MonoBehaviour
{
    public float timeToDeath = 5;
    private bool counting = false;
    public GameObject countingFeedback;
    
    public void Hit(HealthController hc)
    {
        if (counting)
            return;

        StartCoroutine(Counting(hc));
    }

    IEnumerator Counting(HealthController hc)
    {
        countingFeedback.SetActive(true);
        float t = 0;
        while (t < timeToDeath)
        {
            yield return null;
            t += Time.deltaTime;
            if (hc == null || hc.health <= 0)
                yield break;
        }
        hc.Damage(hc.health, DamageSource.Environment);
    }
}
