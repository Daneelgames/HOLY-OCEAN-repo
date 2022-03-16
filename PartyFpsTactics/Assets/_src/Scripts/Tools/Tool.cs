using System;
using UnityEngine;

namespace MrPink.Tools
{
    [Serializable]
    public partial class Tool
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