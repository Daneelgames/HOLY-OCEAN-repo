using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUi : MonoBehaviour
{
    public static PlayerUi Instance;
    public GameObject enemyMarkPrefab;
    public Transform canvas;

    public Dictionary<HealthController, GameObject> markedEnemies = new Dictionary<HealthController, GameObject>();
    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        
    }

    public void MarkEnemy(HealthController enemy)
    {
        // store his position
        var newMark = Instantiate(enemyMarkPrefab, canvas);
        markedEnemies.Add(enemy, newMark);
    }

    IEnumerator Start()
    {
        while (true)
        {
            List<HealthController> unitsToRemove = new List<HealthController>();
            foreach (var enemy in markedEnemies)
            {
                if (enemy.Key == null)
                    continue;

                if (enemy.Key.health <= 0)
                {
                    unitsToRemove.Add(enemy.Key);
                }

                var worldPosition = enemy.Key.visibilityTrigger.transform.position;
                Vector3 screenPoint = PlayerMovement.Instance.MainCam.WorldToViewportPoint(worldPosition);
                
                bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
                
                if (onScreen)
                {
                    enemy.Value.SetActive(true);
                    enemy.Value.transform.position = screenPoint;
                }
                else
                {
                    enemy.Value.SetActive(false);
                }
            }

            foreach (var unit in unitsToRemove)
            {
                Destroy(markedEnemies[unit]); // this destroys MARK
                markedEnemies.Remove(unit);
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}