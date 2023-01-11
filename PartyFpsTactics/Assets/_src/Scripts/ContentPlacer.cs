using System;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet.Object;
using MrPink;
using MrPink.Health;
using MrPink.Units;
using NWH.DWP2.ShipController;
using Unity.Mathematics;
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
    [SerializeField] private AdvancedShipController defaultPlayerWaterbikerPrefab;
    [SerializeField] private List<HealthController> aiWaterBikes = new List<HealthController>();
    
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


    [ServerRpc(RequireOwnership = false)]
    void RpcSpawnPlayerBikeOnServer(NetworkConnection playerConnection)
    {
        SpawnPlayerBikeOnServer(playerConnection);
    }
    [Server]
    void SpawnPlayerBikeOnServer(NetworkConnection playerConnection)
    {
        var playerBike = Instantiate(defaultPlayerWaterbikerPrefab, Game.LocalPlayer.transform.position + Vector3.up * 5, Game.LocalPlayer.transform.rotation);
        ServerManager.Spawn(playerBike.gameObject);
        
        RpcSetPlayerDrivingOnClient(playerConnection, playerBike.gameObject);
    }
    
    [TargetRpc]
    void RpcSetPlayerDrivingOnClient(NetworkConnection playerConnection, GameObject bikeGameObject)
    {
        var veh = bikeGameObject.GetComponent<ControlledMachine>();
        Game.LocalPlayer.VehicleControls.RequestVehicleAction(veh);
    }
    
    
    IEnumerator SpawnAroundPlayer()
    {
        while (Game._instance == null || Game.LocalPlayer == null)
        {
            Debug.Log("SpawnAroundPlayer wait");
            yield return new WaitForSeconds(1);
        }

        // HOW TO SPAWN BIKE
        if (base.IsHost)
        {
            SpawnPlayerBikeOnServer(Game.LocalPlayer.Owner);
        }
        else
        {
            RpcSpawnPlayerBikeOnServer(Game.LocalPlayer.Owner);
        }

        yield return new WaitForSeconds(5);
        float cooldown = respawnDelay;
        while (true)
        {
            if (base.IsHost)
            {
                SpawnRedUnitAroundRandomPlayer();
            }

            if (Game.LocalPlayer != null && Game.LocalPlayer.Health.health > 0)
            {
                Debug.Log("SpawnAroundPlayer SpawnLootAroundPlayer");
                SpawnLootAroundLocalPlayer();
            }

            yield return new WaitForSeconds(cooldown);
        }
    }

    // spawn mob on navmesh if player is close to island
    // else spawn mob in bike in the ocean 
    [SerializeField] private float islandDistanceSpawn = 200;
    [Server]
    void SpawnRedUnitAroundRandomPlayer()
    {
        if (UnitsManager.Instance.HcInGame.Count > maxEnemiesAlive)
            return;
        var players = Game._instance.PlayersInGame;
        var randomPlayer = players[Random.Range(0, players.Count)];

        var distance = IslandSpawner.Instance.GetDistanceToClosestIsland(randomPlayer.transform.position);
        Vector3 pos;
        if (distance <= islandDistanceSpawn)
        {
            pos = NavMeshPosAroundPosition(randomPlayer.MainCamera.transform.position, islandDistanceSpawn);   
            if (Vector3.Distance(pos, randomPlayer.MainCamera.transform.position) < minMobSpawnDistance)
                return;

            SpawnRedUnit(pos);
        }
        else // spawn in sea
        {
            pos = randomPlayer.MainCamera.transform.position + Random.onUnitSphere * 50;   

            SpawnRedUnitInBoat(pos);
        }
    }
    
    [Server]
    public void SpawnEnemiesInBuilding(BuildingGenerator.Building building)
    {
        StartCoroutine(SpawnEnemiesInBuildingCoroutine(building));
    }

    [Server]
    public IEnumerator SpawnPropsInVoxelBuilding(List<VoxelBuildingFloor> floors)
    {
        yield return new WaitForSeconds(5);
        var propsPrefabs = BuildingGenerator.Instance.PropsPrefabs;
        
        for (int i = 0; i < floors.Count; i++)
        {
            bool noPlaceOnFloor = false;
            var floor = floors[i];
            var size = floor.LevelSize;
            int propsAmount = ((size.x * size.z) / 50) / 3;
            for (int j = 0; j < propsAmount; j++)
            {
                Vector3 pos = floor.transform.position;

                int attempts = 0;
                while (true)
                {
                    yield return null;
                    
                    pos = floor.GetRandomWorldPosOnFloor();

                    if (NavMesh.SamplePosition(pos, out var hit, Mathf.Infinity, NavMesh.AllAreas) == false)
                    {
                        continue;
                    }
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
                
                
                var prop = Instantiate(propsPrefabs[Random.Range(0, propsPrefabs.Count)], pos, Quaternion.Euler(0,Random.Range(0,360), 0));
                ServerManager.Spawn(prop.gameObject);
            }
        }
    }
    
    [Server]
    public IEnumerator SpawnEnemiesInVoxelBuilding(List<VoxelBuildingFloor> floors)
    {
        yield return new WaitForSeconds(5);
        for (int i = 0; i < floors.Count; i++)
        {
            bool noPlaceOnFloor = false;
            var floor = floors[i];
            var size = floor.LevelSize;
            int mobsAmount = ((size.x * size.z) / 50) / 3;
            //int mobsAmount = 0;
            for (int j = 0; j < mobsAmount; j++)
            {
                Vector3 pos = floor.transform.position;

                int attempts = 0;
                while (true)
                {
                    yield return null;
                    
                    pos = floor.GetRandomWorldPosOnFloor();

                    if (NavMesh.SamplePosition(pos, out var hit, Mathf.Infinity, NavMesh.AllAreas) == false)
                    {
                        continue;
                    }
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
    
    [Server]
    void SpawnRedUnitInBoat(Vector3 pos)
    {
        var unit =  Instantiate(UnitsManager.Instance.redTeamUnitPrefabs[Random.Range(0, UnitsManager.Instance.redTeamUnitPrefabs.Count)], pos, Quaternion.identity, UnitsManager.Instance.SpawnRoot); // spawn only easy one for now
        var boat = Instantiate(aiWaterBikes[Random.Range(0, aiWaterBikes.Count)], pos, Quaternion.identity, UnitsManager.Instance.SpawnRoot);
        
        ServerManager.Spawn(unit.gameObject);
        ServerManager.Spawn(boat.gameObject);
        
        unit.aiVehicleControls.DriverSitOnServer(boat);
    }
    void SpawnLootAroundLocalPlayer()
    {
        float distanceToClosestPickup = InteractableEventsManager.Instance.GetDistanceFromClosestPickUpToPosition(Game.LocalPlayer.transform.position);

        if (distanceToClosestPickup < 50)
            return;

        var distance = IslandSpawner.Instance.GetDistanceToClosestIsland(Game.LocalPlayer.transform.position);
        Vector3 pos;
        if (distance <= islandDistanceSpawn)
        {
            pos = NavMeshPosAroundPosition(Game.LocalPlayer.MainCamera.transform.position, islandDistanceSpawn);
        }
        else // spawn in sea
        {
            pos = Game.LocalPlayer.MainCamera.transform.position + Random.onUnitSphere * 50;
            pos.y = Game.LocalPlayer.MainCamera.transform.position.y;
        }   
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
        
        if (NavMesh.SamplePosition(initPos + randomDir * maxDistance, out var hit, Mathf.Infinity, NavMesh.AllAreas))
            return hit.position;

        return initPos;
    }

}
