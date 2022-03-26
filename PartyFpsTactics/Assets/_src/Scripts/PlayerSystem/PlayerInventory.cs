using System.Collections.Generic;
using MrPink.Tools;
using MrPink.WeaponsSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink.PlayerSystem
{
    public class PlayerInventory : MonoBehaviour
    {
        public Dictionary<ToolType, int> amountOfEachTool = new Dictionary<ToolType, int>();

        [SerializeField, AssetsOnly, Required]
        private WeaponController startingPistolWeapon;

        [SerializeField, AssetsOnly, Required] 
        private WeaponController _startingSwordWeapon;

        private void Start()
        {
            if (LevelGenerator.Instance.levelType == LevelGenerator.LevelType.Game)
            {
                SpawnPlayerWeapon(startingPistolWeapon, 0);
                SpawnPlayerWeapon(_startingSwordWeapon, 1);
            }
        }
    
    
        // TODO стороны - через enum
        private void SpawnPlayerWeapon(WeaponController weaponPrefab, int side) // 0- left, 1 - right
        {
            var wpn = Instantiate(weaponPrefab, Player.GameObject.transform.position, Quaternion.identity);
            switch (side)
            {
                case 0:
                    Player.Weapon.SetWeapon(wpn, Hand.Left);
                    break;
                case 1:
                    Player.Weapon.SetWeapon(wpn, Hand.Right);
                    break;
            }
        }
        public void AddTool(Tool tool)
        {
            if (tool.tool == ToolType.DualWeilder)
                SpawnPlayerWeapon(startingPistolWeapon, 1);
            
            if (amountOfEachTool.ContainsKey(tool.tool))
            {
                amountOfEachTool[tool.tool]++;
                return;
            }   
            amountOfEachTool.Add(tool.tool, 1);
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

    }
}