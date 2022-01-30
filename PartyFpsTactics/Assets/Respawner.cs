using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Respawner : MonoBehaviour
{
    public float corpseShredderY = -50;
    public List<Transform> redRespawns;
    public int enemiesAmount = 10;
    public List<Transform> blueRespawns;
    public int alliesAmount = 3;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1);
        for (int i = 0; i < alliesAmount; i++)
        {
            GameManager.Instance.SpawnBlueUnit(blueRespawns[Random.Range(0, blueRespawns.Count)].position);
        }
        for (int i = 0; i < enemiesAmount; i++)
        {
            GameManager.Instance.SpawnRedUnit(redRespawns[Random.Range(0, redRespawns.Count)].position);
        }
    }

    void Update()
    {
        if (PlayerMovement.Instance.transform.position.y < corpseShredderY)
        {
            GameManager.Instance.Restart();
            return;
        }
        for (int i = 0; i < GameManager.Instance.ActiveHealthControllers.Count; i++)
        {
            var corpse = GameManager.Instance.ActiveHealthControllers[i];
            if (corpse.HumanVisualController && corpse.HumanVisualController.rigidbodies[0].transform.position.y < corpseShredderY)
            {
                switch (corpse.team)
                {
                    case HealthController.Team.Blue:
                        GameManager.Instance.SpawnBlueUnit(blueRespawns[Random.Range(0, blueRespawns.Count)].position);
                        break;
                    case HealthController.Team.Red:
                        var spawners = GetSpawnersInRange(redRespawns, PlayerMovement.Instance.transform.position, 10, 100);
                        GameManager.Instance.SpawnRedUnit(spawners[Random.Range(0, spawners.Count)].position);
                        break;
                }

                Destroy(corpse.gameObject);
            }
        }
    }

    public List<Transform> GetSpawnersInRange(List<Transform> spawners, Vector3 origin, float minRange, float maxRange)
    {
        List<Transform> temp = new List<Transform>(spawners);
        for (int i = temp.Count - 1; i >= 0; i--)
        {
            float dst = Vector3.Distance(origin, temp[i].position);
            if (dst < minRange || dst > maxRange)
                temp.RemoveAt(i);
        }

        return temp;
    }
}
