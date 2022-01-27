using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public Transform shotHolder;
    public ProjectileController projectilePrefab;
    public float cooldown = 1;
    bool onCooldown = false;
    public bool OnCooldown
    {
        get { return onCooldown; }
        set { onCooldown = value; }
    }
    
    public void Shot()
    {
        Shot(shotHolder.forward);
    }

    public void Shot(Vector3 direction)
    {
        var newProjectile = Instantiate(projectilePrefab, shotHolder.position, Quaternion.LookRotation(direction));
        
        StartCoroutine(Cooldown());
    }

    IEnumerator Cooldown()
    {
        OnCooldown = true;
        yield return new WaitForSeconds(cooldown);
        OnCooldown = false;
    }
}