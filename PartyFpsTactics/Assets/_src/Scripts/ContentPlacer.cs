using System;
using System.Collections;
using System.Collections.Generic;
using _src.Scripts.Data;
using FishNet.Connection;
using FishNet.Object;
using MrPink;
using MrPink.Health;
using MrPink.PlayerSystem;
using MrPink.Units;
using NWH.DWP2.ShipController;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class ContentPlacer : NetworkBehaviour
{
    public static ContentPlacer Instance;
    
    [SerializeField] private int maxMobsAlive = 30;
    [SerializeField] private float respawnDelay = 5;
    [SerializeField] private float minMobSpawnDistance = 20;
    [SerializeField] private AdvancedShipController defaultPlayerWaterbikerPrefab;
    [SerializeField] private List<HealthController> aiWaterBikes = new List<HealthController>();
    
    public InteractiveObject inventoryLootPrefab;
    public List<InteractiveObject> lootToSpawnAround;
    public List<InteractiveObject> toolsToSpawnOnBuildingLevels;

    private List<ContentPlacerBlocker> _contentPlacerBlockers = new List<ContentPlacerBlocker>();

    private void Awake()
    {
        Instance = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        
        //Instance = this;
        
        if (respawnDelay <= 0)
            return;
        
        if (spawnAroundPlayer != null)
            StopCoroutine(spawnAroundPlayer);
        
        spawnAroundPlayer = StartCoroutine(SpawnAroundPlayer());
    }

    private Coroutine spawnAroundPlayer;

    private InteractiveObject currentPlayerInventoryLoot;
    public void SpawnPlayerLootContainer(List<PlayerInventory.InventoryItem> inventoryItems, int moneyToDrop) // locally
    {
        if (currentPlayerInventoryLoot != null)
        {
            Destroy(currentPlayerInventoryLoot.gameObject);
        }
        currentPlayerInventoryLoot = Instantiate(inventoryLootPrefab, Game.LocalPlayer.Position,
            Game.LocalPlayer.transform.rotation);
        currentPlayerInventoryLoot.SaveInventoryLoot(inventoryItems, moneyToDrop);
    }

    [ServerRpc(RequireOwnership = false)]
    void RpcSpawnPlayerBikeOnServer(NetworkConnection playerConnection)
    {
        SpawnPlayerBikeOnServer(playerConnection);
    }
    [Server]
    void SpawnPlayerBikeOnServer(NetworkConnection playerConnection)
    {
        var playerBike = Instantiate(defaultPlayerWaterbikerPrefab, Game.LocalPlayer.transform.position + Vector3.up * 5, Game.LocalPlayer.transform.rotation);
        ServerManager.Spawn(playerBike.gameObject, playerConnection);
        
        RpcSetPlayerDrivingOnClient(playerConnection, playerBike.gameObject);
    }
    
    [TargetRpc]
    void RpcSetPlayerDrivingOnClient(NetworkConnection playerConnection, GameObject bikeGameObject)
    {
        var veh = bikeGameObject.GetComponent<ControlledMachine>();
        Game.LocalPlayer.VehicleControls.SaveOwnVehicle(veh);
        //Game.LocalPlayer.VehicleControls.RequestVehicleAction(veh);
    }
    
    
    IEnumerator SpawnAroundPlayer()
    {
        while (Game._instance == null || Game.LocalPlayer == null)
        {
            Debug.Log("SpawnAroundPlayer wait");
            yield return new WaitForSeconds(1);
        }

        // SPAWN BIKE FOR THIS PLAYER
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
            yield return new WaitForSeconds(cooldown);
            
            if (IsLocalPlayerNearContentSpawnBlocker())
                continue;
            
            if (base.IsHost)
            {
                SpawnRedUnitAroundRandomPlayer();
            }

            /*
            if (Game.LocalPlayer != null && Game.LocalPlayer.Health.health > 0)
            {
                Debug.Log("SpawnAroundPlayer SpawnLootAroundPlayer");
                SpawnLootAroundLocalPlayer();
            }*/

        }
    }

    // spawn mob on navmesh if player is close to island
    // else spawn mob in bike in the ocean 
    [SerializeField] private float islandDistanceSpawn = 200;
    [Server]
    void SpawnRedUnitAroundRandomPlayer()
    {
        if (UnitsManager.Instance.MobsInGame.Count > maxMobsAlive)
            return;
        var players = Game._instance.PlayersInGame;
        var randomPlayer = players[Random.Range(0, players.Count)];

        var distance = IslandSpawner.Instance.GetDistanceToClosestIsland(randomPlayer.transform.position);
        Vector3 pos;
        if (distance <= islandDistanceSpawn)
        {
            pos = PosAroundPosition(randomPlayer.MainCamera.transform.position, islandDistanceSpawn);   
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
    public void SpawnBossOnIsland(Island island, Vector3 spawnPos)
    {
        Vector3 pos = spawnPos;
        var currentLevel = ProgressionManager.Instance.CurrentLevel;
        switch (currentLevel.spawnBossType)
        {
            case ProcLevelData.SpawnBossType.Building:
                pos = island.TileBuildingGenerator.GetRandomPosInsideLastLevel();
                break;
            case ProcLevelData.SpawnBossType.Island:
                pos = PosAroundPosition(Game._instance.PlayersInGame[0].transform.position, islandDistanceSpawn); 
                break;
            case ProcLevelData.SpawnBossType.Ocean:
                pos = island.transform.position + Random.onUnitSphere * Random.Range(200, 300);
                break;
        }
        var unit =  Instantiate(currentLevel.boss, pos, Quaternion.identity, UnitsManager.Instance.SpawnRoot); // spawn only easy one for now
        island.AddIslandUnit(unit, true);
        ServerManager.Spawn(unit.gameObject);
    }
    
    [Server]
    public void SpawnEnemiesInBuilding(BuildingGenerator.Building building, Island island)
    {
        StartCoroutine(SpawnEnemiesInBuildingCoroutine(building, island));
    }

    #region Voxel Buildings

    [Server]
    public IEnumerator SpawnPropsInVoxelBuilding(List<VoxelBuildingFloor> floors)
    {
        yield return new WaitForSeconds(5);
        var propsPrefabs = IslandSpawner.Instance.GetClosestTileBuilding(floors[0].transform.position).PropsPrefabs;
        
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
    public IEnumerator SpawnEnemiesInVoxelBuilding(List<VoxelBuildingFloor> floors, Island island)
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
                
                if (island.IsCulled)
                    yield break;
                
                var unit =  Instantiate(UnitsManager.Instance.GetRandomRedUnit, pos, Quaternion.identity, UnitsManager.Instance.SpawnRoot); // spawn only easy one for now
                island.AddIslandUnit(unit);
                ServerManager.Spawn(unit.gameObject);
                yield return null;
            }
        }
    }
    

    #endregion
    
    [Server]
    IEnumerator SpawnEnemiesInBuildingCoroutine(BuildingGenerator.Building building, Island island)
    {
        var tilesForSpawns = new List<TileHealth>();

        while (island.GetTargetHavok < 1)
        {
            yield return null;
        }
        while (island && island.GetHavokFill < 1)
        {
            for (int i = 0; i < building.spawnedBuildingLevels.Count; i++)
            {
                var level = building.spawnedBuildingLevels[i];
                    
                tilesForSpawns.Clear();
                var tilesInsideTemp = new List<TileHealth>(level.tilesInside); 
                for (var index = tilesInsideTemp.Count - 1; index >= 0; index--)
                {
                    var tile = tilesInsideTemp[index];
                    if (tile == null)
                    {
                        tilesInsideTemp.RemoveAt(index);
                        continue;
                    }
                    tilesForSpawns.Add(tile);
                }

                for (int j = 0; j < level.unitsToSpawn.Count; j++)
                {
                    if (island.IsCulled)
                        yield break;
                    var randomPos = tilesForSpawns[Random.Range(0, tilesForSpawns.Count)].transform.position;
                    if (Random.value > 0.5f)
                    {
                        randomPos += Random.onUnitSphere * Random.Range(1, 100);
                        if (randomPos.y < 0)
                            randomPos.y *= -1;
                    }
                    var unit =  Instantiate(level.unitsToSpawn[j], randomPos, Quaternion.identity, UnitsManager.Instance.SpawnRoot); // spawn only easy one for now
                    island.AddIslandUnit(unit);
                    ServerManager.Spawn(unit.gameObject);
                    yield return null;
                }
                yield return null;
            }

            while (UnitsManager.Instance.MobsInGame.Count > maxMobsAlive)
            {
                yield return null;
            }
        }
    }
        
    [Server]
    void SpawnRedUnit(Vector3 pos)
    {
        var island = IslandSpawner.Instance.GetClosestIsland(pos);
        //pos = UnitsManager.Instance.SamplePos(pos);
        var unit =  Instantiate(UnitsManager.Instance.redTeamUnitPrefabs[Random.Range(0, UnitsManager.Instance.redTeamUnitPrefabs.Count)], pos, Quaternion.identity, UnitsManager.Instance.SpawnRoot); // spawn only easy one for now
        island.AddIslandUnit(unit);
        ServerManager.Spawn(unit.gameObject);
    }
    
    [Server]
    void SpawnRedUnitInBoat(Vector3 pos)
    {
        var unit =  Instantiate(UnitsManager.Instance.redTeamUnitPrefabs[Random.Range(0, UnitsManager.Instance.redTeamUnitPrefabs.Count)], pos, Quaternion.identity, UnitsManager.Instance.SpawnRoot); // spawn only easy one for now
        var boat = Instantiate(aiWaterBikes[Random.Range(0, aiWaterBikes.Count)], pos, Quaternion.identity, UnitsManager.Instance.SpawnRoot);
        
        var island = IslandSpawner.Instance.GetClosestIsland(pos);
        island.AddIslandUnit(unit);
        
        ServerManager.Spawn(unit.gameObject);
        ServerManager.Spawn(boat.gameObject);
        
        unit.aiVehicleControls.DriverSitOnServer(boat);
    }

    [Server]
    public void SpawnBoatForUnit(Unit unit)
    { 
        var boat = Instantiate(aiWaterBikes[Random.Range(0, aiWaterBikes.Count)], unit.transform.position, Quaternion.identity, UnitsManager.Instance.SpawnRoot);
        ServerManager.Spawn(boat.gameObject);
        unit.HealthController.aiVehicleControls.DriverSitOnServer(boat);
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
            pos = PosAroundPosition(Game.LocalPlayer.MainCamera.transform.position, islandDistanceSpawn);
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

    Vector3 PosAroundPosition(Vector3 initPos, float maxDistance)
    {
        initPos += Random.onUnitSphere * Random.Range(1, maxDistance);
        return initPos;
    }

    public void SetMaxAliveMobs(int amount)
    {
        maxMobsAlive = amount;
    }
    public int GetMaxAliveMobs()
    {
        return maxMobsAlive;
    }

    public void AddContentBlocker(ContentPlacerBlocker contentPlacerBlocker)
    {
        if (_contentPlacerBlockers.Contains(contentPlacerBlocker)) return;
        _contentPlacerBlockers.Add(contentPlacerBlocker);
    }
    public void RemoveContentBlocker(ContentPlacerBlocker contentPlacerBlocker)
    {
        if (_contentPlacerBlockers.Contains(contentPlacerBlocker) == false) return;
        _contentPlacerBlockers.Remove(contentPlacerBlocker);
    }

    bool IsLocalPlayerNearContentSpawnBlocker()
    {
        foreach (var contentPlacerBlocker in _contentPlacerBlockers)
        {
            var distance = Vector3.Distance(Game.LocalPlayer.Position, contentPlacerBlocker.transform.position);
            if (distance <= contentPlacerBlocker.BlockDistance)
                return true;
        }

        return false;
    }
    
    public InteractiveObject GetToolForSpawnOnLevel()
    {
        return toolsToSpawnOnBuildingLevels[Random.Range(0, toolsToSpawnOnBuildingLevels.Count)];
    }
}
