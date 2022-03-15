using System.Collections.Generic;
using MrPink.PlayerSystem;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;
    
    public Dictionary<Tool.ToolType, int> amountOfEachTool = new Dictionary<Tool.ToolType, int>();

    public WeaponController startingPistolWeapon;
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        SpawnPlayerWeapon(startingPistolWeapon, 0);
    }

    void SpawnPlayerWeapon(WeaponController weaponPrefab, int side) // 0- left, 1 - right
    {
        var wpn = Instantiate(weaponPrefab, Player.GameObject.transform.position, Quaternion.identity);
        switch (side)
        {
            case 0:
                PlayerWeaponControls.Instance.SetLeftWeapon(wpn);
                break;
            case 1:
                PlayerWeaponControls.Instance.SetRightWeapon(wpn);
                break;
        }
    }
    public void AddTool(Tool tool)
    {
        if (tool.tool == Tool.ToolType.DualWeilder)
            SpawnPlayerWeapon(startingPistolWeapon, 1);
            
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