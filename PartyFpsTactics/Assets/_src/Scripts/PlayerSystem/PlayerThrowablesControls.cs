using System;
using System.Collections.Generic;
using MrPink.Health;
using MrPink.WeaponsSystem;
using UnityEngine;
using UnityEngine.UI;

namespace MrPink.PlayerSystem
{
    public class PlayerThrowablesControls : MonoBehaviour
    {
        // TODO подрубить UnityDictionary, сделать UnityDictionary<Enum, ProjectileController>
        // 0 - spycam; 1 - ladder; 2 - fragGrenade
        public List<ProjectileController> toolsPrefabs;
        public int selectedTool = 0;
        public int selectedToolAmount = 0;
        public Text toolsControlsHintText;

        public void Init()
        {
            SelectNextThrowable();
            UpdateSelectedToolFeedback();
        }

        private void Update()
        {
            if (Shop.Instance && Shop.Instance.IsActive)
                return;

            if (!LevelGenerator.Instance.levelIsReady)
                return;
        
            if (Game.Player.Health.health <= 0)
                return;
            
            // TODO роутить управление централизованно
            
            if (Input.GetKeyDown(KeyCode.Q))
            {
                SelectNextThrowable();
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
                if (Game.Player.Inventory.GetAmount(toolsPrefabs[selectedTool].toolType) <= 0)
                {
                    return;
                }
            
                // throw selected
                var newTool = Instantiate(toolsPrefabs[selectedTool]);
                newTool.transform.position = Game.Player.Movement.headTransform.position;
                newTool.transform.rotation = Game.Player.MainCamera.transform.rotation;
                newTool.Init(Game.Player.Health, DamageSource.Player);
                Game.Player.Inventory.RemoveTool(toolsPrefabs[selectedTool].toolType);
                UpdateSelectedToolFeedback();
            }
        }

        void SelectNextThrowable()
        {
            int i = selectedTool + 1;
            while (true)
            {
                if (i >= toolsPrefabs.Count)
                    i = 0;

                var amount = Game.Player.Inventory.GetAmount(toolsPrefabs[i].toolType); 
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
        
        void UpdateSelectedToolFeedback()
        {
            selectedToolAmount = Game.Player.Inventory.GetAmount(toolsPrefabs[selectedTool].toolType);
            
            if (selectedToolAmount <= 0)
                toolsControlsHintText.text = String.Empty;
            else
                 toolsControlsHintText.text = "F to throw " + toolsPrefabs[selectedTool].name + ". Amount: " + selectedToolAmount;
        }
    }
}