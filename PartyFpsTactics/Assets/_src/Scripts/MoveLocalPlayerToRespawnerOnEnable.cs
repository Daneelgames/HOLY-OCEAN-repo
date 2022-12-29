using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Component.Spawning;
using MrPink;
using UnityEngine;

public class MoveLocalPlayerToRespawnerOnEnable : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CatchLocalPlayer());
    }

    IEnumerator CatchLocalPlayer()
    {
        while (Game._instance == null || Game.LocalPlayer == null)
        {
            yield return null;
        }

        var spawner = InstanceFinder.NetworkManager.gameObject.GetComponent<PlayerSpawner>(); 
        while (spawner == null)
        {
            Debug.Log("RespawnPlayerOverTime WAIT PlayerSpawner.Instance == null RESPAWN");
            spawner = InstanceFinder.NetworkManager.gameObject.GetComponent<PlayerSpawner>();
            yield return null;
        }
        
        StartCoroutine(Game.LocalPlayer.Movement.TeleportToPosition(spawner.Spawns[Random.Range(0, spawner.Spawns.Length)].position));
        foreach (var player in Game._instance.PlayersInGame)
        {
            player.Health.Resurrect();   
        }
    }
}
