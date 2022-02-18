using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;
    
    public Dictionary<Tool.ToolType, int> amountOfEachTool = new Dictionary<Tool.ToolType, int>();

    void Awake()
    {
        Instance = this;
    }

    public void AddTool(Tool tool)
    {
        if (amountOfEachTool.ContainsKey(tool.tool))
        {
            amountOfEachTool[tool.tool]++;
            return;
        }   
        amountOfEachTool.Add(tool.tool, 1);
    }
    public void RemoveTool(Tool.ToolType tool)
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
    public int GetAmount(Tool.ToolType toolType)
    {
        if (amountOfEachTool.ContainsKey(toolType))
        {
            return amountOfEachTool[toolType];
        }

        return 0;
    }
}