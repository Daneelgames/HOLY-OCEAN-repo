using System;
using System.Collections.Generic;
using MrPink.Tools;
using MrPink.WeaponsSystem;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MrPink.PlayerSystem
{
    public class PlayerInventory : MonoBehaviour
    {
        public List<InventoryItem> inventoryItems = new List<InventoryItem>();

        [Serializable]
        public class InventoryItem
        {
            public ToolType _toolType;
            public int usesLeft;
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
            AddDefaultMeleeWeapon();
            foreach (var startingTool in startingTools)
            {
                AddTool(startingTool);
            }
        }

        void AddDefaultMeleeWeapon()
        {
            for (int i = 0; i < 2; i++)
            {
                AddTool(defaultMeleeWeapon);
            }
        }
    
    
        // TODO стороны - через enum
        public void SpawnPlayerWeapon(WeaponController weaponPrefab, int side = 0, bool forceHand = false) // 0- left, 1 - right
        {
            var wpn = Instantiate(weaponPrefab, Game.LocalPlayer.Position, Quaternion.identity);
            
            if (Game.LocalPlayer.Weapon.Hands[0].Weapon != null && forceHand == false)
                side = 1;
            
            switch (side)
            {
                case 0:
                    Game.LocalPlayer.Weapon.SetWeapon(wpn, Hand.Left);
                    break;
                case 1:
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
        
        public void AddTool(Tool tool)
        {
            if (tool.tool == ToolType.Knife)
                SpawnPlayerWeapon(_startingSwordWeapon);
            if (tool.tool == ToolType.PistolLaserPoint)
                SpawnPlayerWeapon(startingPistolLaserPointWeapon);
            if (tool.tool == ToolType.DualWeilder)
                SpawnPlayerWeapon(startingPistolWeapon);
            if (tool.tool == ToolType.Shotter)
                SpawnPlayerWeapon(shotterWeapon);
            if (tool.tool == ToolType.Fist)
                SpawnPlayerWeapon(defaultMeleeWeaponPrefab);
            
            if (tool.tool == ToolType.OneTimeShield)
                PlayerUi.Instance.AddShieldFeedback();

            InventoryItem newItem = new InventoryItem();
            newItem.usesLeft = tool.defaultUses;
            newItem._toolType = tool.tool;
            inventoryItems.Add(newItem);
            
            Game.LocalPlayer.ToolControls.SelectNextTool();
            Game.LocalPlayer.ToolControls.UpdateSelectedToolFeedback();
        }

        public void SpawnFist()
        {
            AddTool(defaultMeleeWeapon);
        }
    
        public int RemoveTool(ToolType tool, int amount = 1)
        {
            Debug.Log("DAMAGE DURABILITY 1");
            if (tool == ToolType.Null ||tool == ToolType.Fist)
                return -1;
            
            Debug.Log("DAMAGE DURABILITY 1,1");
            foreach (var toolAmount in inventoryItems)
            {
                if (toolAmount._toolType != tool)
                    continue;

                toolAmount.usesLeft -= amount;
                if (toolAmount.usesLeft <= 0)
                    inventoryItems.Remove(toolAmount);
                Debug.Log("DAMAGE DURABILITY 2");
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

        public void DropAll()
        {
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

            AddDefaultMeleeWeapon();
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