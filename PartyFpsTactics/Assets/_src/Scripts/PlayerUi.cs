using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUi : MonoBehaviour
{
    public static PlayerUi Instance;
    public Image enemyMarkPrefab;
    public Transform canvas;

    public Dictionary<HealthController, Image> markedEnemies = new Dictionary<HealthController, Image>();
    public Vector3 enemyMarkerOffset;
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
    public void UnmarkEnemy(HealthController enemy)
    {
        // store his position
        if (markedEnemies.ContainsKey(enemy))
        {
            Destroy(markedEnemies[enemy].gameObject);  // this destroys MARK
            markedEnemies.Remove(enemy); 
        }
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
                enemy.Value.transform.position = OnScreenPosition(enemy.Value, enemy.Key.visibilityTrigger.transform.position);

                /*
                var worldPosition = enemy.Key.visibilityTrigger.transform.position;
                Vector3 screenPoint = PlayerMovement.Instance.MainCam.WorldToViewportPoint(worldPosition);
                
                bool onScreen = screenPoint.z > 0 && screenPoint.x > 0 && screenPoint.x < 1 && screenPoint.y > 0 && screenPoint.y < 1;
                
                if (onScreen)
                {
                    enemy.Value.gameObject.SetActive(true);
                    enemy.Value.transform.position = screenPoint;
                }
                else
                {
                    enemy.Value.gameObject.SetActive(false);
                }
                */
            }

            foreach (var unit in unitsToRemove)
            {
                Destroy(markedEnemies[unit].gameObject); // this destroys MARK
                markedEnemies.Remove(unit);
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    Vector3 OnScreenPosition(Image marker, Vector3 targetPos)
    {
        // Giving limits to the icon so it sticks on the screen
        // Below calculations witht the assumption that the icon anchor point is in the middle
        // Minimum X position: half of the icon width
        float minX = marker.GetPixelAdjustedRect().width / 2;
        // Maximum X position: screen width - half of the icon width
        float maxX = Screen.width - minX;

        // Minimum Y position: half of the height
        float minY = marker.GetPixelAdjustedRect().height / 2;
        // Maximum Y position: screen height - half of the icon height
        float maxY = Screen.height - minY;

        // Temporary variable to store the converted position from 3D world point to 2D screen point
        Vector2 pos = Camera.main.WorldToScreenPoint(targetPos + enemyMarkerOffset);

        // Check if the target is behind us, to only show the icon once the target is in front
        if(Vector3.Dot((targetPos - transform.position), transform.forward) < 0)
        {
            // Check if the target is on the left side of the screen
            if(pos.x < Screen.width / 2)
            {
                // Place it on the right (Since it's behind the player, it's the opposite)
                pos.x = maxX;
            }
            else
            {
                // Place it on the left side
                pos.x = minX;
            }
        }

        // Limit the X and Y positions
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        // Update the marker's position
        return pos;
        // Change the meter text to the distance with the meter unit 'm'
        //meter.text = ((int)Vector3.Distance(targetPos, transform.position)).ToString() + "m";
    }
}