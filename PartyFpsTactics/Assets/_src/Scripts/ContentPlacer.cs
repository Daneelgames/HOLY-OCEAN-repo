using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using MrPink;
using MrPink.Health;
using MrPink.Units;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class ContentPlacer : NetworkBehaviour
{
    public static ContentPlacer Instance;

    private void Awake()
    {
        Instance = this;
    }
    
    [SerializeField] private int maxEnemiesAlive = 30;
    [SerializeField] private float respawnDelay = 5;
    [SerializeField] private float minMobSpawnDistance = 20;
    
    public List<InteractiveObject> lootToSpawnAround;
    
    private void OnEnable()
    {
        if (respawnDelay <= 0)
            return;
        
        if (spawnAroundPlayer != null)
            StopCoroutine(spawnAroundPlayer);
        
        spawnAroundPlayer = StartCoroutine(SpawnAroundPlayer());
    }

    private Coroutine spawnAroundPlayer;
    
    IEnumerator SpawnAroundPlayer()
    {
        while (Game._instance == null || Game.LocalPlayer == null)
        {
            Debug.Log("SpawnAroundPlayer wait");
            yield return new WaitForSeconds(1);
        }
        float cooldown = respawnDelay;
        while (true)
        {
            yield return new WaitForSeconds(cooldown);
            
                Debug.Log("SpawnAroundPlayer 0");
            if (base.IsHost)
            {
                Debug.Log("SpawnAroundPlayer SpawnRedUnitAroundPlayer");
                SpawnRedUnitAroundPlayer();
            }
            
            if (Game.LocalPlayer == null || Game.LocalPlayer.Health.health <= 0)
                continue;

            Debug.Log("SpawnAroundPlayer SpawnLootAroundPlayer");
            SpawnLootAroundPlayer();
        }
    }

    [Server]
    void SpawnRedUnitAroundPlayer()
    {
        if (UnitsManager.Instance.HcInGame.Count > maxEnemiesAlive)
            return;
        var players = Game._instance.PlayersInGame;
        var randomPlayer = players[Random.Range(0, players.Count)];
        
        Vector3 pos = NavMeshPosAroundPosition(randomPlayer.MainCamera.transform.position, 100);
            
        if (Vector3.Distance(pos, randomPlayer.MainCamera.transform.position) < minMobSpawnDistance)
            return;

        SpawnRedUnit(pos);
    }
    
    [Server]
    public void SpawnEnemiesInBuilding(BuildingGenerator.Building building)
    {
        StartCoroutine(SpawnEnemiesInBuildingCoroutine(building));
    }

    [Server]
    public IEnumerator SpawnEnemiesInVoxelBuilding(List<VoxelBuildingFloor> floors)
    {
        for (int i = 0; i < floors.Count; i++)
        {
            bool noPlaceOnFloor = false;
            var floor = floors[i];
            var size = floor.LevelSize;
            int mobsAmount = ((size.x * size.z) / 50) / 3;
            for (int j = 0; j < mobsAmount; j++)
            {
                Vector3 pos = floor.transform.position;

                int attempts = 0;
                while (true)
                {
                    yield return null;
                    
                    pos = floor.GetRandomWorldPosOnFloor();
                    
                    NavMesh.SamplePosition(pos, out var hit, Mathf.Infinity, NavMesh.AllAreas);
                    if (hit.hit)
                    {
                        pos = hit.position;
                        break;
                    }

                    attempts++;
                    if (attempts > 60)
                    {
                        noPlaceOnFloor = true;
                        break;
                    }
                }

                if (noPlaceOnFloor)
                {
                    break;
                }
                
                var unit =  Instantiate(UnitsManager.Instance.GetRandomRedUnit, pos, Quaternion.identity, UnitsManager.Instance.SpawnRoot); // spawn only easy one for now
                ServerManager.Spawn(unit.gameObject);
            }
        }
    }
    
    [Server]
    IEnumerator SpawnEnemiesInBuildingCoroutine(BuildingGenerator.Building building)
    {
        var tilesForSpawns = new List<TileHealth>();

        for (int i = 0; i < building.spawnedBuildingLevels.Count; i++)
        {
            var level = building.spawnedBuildingLevels[i];
                
            tilesForSpawns.Clear();
            for (var index = level.tilesInside.Count - 1; index >= 0; index--)
            {
                var tile = level.tilesInside[index];
                if (tile == null)
                {
                    level.tilesInside.RemoveAt(index);
                    continue;
                }
                tilesForSpawns.Add(tile);
            }

            for (int j = 0; j < level.unitsToSpawn.Count; j++)
            {
                var randomTile = tilesForSpawns[Random.Range(0, tilesForSpawns.Count)];
                var unit =  Instantiate(level.unitsToSpawn[j], randomTile.transform.position, Quaternion.identity, UnitsManager.Instance.SpawnRoot); // spawn only easy one for now
                ServerManager.Spawn(unit.gameObject);
            }
            for (int j = 0; j < level.uniqueNpcToSpawn.Count; j++)
            {
                var randomTile = tilesForSpawns[Random.Range(0, tilesForSpawns.Count)];
                var unit =  Instantiate(level.uniqueNpcToSpawn[j], randomTile.transform.position, Quaternion.identity, UnitsManager.Instance.SpawnRoot); // spawn only easy one for now
                ServerManager.Spawn(unit.gameObject);
            }
            yield return null;
        }
    }
        
    [Server]
    void SpawnRedUnit(Vector3 pos)
    {
        pos = UnitsManager.Instance.SamplePos(pos);
        var unit =  Instantiate(UnitsManager.Instance.redTeamUnitPrefabs[Random.Range(0, UnitsManager.Instance.redTeamUnitPrefabs.Count)], pos, Quaternion.identity, UnitsManager.Instance.SpawnRoot); // spawn only easy one for now
        ServerManager.Spawn(unit.gameObject);
    }
    void SpawnLootAroundPlayer()
    {
        float distanceToClosestPickup = InteractableEventsManager.Instance.GetDistanceFromClosestPickUpToPosition(Game.LocalPlayer.transform.position);

        if (distanceToClosestPickup < 50)
            return;

        Vector3 pos = NavMeshPosAroundPosition(Game.LocalPlayer.MainCamera.transform.position, 100);
            
        if (Vector3.Distance(pos, Game.LocalPlayer.MainCamera.transform.position) < 10)
            return;
        SpawnRandomLoot(pos);
    }

    public void SpawnRandomLoot(Vector3 pos)
    {
        var loot = Instantiate(lootToSpawnAround[Random.Range(0, lootToSpawnAround.Count)], pos,
            Quaternion.Euler(Random.Range(0, 360), Random.Range(0, 360), Random.Range(0, 360)));
        if (loot.rb)
        {
            loot.rb.useGravity = false;
            loot.rb.constraints = RigidbodyConstraints.FreezeAll;
            loot.rb.isKinematic = false;
        }
    }

    public Vector3 NavMeshPosAroundPosition(Vector3 initPos, float maxDistance)
    {
        Vector3 randomDir = Game.LocalPlayer.MainCamera.transform.forward;
        
        randomDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-0.5f, 0.5f), Random.Range(-1f, 1f));
        
        NavMesh.SamplePosition(initPos + randomDir * maxDistance, out var hit, Mathf.Infinity, NavMesh.AllAreas);
        return hit.position;
    }

}
