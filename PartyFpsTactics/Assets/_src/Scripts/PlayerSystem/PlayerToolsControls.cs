using System;
using System.Collections.Generic;
using MrPink.Health;
using MrPink.WeaponsSystem;
using UnityEngine;
using UnityEngine.UI;

namespace MrPink.PlayerSystem
{
    public class PlayerToolsControls : MonoBehaviour
    {
        // TODO подрубить UnityDictionary, сделать UnityDictionary<Enum, ProjectileController>
        // 0 - spycam; 1 - ladder; 2 - fragGrenade
        public List<ProjectileController> toolsProjectilesPrefabs;
        public int selectedTool = 0;
        public int selectedToolAmount = 0;
        public Text toolsControlsHintText;

        public void Init()
        {
            SelectNextTool();
            UpdateSelectedToolFeedback();
        }

        private void Update()
        {
            if (Shop.Instance && Shop.Instance.IsActive)
                return;

            if (Game.Flags.IsPlayerInputBlocked)
                return;
        
            if (Game.Player.Health.health <= 0)
                return;
            
            // TODO роутить управление централизованно
            
            if (Input.GetKeyDown(KeyCode.Q))
            {
                SelectNextTool();
            }
        
            /*
            if (Input.GetKeyDown(KeyCode.Z))
            {
                int i = selectedTool - 1;
                while (true)
                {
                    if (i < 0)
                        i = toolsPrefabs.Count - 1;
                
                    if (Game.Player.Inventory.GetAmount(toolsPrefabs[i].toolType) > 0)
                    {
                        selectedTool = i;
                        break;
                    }
                
                    if (i == selectedTool)
                        break;
                    i--;
                }

                UpdateSelectedToolFeedback();
            }*/
        
            if (Input.GetKeyDown(KeyCode.F))
            { 
                if (Game.Player.Inventory.GetAmount(toolsProjectilesPrefabs[selectedTool].toolType) <= 0)
                {
                    return;
                }
            
                // throw selected
                var newTool = Instantiate(toolsProjectilesPrefabs[selectedTool]);
                newTool.transform.position = Game.Player.Movement.headTransform.position;
                newTool.transform.rotation = Game.Player.MainCamera.transform.rotation;
                newTool.Init(Game.Player.Health, DamageSource.Player);
                Game.Player.Inventory.RemoveTool(toolsProjectilesPrefabs[selectedTool].toolType);
                UpdateSelectedToolFeedback();
            }
        }

        public void SelectNextTool()
        {
            int i = selectedTool + 1;
            while (true)
            {
                if (i >= toolsProjectilesPrefabs.Count)
                    i = 0;

                var amount = Game.Player.Inventory.GetAmount(toolsProjectilesPrefabs[i].toolType); 
                if (amount > 0)
                {
                    selectedTool = i;
                    break;
                }
                
                if (i == selectedTool)
                    break;
                i++;
            }

            UpdateSelectedToolFeedback();
        }
        
        public void UpdateSelectedToolFeedback()
        {
            selectedToolAmount = Game.Player.Inventory.GetAmount(toolsProjectilesPrefabs[selectedTool].toolType);
            
            if (selectedToolAmount <= 0)
                toolsControlsHintText.text = String.Empty;
            else
                 toolsControlsHintText.text = "F to throw " + toolsProjectilesPrefabs[selectedTool].name + ". Amount: " + selectedToolAmount;
        }
    }
}