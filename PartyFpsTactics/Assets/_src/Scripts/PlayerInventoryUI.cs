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
    [ReadOnly][SerializeField] private List<PlayerInventory.InventoryItem> toolAmounts;
    
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

    private void Start()
    {
        for (var index = 0; index < inventoryItems.Count; index++)
        {
            var t = inventoryItems[index];
            t.button.onClick.AddListener(() => SlotSelected(index));
        }
    }

    [Button]
    public void ShowInventory()
    {
        ScoringSystem.Instance.UpdateScore();
        canvasAnim.gameObject.SetActive(true);
        IsActive = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        toolAmounts = new List<PlayerInventory.InventoryItem>(Game.LocalPlayer.Inventory.inventoryItems);   
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (i >= toolAmounts.Count)
            {
                inventoryItems[i].HideItem();
                continue;
            }
            
            inventoryItems[i].ShowItem(toolAmounts[i]._toolType.ToString(), toolAmounts[i].usesLeft);
        }
    }

    void SlotSelected(int index)
    {
        if (index >= toolAmounts.Count)
            return;
        
        var toolAmount = toolAmounts[index];
        
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
