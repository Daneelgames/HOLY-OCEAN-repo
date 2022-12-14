using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using MrPink.Tools;
using MrPink.WeaponsSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace MrPink.PlayerSystem
{
    public class PlayerToolsControls : MonoBehaviour
    {
        // TODO подрубить UnityDictionary, сделать UnityDictionary<Enum, ProjectileController>
        // 0 - spycam; 1 - ladder; 2 - fragGrenade
        public List<ProjectileController> toolsProjectilesPrefabs;
        [Header("TOOL SELECTED IN THE LIST OF PREFABS")]
        public int selectedToolInListOfPrefabs = 0;
        public int selectedToolInInventorySlot = 0;
        public int selectedToolAmount = 0;

        [SerializeField] private List<ToolUiFeedback> spawnedToolFeedbacks;
        //public Text toolsControlsHintText;

        public void Init()
        {
            SelectNextTool();
            UpdateSelectedToolFeedback();

            StartCoroutine(UpdateToolBar());
        }

        IEnumerator UpdateToolBar()
        {
            while (true)
            {
                yield return null;

                var toolsInInventory = Game.Player.Inventory.amountOfEachTool;
                for (int i = 0; i < spawnedToolFeedbacks.Count; i++)
                {
                    if (i >= toolsInInventory.Count)
                    {
                        spawnedToolFeedbacks[i].SetTool(ToolType.Null, 0);
                        spawnedToolFeedbacks[i].SetSelected(false);
                        continue;
                    }
                    
                    spawnedToolFeedbacks[i].SetTool(toolsInInventory[i]._toolType, toolsInInventory[i].amount);
                    if (i != selectedToolInInventorySlot)
                        spawnedToolFeedbacks[i].SetSelected(false);
                    else
                        spawnedToolFeedbacks[i].SetSelected(true);
                }
            }
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
            
            if (Input.GetKeyDown(KeyCode.R))
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
                if (Game.Player.Inventory.GetAmount(toolsProjectilesPrefabs[selectedToolInListOfPrefabs].toolType) <= 0)
                {
                    return;
                }
            
                // throw selected
                var newTool = Instantiate(toolsProjectilesPrefabs[selectedToolInListOfPrefabs]);
                newTool.transform.position = Game.Player.Movement.headTransform.position;
                newTool.transform.rotation = Game.Player.MainCamera.transform.rotation;
                newTool.Init(Game.Player.Health, DamageSource.Player, null);
                Game.Player.Inventory.RemoveTool(toolsProjectilesPrefabs[selectedToolInListOfPrefabs].toolType);
                UpdateSelectedToolFeedback();
            }
        }

        public void SelectNextTool()
        {
            int i = selectedToolInListOfPrefabs + 1;
            while (true)
            {
                if (i >= toolsProjectilesPrefabs.Count)
                    i = 0;

                var amount = Game.Player.Inventory.GetAmount(toolsProjectilesPrefabs[i].toolType); 
                if (amount > 0)
                {
                    selectedToolInListOfPrefabs = i;
                    selectedToolInInventorySlot = Game.Player.Inventory.GetCurrentSelectedIndex(toolsProjectilesPrefabs[i].toolType);
                    break;
                }
                
                if (i == selectedToolInListOfPrefabs)
                    break;
                i++;
            }

            UpdateSelectedToolFeedback();
        }
        
        public void UpdateSelectedToolFeedback()
        {
            selectedToolAmount = Game.Player.Inventory.GetAmount(toolsProjectilesPrefabs[selectedToolInListOfPrefabs].toolType);
            
            /*
            if (selectedToolAmount <= 0)
                toolsControlsHintText.text = String.Empty;
            else
                 toolsControlsHintText.text = "F to throw " + toolsProjectilesPrefabs[selectedTool].name + ". Amount: " + selectedToolAmount;*/
        }
    }
}