using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MrPink.Tools;
using MrPink.WeaponsSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MrPink.PlayerSystem
{
    public class PlayerInventory : MonoBehaviour
    {
        // in the bag
        public List<InventoryItem> inventoryItems = new List<InventoryItem>();
        [SerializeField] private List<EquipmentSlot> _equipmentSlots;
        public List<EquipmentSlot> GetEquipmentSlots => _equipmentSlots;
        [Serializable] public class InventoryItem
        {
            public EquipmentSlot.Slot currentSlot;
            public ToolType _toolType;
            public List<ItemAction> ItemActions;
            public int usesLeft;
        }
        
        [Serializable] public class EquipmentSlot
        {
            public enum Slot
            {
                LeftHand,
                RightHand,
                Head,
                Body,
                Legs,
                Null
            }

            public Slot slot = Slot.Null;
            public InventoryItem equippedItem;
        }

        public enum ItemAction
        {
            ArmLeft, ArmRight, QuickSlot, Use
        }

        [SerializeField] private Tool defaultMeleeWeapon;
        [SerializeField] private List<Tool> startingTools;
        
        [SerializeField, AssetsOnly, Required]
        private WeaponController startingPistolWeapon;
        [SerializeField, AssetsOnly, Required]
        private WeaponController startingPistolLaserPointWeapon;
        [SerializeField, AssetsOnly, Required]
        private WeaponController shotterWeapon;
        [SerializeField, AssetsOnly, Required]
        private WeaponController defaultMeleeWeaponPrefab;

        [SerializeField, AssetsOnly, Required] 
        private WeaponController _startingSwordWeapon;

        public int GetCurrentSelectedIndex(ToolType toolType)
        {
            for (var index = 0; index < inventoryItems.Count; index++)
            {
                var toolAmount = inventoryItems[index];
                if (toolAmount._toolType == toolType)
                {
                    return index;
                }
            }

            return 0;
        }

        public void Init()
        {
            foreach (var startingTool in startingTools)
            {
                AddTool(startingTool);
            }

            CheckIfPlayerHasEmptyHands();

        }

        async void CheckIfPlayerHasEmptyHands()
        {
            while (leftWeapon == null || rightWeapon == null)
            {
                await UniTask.Delay(100);
                if (leftWeapon != null && rightWeapon != null)
                    return;
                
                SpawnFist();
            }
        }    
    
        // TODO стороны - через enum
        public void SpawnPlayerWeapon(InventoryItem inventoryItem, WeaponController weaponPrefab, int side = 0, bool forceHand = false) // 0- left, 1 - right
        {
            var wpn = Instantiate(weaponPrefab, Game.LocalPlayer.Position, Quaternion.identity);
            if (Game.LocalPlayer.Weapon.Hands[0].Weapon != null && forceHand == false)
                side = 1;
            
            switch (side)
            {
                case 0:
                    inventoryItem.currentSlot = EquipmentSlot.Slot.LeftHand;
                    ClearSlot(EquipmentSlot.Slot.LeftHand);
                    if (_equipmentSlots[0].equippedItem != null)
                        _equipmentSlots[0].equippedItem.currentSlot = EquipmentSlot.Slot.Null;
                    _equipmentSlots[0].equippedItem = inventoryItem;
                    Game.LocalPlayer.Weapon.SetWeapon(wpn, Hand.Left);
                    break;
                case 1:
                    inventoryItem.currentSlot = EquipmentSlot.Slot.RightHand;
                    ClearSlot(EquipmentSlot.Slot.RightHand);
                    if (_equipmentSlots[1].equippedItem != null)
                        _equipmentSlots[1].equippedItem.currentSlot = EquipmentSlot.Slot.Null;
                    _equipmentSlots[1].equippedItem = inventoryItem;
                    Game.LocalPlayer.Weapon.SetWeapon(wpn, Hand.Right);
                    break;
            }
        }

        public bool HasTool(ToolType toolType)
        {
            foreach (var toolAmount in inventoryItems)
            {
                if (toolAmount._toolType == toolType)
                    return toolAmount.usesLeft > 0;
            }

            return false;
        }

        public void ClearSlot(EquipmentSlot.Slot slot)
        {
            foreach (var inventoryItem in inventoryItems)
            {
                if (inventoryItem.currentSlot == slot)
                    inventoryItem.currentSlot = EquipmentSlot.Slot.Null;
            }
            foreach (var equipmentSlot in _equipmentSlots)
            {
                if (equipmentSlot.slot == slot)
                {
                    if (equipmentSlot.equippedItem != null)
                        equipmentSlot.equippedItem.currentSlot = EquipmentSlot.Slot.Null;
                    equipmentSlot.equippedItem = null;
                    if (slot == EquipmentSlot.Slot.LeftHand)
                        Game.LocalPlayer.Weapon.ClearHand(Hand.Left);
                    if (slot == EquipmentSlot.Slot.RightHand)
                        Game.LocalPlayer.Weapon.ClearHand(Hand.Right);
                    return;
                }
            }
        }

        public void EquipToolByUiButton(ToolType toolType, int usesLeft, List<ItemAction> itemActions, int handIndex)
        {
            if (toolType == ToolType.Knife)
                SpawnPlayerWeapon(GetInventoryItem(toolType, usesLeft, itemActions), _startingSwordWeapon, handIndex, true);
            if (toolType == ToolType.PistolLaserPoint)
                SpawnPlayerWeapon(GetInventoryItem(toolType, usesLeft, itemActions), startingPistolLaserPointWeapon, handIndex, true);
            if (toolType == ToolType.UziPistol)
                SpawnPlayerWeapon(GetInventoryItem(toolType, usesLeft, itemActions), startingPistolWeapon, handIndex, true);
            if (toolType == ToolType.Shotter)
                SpawnPlayerWeapon(GetInventoryItem(toolType, usesLeft, itemActions), shotterWeapon, handIndex, true);
            if (toolType == ToolType.Fist)
                SpawnPlayerWeapon(GetInventoryItem(toolType, usesLeft, itemActions),defaultMeleeWeaponPrefab, handIndex, true);
        }

        
        public static InventoryItem GetInventoryItem(ToolType tooltype, int usesLeft, List<ItemAction> itemActions)
        {
            InventoryItem newItem = new InventoryItem();
            newItem.usesLeft = usesLeft;
            newItem._toolType = tooltype;
            newItem.ItemActions = new List<ItemAction>(itemActions);
            return newItem;
        }
        InventoryItem GetInventoryItem(Tool tool)
        {
            InventoryItem newItem = new InventoryItem();
            newItem.usesLeft = tool.defaultUses;
            newItem._toolType = tool.tool;
            newItem.currentSlot = EquipmentSlot.Slot.Null;
            newItem.ItemActions = new List<ItemAction>(tool.InventoryItemActions);
            return newItem;
        }

        public void AddInventoryItems(List<InventoryItem> newInventoryItems)
        {
            foreach (var inventoryItem in newInventoryItems)
            {
                AddInventoryItem(inventoryItem);
            }
        }

        public void AddInventoryItem(InventoryItem newItem)
        {
            if (newItem._toolType == ToolType.Fist)
                SpawnPlayerWeapon(newItem, defaultMeleeWeaponPrefab);
            else
                inventoryItems.Add(newItem);
            
            if (newItem._toolType == ToolType.OneTimeShield)
                PlayerUi.Instance.AddShieldFeedback();
        }
        
        public void AddTool(Tool tool)
        {
            var newItem = GetInventoryItem(tool);
            
            if (tool.tool == ToolType.Fist)
                SpawnPlayerWeapon(newItem, defaultMeleeWeaponPrefab);
            else
                inventoryItems.Add(newItem);
            
            if (tool.tool == ToolType.OneTimeShield)
                PlayerUi.Instance.AddShieldFeedback();

            
            Game.LocalPlayer.ToolControls.SelectNextToolOnQuickSlots();
            Game.LocalPlayer.ToolControls.UpdateSelectedToolFeedback();
        }

        public void SpawnFist()
        {
            AddTool(defaultMeleeWeapon);
        }
    
        public int RemoveTool(ToolType tool, int amount = 1)
        {
            if (tool == ToolType.Null ||tool == ToolType.Fist)
                return -1;
            
            foreach (var toolAmount in inventoryItems)
            {
                if (toolAmount._toolType != tool)
                    continue;

                toolAmount.usesLeft -= amount;
                
                Debug.Log("REMOVE TOOL. Compare " + tool + " with " + toolAmount._toolType + "; Should be the same");
                if (toolAmount.usesLeft <= 0)
                    inventoryItems.Remove(toolAmount);
                return toolAmount.usesLeft;
            }

            return -1;
        }

    
        public bool CanFitTool(Tool tool)
        {
            /*
            foreach (var toolAmount in inventoryItems)
            {
                if (toolAmount._toolType != tool.tool)
                    continue;

                return toolAmount.amount < tool.maxAmount;
            }*/

            return true;
        }
    
        public int GetItemsAmount(ToolType toolType)
        {
            int amount = 0;
            foreach (var toolAmount in inventoryItems)
            {
                if (toolAmount._toolType != toolType)
                    continue;

                amount++;
            }

            return amount;
        }

        public void DropAllOnDeath()
        {
            var itemsToDrop = new List<InventoryItem>(inventoryItems);
            int moneyToDrop = ScoringSystem.Instance.CurrentScore;
            ScoringSystem.Instance.RemoveScore(moneyToDrop);
            ContentPlacer.Instance.SpawnPlayerLootContainer(itemsToDrop, moneyToDrop);
            
            for (int i = 0; i < Game.LocalPlayer.ToolControls.toolsProjectilesPrefabs.Count; i++)
            {
                var toolPrefab = Game.LocalPlayer.ToolControls.toolsProjectilesPrefabs[i];
                var amount = GetItemsAmount(toolPrefab.toolType);
                
                for (int j = 0; j < amount; j++)
                {
                    RemoveTool(toolPrefab.toolType);
                }
            }
            
            Game.LocalPlayer.Weapon.SetWeapon(null, Hand.Right);
            SetWeapon(null, Hand.Right);
            
            Game.LocalPlayer.Weapon.SetWeapon(null, Hand.Left);
            SetWeapon(null, Hand.Left);
            
            foreach (var toolAmount in inventoryItems)
            {
                toolAmount.usesLeft = 0;
            }
            inventoryItems.Clear();

            CheckIfPlayerHasEmptyHands();
        }

        WeaponController leftWeapon;
        WeaponController rightWeapon;
        public void SetWeapon(WeaponController weapon, Hand hand)
        {
            if (hand == Hand.Left)
            {
                if (leftWeapon != null)
                {
                    Destroy(leftWeapon.gameObject);
                }
                leftWeapon = weapon;
            }
            
            if (hand == Hand.Right)
            {
                if (rightWeapon != null)
                {
                    Destroy(rightWeapon.gameObject);
                }
                rightWeapon = weapon;
            }
        }
    }
}