using System;
using System.Collections.Generic;
using MrPink.PlayerSystem;
using MrPink.WeaponsSystem;
using UnityEngine;

namespace MrPink.Tools
{
    [CreateAssetMenu(fileName = "ToolData", menuName = "ScriptableObjects/ToolData", order = 1)]
    public class Tool : ScriptableObject
    {
        public PlayerInventory.EquipmentSlot.Slot equipmentSlot = PlayerInventory.EquipmentSlot.Slot.Null;

        public WeaponController WeaponPrefab;
        public WeaponController PassiveToolPrefab;
        public ToolType tool;
        public int defaultUses = 1;
        public int baseCost = 1000;

        public string toolName = "Name";
        public string toolDescription = "Description";
    }
}