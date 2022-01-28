using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using Random = UnityEngine.Random;

public class HealthController : MonoBehaviour
{
    public int health = 100;
    int healthMax = 100;
    public bool destroyOnDeath = false;
    public Collider visibilityTrigger;
    
    [Header("Shake On Damage")]
    public bool proceduralDamageShake = false;
    public bool shakeX = true;
    public bool shakeY = false;
    public bool shakeZ = true;
    public float maxShakeOffset = 0.1f;
    public Transform transformToShake;
    
    [Header("AI")]
    public AiMovement AiMovement;
    public AiWeaponControls AiWeaponControls;
    public HumanVisualController HumanVisualController;


    [Header("Mis")] 
    public NavMeshSurface navmeshToRebuildOnDeath;
    public enum Team
    {
        Red, Blue, NULL
    }

    public Team team;

    public List<BodyPart> bodyParts;


    public List<DamageState> damageStates;

    private void Start()
    {
        healthMax = health;
        GameManager.Instance.ActiveHealthControllers.Add(this);
    }

    [ContextMenu("GetBodyParts")]
    public void GetBodyParts()
    {
        bodyParts = new List<BodyPart>();
        var parts = transform.GetComponentsInChildren<BodyPart>();
        for (int i = 0; i < parts.Length; i++)
        {
            bodyParts.Add(parts[i]);
            parts[i].hc = this;
        }
    }
    [ContextMenu("SetHcToParts")]
    public void SetHcToParts()
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            bodyParts[i].hc = this;
        }
    }

    public void Damage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Death();
            return;
        }

        if (proceduralDamageShake)
            StartCoroutine(DamageShake());
        SetDamageState();
    }

    IEnumerator DamageShake()
    {
        float t = 0f;
        var originalPos = transformToShake.localPosition;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            float x = 0;
            if (shakeX)
                x = Random.Range(-maxShakeOffset, maxShakeOffset);
            float y = 0;
                y = Random.Range(-maxShakeOffset, maxShakeOffset);
            float z = 0;
                z = Random.Range(-maxShakeOffset, maxShakeOffset);
                
            transformToShake.localPosition = originalPos + new Vector3(x,y,z);
            yield return null;
        }
    }

    void SetDamageState()
    {
        float currentHealthPercentage = health / healthMax;
        for (int i = 0; i < damageStates.Count; i++)
        {
            if (damageStates[i].healthPercentage > currentHealthPercentage)
            {
                damageStates[i].visual.SetActive(false);
            }
            else
            {
                damageStates[i].visual.SetActive(true);
                break;
            }
        }
    }

    void Death()
    {
        if (AiMovement)
            AiMovement.Death();

        if (HumanVisualController)
            HumanVisualController.DeathRagdoll();
        
        if (navmeshToRebuildOnDeath)
        {
            navmeshToRebuildOnDeath.BuildNavMesh();
            //navmeshToRebuildOnDeath.UpdateNavMesh(navmeshToRebuildOnDeath.navMeshData);
        }
        
        if (destroyOnDeath)
            Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (GameManager.Instance.ActiveHealthControllers.Contains(this))
            GameManager.Instance.ActiveHealthControllers.Remove(this);
        if (CommanderControls.Instance.unitsInParty.Contains(this))
            CommanderControls.Instance.unitsInParty.Remove(this);
    }
}

[Serializable]
public class DamageState
{
    public string name = "Normal State";
    public GameObject visual;
    [Range(0, 1)] public float healthPercentage = 1;
}