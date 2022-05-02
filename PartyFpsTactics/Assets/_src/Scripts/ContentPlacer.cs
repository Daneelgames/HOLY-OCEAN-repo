using System;
using System.Collections;
using System.Collections.Generic;
using MrPink;
using MrPink.Units;
using UnityEngine;
using Random = UnityEngine.Random;

public class ContentPlacer : MonoBehaviour
{
    public List<Transform> proceedGameObjects = new List<Transform>();
    
    public List<InteractiveObject> lootToSpawnAround;
    private void Start()
    {
        StartCoroutine(FollowPlayer());
    }

    IEnumerator FollowPlayer()
    {
        var player = Game.Player.Health;
        float cooldown = 5;
        Collider[] hits;
        while (true)
        {
            yield return null;
            cooldown -= Time.deltaTime;
            transform.position = player.transform.position;
            
            if (cooldown > 0)
                continue;
            
            // cooldown <= 0
            cooldown = 5;

            hits = Physics.OverlapSphere(transform.position, 100, GameManager.Instance.AllSolidsMask,
                QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hits.Length; i++)
            {
                yield return new WaitForSeconds(0.01f);
                
                if (proceedGameObjects.Contains(hits[i].transform))
                    continue;
                

                var spawnPos = hits[i].ClosestPoint(hits[i].transform.position + Vector3.up * 100);
                if (Physics.Raycast(Game.Player._mainCamera.transform.position,
                    spawnPos - Game.Player._mainCamera.transform.position,
                    Vector3.Distance(Game.Player._mainCamera.transform.position, spawnPos),
                    GameManager.Instance.AllSolidsMask))
                {
                    continue;
                }
                
                proceedGameObjects.Add(hits[i].transform);
                
                if (Random.value > 0.5f)
                {
                    // mob
                    UnitsManager.Instance.SpawnRedUnit(spawnPos);
                    continue;
                }
                var newLoot = Instantiate(lootToSpawnAround[Random.Range(0, lootToSpawnAround.Count)], spawnPos, Quaternion.Euler(Random.Range(0,360),Random.Range(0,360),Random.Range(0,360)));
            }
        }
    }
}
