using System;
using UnityEngine;

namespace MrPink.Tools
{
    [CreateAssetMenu(fileName = "ToolData", menuName = "ScriptableObjects/ToolData", order = 1)]
    public class Tool : ScriptableObject
    {
        public ToolType tool;
        public int scoreCost = 1000;
        [Range(1, 99)]
        public int maxAmount = 1;

        public bool activeTool = false;
    
        public string toolName = "Name";
        public string toolDescription = "Description";
    }
}