using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using MrPink;
using MrPink.PlayerSystem;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerInventoryUI : MonoBehaviour
{
    public static PlayerInventoryUI Instance;
    [SerializeField] private List<ShopItem> inventoryItems;
    [ReadOnly][SerializeField] private List<PlayerInventory.ToolAmount> toolAmounts;
    
    public Animator canvasAnim;
    private bool isActive = false;
    public bool IsActive
    {
        get { return isActive; }
        set { isActive = value; }
    }
    private void Awake()
    {
        Instance = this;
    }

    [Button]
    public void ShowInventory()
    {
        ScoringSystem.Instance.UpdateScore();
        canvasAnim.gameObject.SetActive(true);
        IsActive = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        toolAmounts = new List<PlayerInventory.ToolAmount>(Game.LocalPlayer.Inventory.amountOfEachTool);   
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (i >= toolAmounts.Count)
            {
                inventoryItems[i].HideItem();
                continue;
            }
            
            inventoryItems[i].ShowItem(toolAmounts[i]._toolType.ToString(), toolAmounts[i].amount);
        }
    }
    
    [Button]
    public void HideInventory()
    {
        if (Game.LocalPlayer == null) return;
            
        Debug.Log("CloseShop");
        canvasAnim.gameObject.SetActive(false);
        IsActive = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
