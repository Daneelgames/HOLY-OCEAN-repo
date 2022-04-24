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
        public static PlayerInventory Instance;
        
        public Dictionary<ToolType, int> amountOfEachTool = new Dictionary<ToolType, int>();

        [SerializeField, AssetsOnly, Required]
        private WeaponController startingPistolWeapon;

        [SerializeField, AssetsOnly, Required] 
        private WeaponController _startingSwordWeapon;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (LevelGenerator.Instance == null)
                return;
            
            return;
            if (LevelGenerator.Instance.levelType == LevelGenerator.LevelType.Game)
            {
                SpawnPlayerWeapon(startingPistolWeapon, 0);
                SpawnPlayerWeapon(_startingSwordWeapon, 1);
            }
        }
    
    
        // TODO стороны - через enum
        public void SpawnPlayerWeapon(WeaponController weaponPrefab, int side = 0) // 0- left, 1 - right
        {
            var wpn = Instantiate(weaponPrefab, Game.Player.Position, Quaternion.identity);
            
            if (Game.Player.Weapon.Hands[0].Weapon != null)
                side = 1;
            
            switch (side)
            {
                case 0:
                    Game.Player.Weapon.SetWeapon(wpn, Hand.Left);
                    break;
                case 1:
                    Game.Player.Weapon.SetWeapon(wpn, Hand.Right);
                    break;
            }
        }

        public bool HasTool(ToolType toolType)
        {
            if (amountOfEachTool.ContainsKey(toolType) && amountOfEachTool[toolType] > 0)
                return true;

            return false;
        }
        
        public void AddTool(Tool tool)
        {
            if (tool.tool == ToolType.DualWeilder)
                SpawnPlayerWeapon(startingPistolWeapon, 1);
            
            if (tool.tool == ToolType.OneTimeShield)
                
                PlayerUi.Instance.AddShieldFeedback();
            
            if (amountOfEachTool.ContainsKey(tool.tool))
            {
                amountOfEachTool[tool.tool]++;
                Game.Player.ToolControls.UpdateSelectedToolFeedback();
                return;
            }   
            amountOfEachTool.Add(tool.tool, 1);
            
            Game.Player.ToolControls.SelectNextTool();
            Game.Player.ToolControls.UpdateSelectedToolFeedback();
        }
    
        public void RemoveTool(ToolType tool)
        {
            if (amountOfEachTool.ContainsKey(tool))
            {
                amountOfEachTool[tool]--;
                if (amountOfEachTool[tool] <= 0)
                    amountOfEachTool.Remove(tool);
            }   
        }
    
        public bool CanFitTool(Tool tool)
        {
            if (amountOfEachTool.ContainsKey(tool.tool))
            {
                if (amountOfEachTool[tool.tool] >= tool.maxAmount)
                    return false;
            }

            return true;
        }
    
        public int GetAmount(ToolType toolType)
        {
            if (amountOfEachTool.ContainsKey(toolType))
            {
                return amountOfEachTool[toolType];
            }

            return 0;
        }

        public void DropRandomTools()
        {
            for (int i = 0; i < Game.Player.ToolControls.toolsProjectilesPrefabs.Count; i++)
            {
                var toolPrefab = Game.Player.ToolControls.toolsProjectilesPrefabs[i];
                var amount = GetAmount(toolPrefab.toolType);
                if (amount > 0)
                {
                    int dropAmount = Random.Range(0, amount);
                    
                    for (int j = 0; j < dropAmount; j++)
                    {
                        RemoveTool(toolPrefab.toolType);
                    }
                }
            }
            
            if (rightWeapon != null)
            {
                Game.Player.Weapon.SetWeapon(null, Hand.Right);
                SetWeapon(null, Hand.Right);
                return;
            }

            if (leftWeapon != null)
                Game.Player.Weapon.SetWeapon(null, Hand.Left);

        }

        WeaponController leftWeapon;
        WeaponController rightWeapon;
        public void SetWeapon(WeaponController weapon, Hand hand)
        {
            if (hand == Hand.Left)
                leftWeapon = weapon;
            
            if (hand == Hand.Right)
                rightWeapon = weapon;
        }
    }
}