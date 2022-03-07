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

        if (Input.GetKeyDown(KeyCode.E))
        {
            int i = selectedTool + 1;
            while (true)
            {
                if (i >= toolsPrefabs.Count)
                    i = 0;
                
                if (PlayerInventory.Instance.GetAmount(toolsPrefabs[i].toolType) > 0)
                {
                    selectedTool = i;
                    break;
                }
                
                if (i == selectedTool)
                    break;
                i++;
            }
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            int i = selectedTool - 1;
            while (true)
            {
                if (i < 0)
                    i = toolsPrefabs.Count - 1;
                
                if (PlayerInventory.Instance.GetAmount(toolsPrefabs[i].toolType) > 0)
                {
                    selectedTool = i;
                    break;
                }
                
                if (i == selectedTool)
                    break;
                i--;
            }
        }
        
        if (Input.GetKeyDown(KeyCode.F))
        { 
            if (PlayerInventory.Instance.GetAmount(toolsPrefabs[selectedTool].toolType) <= 0)
            {
                return;
            }
            
            // throw selected
            var newTool = Instantiate(toolsPrefabs[selectedTool]);
            newTool.transform.position = PlayerMovement.Instance.headTransform.position;
            newTool.transform.rotation = PlayerMovement.Instance.MainCam.transform.rotation;
            newTool.Init(PlayerMovement.Instance.hc);
            PlayerInventory.Instance.RemoveTool(toolsPrefabs[selectedTool].toolType);
        }
    }
}