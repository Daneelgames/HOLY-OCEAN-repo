using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPart : MonoBehaviour
{
    public HealthController hc;
    public int localHealth = 100;

    public void DamageTile(int dmg)
    {
        localHealth -= dmg;
        if (localHealth<=0)
        {
            LevelGenerator.Instance.TileDestroyed(this);
            Destroy(gameObject); 
            return;
        }
        LevelGenerator.Instance.TileDamaged(this);
    }
}
