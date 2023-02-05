using System;
using System.Collections.Generic;
using MrPink.PlayerSystem;
using UnityEngine;

namespace MrPink.Tools
{
    [CreateAssetMenu(fileName = "ToolData", menuName = "ScriptableObjects/ToolData", order = 1)]
    public class Tool : ScriptableObject
    {
        public ToolType tool;
        public List<PlayerInventory.ItemAction> InventoryItemActions;
        public int defaultUses = 1;
        public int baseCost = 1000;

        public string toolName = "Name";
        public string toolDescription = "Description";
    }
}