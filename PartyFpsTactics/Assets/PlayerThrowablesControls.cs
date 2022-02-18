using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerThrowablesControls : MonoBehaviour
{
    // 0 - spycam; 1 - ladder; 2 - fragGrenade
    public List<ProjectileController> toolsPrefabs;
    public int selectedTool = 0;

    private void Update()
    {
        if (Shop.Instance.IsActive)
            return;

        if (!LevelGenerator.Instance.levelIsReady)
            return;
        
        if (PlayerMovement.Instance.hc.health <= 0)
            return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            // throw selected 
            var newTool = Instantiate(toolsPrefabs[selectedTool]);
            newTool.transform.position = PlayerMovement.Instance.headTransform.position;
            newTool.transform.rotation = PlayerMovement.Instance.MainCam.transform.rotation;
            newTool.Init(PlayerMovement.Instance.hc);
        }
    }
}