using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Units;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnLootOnDeath : MonoBehaviour
{
        public Unit selfUnit;
        [Serializable]
        class Loot
        {
            public GameObject objectToSpawnOnDeath;
            [Range(0,1)]
            public float chance = 1;
        }

        [SerializeField]
        List<Loot> lootToSpawn;

        public void SpawnLoot()
        {
            for (int i = 0; i < lootToSpawn.Count; i++)
            {
                float r = Random.value;
                if (r <= lootToSpawn[i].chance)
                    Instantiate(lootToSpawn[i].objectToSpawnOnDeath,
                        selfUnit.HealthController.visibilityTrigger.transform.position,
                        selfUnit.HealthController.visibilityTrigger.transform.rotation);
            }
        }
}
