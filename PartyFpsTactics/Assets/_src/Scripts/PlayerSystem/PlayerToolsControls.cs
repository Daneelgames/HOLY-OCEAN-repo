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
        [SerializeField] private List<ToolSprite> _toolSprites;
        [Serializable]
        class ToolSprite
        {
            public ToolType ToolType;
            public Sprite Sprite;
        }
        
        void Start()
        {
            StartCoroutine(UpdateToolBar());
        }

        IEnumerator UpdateToolBar()
        {
            while (Game._instance == null || Game.LocalPlayer == null)
            {
                yield return null;
            }
            
            SelectNextTool();
            UpdateSelectedToolFeedback();
            while (true)
            {
                yield return null;

                var toolsInInventory = Game.LocalPlayer.Inventory.amountOfEachTool;
                for (int i = 0; i < spawnedToolFeedbacks.Count; i++)
                {
                    if (i >= toolsInInventory.Count)
                    {
                        spawnedToolFeedbacks[i].SetTool(ToolType.Null, 0, null);
                        spawnedToolFeedbacks[i].SetSelected(false);
                        continue;
                    }
                    
                    spawnedToolFeedbacks[i].SetTool(toolsInInventory[i]._toolType, toolsInInventory[i].amount, GetToolSprite(toolsInInventory[i]._toolType));
                    if (i != selectedToolInInventorySlot)
                        spawnedToolFeedbacks[i].SetSelected(false);
                    else
                        spawnedToolFeedbacks[i].SetSelected(true);
                }
            }
        }

        Sprite GetToolSprite(ToolType tool)
        {
            foreach (var toolSprite in _toolSprites)
            {
                if (toolSprite.ToolType == tool)
                    return toolSprite.Sprite;
            }

            return null;
        }
        
        private void Update()
        {
            if (Shop.Instance && Shop.Instance.IsActive)
                return;
            if (Game.LocalPlayer == null)
                return;
            if (Game.Flags.IsPlayerInputBlocked)
                return;
        
            if (Game.LocalPlayer.Health.health <= 0)
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
                if (Game.LocalPlayer.Inventory.GetAmount(toolsProjectilesPrefabs[selectedToolInListOfPrefabs].toolType) <= 0)
                {
                    return;
                }
            
                // throw selected
                var newTool = Instantiate(toolsProjectilesPrefabs[selectedToolInListOfPrefabs]);
                newTool.transform.position = Game.LocalPlayer.Movement.headTransform.position;
                newTool.transform.rotation = Game.LocalPlayer.MainCamera.transform.rotation;
                newTool.Init(Game.LocalPlayer.Health, DamageSource.Player, null);
                Game.LocalPlayer.Inventory.RemoveTool(toolsProjectilesPrefabs[selectedToolInListOfPrefabs].toolType);
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

                var amount = Game.LocalPlayer.Inventory.GetAmount(toolsProjectilesPrefabs[i].toolType); 
                if (amount > 0)
                {
                    selectedToolInListOfPrefabs = i;
                    selectedToolInInventorySlot = Game.LocalPlayer.Inventory.GetCurrentSelectedIndex(toolsProjectilesPrefabs[i].toolType);
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
            selectedToolAmount = Game.LocalPlayer.Inventory.GetAmount(toolsProjectilesPrefabs[selectedToolInListOfPrefabs].toolType);
            
            /*
            if (selectedToolAmount <= 0)
                toolsControlsHintText.text = String.Empty;
            else
                 toolsControlsHintText.text = "F to throw " + toolsProjectilesPrefabs[selectedTool].name + ". Amount: " + selectedToolAmount;*/
        }
    }
}