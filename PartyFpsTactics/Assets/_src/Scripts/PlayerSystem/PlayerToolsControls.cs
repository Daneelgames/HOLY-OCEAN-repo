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
        public static PlayerToolsControls Instance;
        // TODO подрубить UnityDictionary, сделать UnityDictionary<Enum, ProjectileController>
        // 0 - spycam; 1 - ladder; 2 - fragGrenade
        public List<ProjectileController> toolsProjectilesPrefabs;
        [SerializeField] List<PlayerInventory.InventoryItem> toolsInQuickSlots = new List<PlayerInventory.InventoryItem>();

        [Header("TOOL SELECTED IN THE LIST OF PREFABS")] 
        [SerializeField] [ReadOnly] private int selectedQuickSlotIndex = -1;
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

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
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
            
            SelectNextToolOnQuickSlots();
            UpdateSelectedToolFeedback();
            while (true)
            {
                yield return null;

                for (int i = 0; i < spawnedToolFeedbacks.Count; i++)
                {
                    if (i >= toolsInQuickSlots.Count)
                    {
                        spawnedToolFeedbacks[i].SetTool(ToolType.Null, 0, null);
                        spawnedToolFeedbacks[i].SetSelected(false);
                        continue;
                    }
                    
                    spawnedToolFeedbacks[i].SetTool(toolsInQuickSlots[i]._toolType, toolsInQuickSlots[i].usesLeft, GetToolSprite(toolsInQuickSlots[i]._toolType));
                    if (i != selectedQuickSlotIndex)
                        spawnedToolFeedbacks[i].SetSelected(false);
                    else
                        spawnedToolFeedbacks[i].SetSelected(true);
                }
            }
        }

        public void ToggleToQuickSlots(PlayerInventory.InventoryItem inventoryItem)
        {
            if (toolsInQuickSlots.Contains(inventoryItem))
            {
                toolsInQuickSlots.Remove(inventoryItem);

                if (toolsInQuickSlots.Count > 0)
                    selectedQuickSlotIndex = Mathf.Clamp(selectedQuickSlotIndex, 0, toolsInQuickSlots.Count - 1);
                else
                    selectedQuickSlotIndex = -1;
                return;
            }

            if (toolsInQuickSlots.Count == spawnedToolFeedbacks.Count)
            {
                ToggleToQuickSlots(toolsInQuickSlots[0]); // remove first before adding another one
            }
            toolsInQuickSlots.Add(inventoryItem);
            if (selectedQuickSlotIndex == -1)
                selectedQuickSlotIndex = toolsInQuickSlots.Count-1;
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
            if ((Shop.Instance && Shop.Instance.IsActive)||
                (PlayerInventoryUI.Instance && PlayerInventoryUI.Instance.IsActive))
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
                SelectNextToolOnQuickSlots();
            }
        
            if (Input.GetKeyDown(KeyCode.F))
            { 
                if (selectedQuickSlotIndex < 0)
                    return;
                if (Game.LocalPlayer.Inventory.GetItemsAmount(toolsInQuickSlots[selectedQuickSlotIndex]._toolType) <= 0)
                {
                    return;
                }
            
                // throw selected
                SpawnToolPrefab(GetToolPrefab(toolsInQuickSlots[selectedQuickSlotIndex]._toolType));
            }
        }

        ProjectileController GetToolPrefab(ToolType type)
        {
            foreach (var prefab in toolsProjectilesPrefabs)
            {
                if (prefab.toolType == type)
                {
                    return prefab;
                }
            }

            return null;
        }

        public void UseTool(ToolType toolType)
        {
            var prefab = GetToolPrefab(toolType);
            if (prefab == null)
            {
                Debug.LogError("TRYING TO SPAWN NULL PREFAB");
                return;
            }
            SpawnToolPrefab(prefab);
        }

        void SpawnToolPrefab(ProjectileController prefab)
        {
            var newTool = Instantiate(prefab);
            newTool.transform.position = Game.LocalPlayer.Movement.headTransform.position;
            newTool.transform.rotation = Game.LocalPlayer.MainCamera.transform.rotation;
            newTool.Init(Game.LocalPlayer.Health, DamageSource.Player, null);
            var resultAmount = Game.LocalPlayer.Inventory.RemoveTool(prefab.toolType);
            if (resultAmount < 1)
            {
                foreach (var item in toolsInQuickSlots)
                {
                    if (item._toolType == prefab.toolType)
                    {
                        ToggleToQuickSlots(item);
                        break;
                    }
                }
            }
            UpdateSelectedToolFeedback();
        }
        
        public void SelectNextToolOnQuickSlots()
        {
            if (toolsInQuickSlots.Count < 1)
            {
                selectedQuickSlotIndex = -1;
                return;
            }
            
            selectedQuickSlotIndex++;
            if (selectedQuickSlotIndex >= toolsInQuickSlots.Count)
                selectedQuickSlotIndex = 0;
            
            UpdateSelectedToolFeedback();
        }
        
        public void UpdateSelectedToolFeedback()
        {
            if (Game.LocalPlayer == null)
                return;
            if (selectedQuickSlotIndex < 0)
                return;
            
            selectedToolAmount = Game.LocalPlayer.Inventory.GetItemsAmount(toolsInQuickSlots[selectedQuickSlotIndex]._toolType);
            
            /*
            if (selectedToolAmount <= 0)
                toolsControlsHintText.text = String.Empty;
            else
                 toolsControlsHintText.text = "F to throw " + toolsProjectilesPrefabs[selectedTool].name + ". Amount: " + selectedToolAmount;*/
        }
    }
}