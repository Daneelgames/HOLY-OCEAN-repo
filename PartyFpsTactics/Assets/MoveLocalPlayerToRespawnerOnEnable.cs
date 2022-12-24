using System.Collections;
using System.Collections.Generic;
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

        while (PlayerSpawner.Instance == null)
        {
            yield return null;
        }

        StartCoroutine(Game.LocalPlayer.Movement.TeleportToPosition(PlayerSpawner.Instance.Spawns[Random.Range(0, PlayerSpawner.Instance.Spawns.Length)].position));
    }
}
