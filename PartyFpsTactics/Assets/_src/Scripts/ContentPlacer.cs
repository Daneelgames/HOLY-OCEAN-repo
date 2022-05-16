using System;
using System.Collections;
using System.Collections.Generic;
using MrPink;
using MrPink.Units;
using UnityEngine;
using Random = UnityEngine.Random;

public class ContentPlacer : MonoBehaviour
{
    public static ContentPlacer Instance;

    private void Awake()
    {
        Instance = this;
    }

    public List<Transform> proceedGameObjects = new List<Transform>();
    
    public List<InteractiveObject> lootToSpawnAround;
    private void Start()
    {
        StartCoroutine(SpawnAroundPlayer());
    }

    IEnumerator SpawnAroundPlayer()
    {
        float cooldown = 5;
        while (true)
        {
            yield return new WaitForSeconds(cooldown);
            
            if (Game.Player.Health.health <= 0)
                continue;
            
            SpawnRedUnitAroundPlayer();
            SpawnLootAroundPlayer();
        }
    }

    public void SpawnRedUnitAroundPlayer()
    {
        Vector3 pos = RaycastedPosAroundPosition(Game.Player._mainCamera.transform.position, 100);
            
        if (Vector3.Distance(pos, Game.Player._mainCamera.transform.position) < 20)
            return;

        UnitsManager.Instance.SpawnRedUnit(pos);
    }
    
    void SpawnLootAroundPlayer()
    {
        Vector3 pos = RaycastedPosAroundPosition(Game.Player._mainCamera.transform.position, 100);
            
        if (Vector3.Distance(pos, Game.Player._mainCamera.transform.position) < 10)
            return;
        SpawnRandomLoot(pos);
    }

    public InteractiveObject SpawnRandomLoot(Vector3 pos)
    {
        var loot = Instantiate(lootToSpawnAround[Random.Range(0, lootToSpawnAround.Count)], pos,
            Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)));
        if (loot.rb)
        {
            loot.rb.useGravity = false;
            loot.rb.constraints = RigidbodyConstraints.FreezeAll;
            loot.rb.isKinematic = false;
        }
        return loot;
    }

    Vector3 RaycastedPosAroundPosition(Vector3 initPos, float maxDistance)
    {
        Vector3 randomDir = Game.Player._mainCamera.transform.forward;
        
        randomDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-0.5f, 0.5f), Random.Range(-1f, 1f));
        if (!Physics.Raycast(Game.Player._mainCamera.transform.position, randomDir, out var hit, maxDistance,
            GameManager.Instance.AllSolidsMask))
            return initPos;
            
        if (GameManager.Instance.IsPositionInPlayerFov(hit.point))
            return initPos;
        
        return hit.point;
    }
}
