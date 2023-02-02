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
        public List<ToolAmount> amountOfEachTool = new List<ToolAmount>();

        [Serializable]
        public class ToolAmount
        {
            public ToolType _toolType;
            public int amount;
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
            for (var index = 0; index < amountOfEachTool.Count; index++)
            {
                var toolAmount = amountOfEachTool[index];
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
            foreach (var toolAmount in amountOfEachTool)
            {
                if (toolAmount._toolType == toolType)
                    return toolAmount.amount > 0;
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

            foreach (var toolAmount in amountOfEachTool)
            {
                if (toolAmount._toolType != tool.tool)
                    continue;
             
                toolAmount.amount++;
                Game.LocalPlayer.ToolControls.UpdateSelectedToolFeedback();
                return;
            }

            ToolAmount newAmount = new ToolAmount();
            newAmount.amount = 1;
            newAmount._toolType = tool.tool;
            amountOfEachTool.Add(newAmount);
            
            Game.LocalPlayer.ToolControls.SelectNextTool();
            Game.LocalPlayer.ToolControls.UpdateSelectedToolFeedback();
        }
    
        public void RemoveTool(ToolType tool)
        {
            foreach (var toolAmount in amountOfEachTool)
            {
                if (toolAmount._toolType != tool)
                    continue;

                toolAmount.amount--;
                if (toolAmount.amount <= 0)
                    amountOfEachTool.Remove(toolAmount);
                return;
            }
        }
    
        public bool CanFitTool(Tool tool)
        {
            foreach (var toolAmount in amountOfEachTool)
            {
                if (toolAmount._toolType != tool.tool)
                    continue;

                return toolAmount.amount < tool.maxAmount;
            }

            return true;
        }
    
        public int GetAmount(ToolType toolType)
        {
            foreach (var toolAmount in amountOfEachTool)
            {
                if (toolAmount._toolType != toolType)
                    continue;

                return toolAmount.amount;
            }

            return 0;
        }

        public void DropAll()
        {
            for (int i = 0; i < Game.LocalPlayer.ToolControls.toolsProjectilesPrefabs.Count; i++)
            {
                var toolPrefab = Game.LocalPlayer.ToolControls.toolsProjectilesPrefabs[i];
                var amount = GetAmount(toolPrefab.toolType);
                
                for (int j = 0; j < amount; j++)
                {
                    RemoveTool(toolPrefab.toolType);
                }
            }
            
            Game.LocalPlayer.Weapon.SetWeapon(null, Hand.Right);
            SetWeapon(null, Hand.Right);
            
            Game.LocalPlayer.Weapon.SetWeapon(null, Hand.Left);
            SetWeapon(null, Hand.Left);
            
            foreach (var toolAmount in amountOfEachTool)
            {
                toolAmount.amount = 0;
            }
            amountOfEachTool.Clear();

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