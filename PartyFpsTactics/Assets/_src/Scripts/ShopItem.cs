using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.PlayerSystem;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    public Button button;
    public Image raycastedSprite;
    public Text itemName;
    public Text itemAmount;

    private PlayerInventory.InventoryItem currentInventoryItem;
    public PlayerInventory.InventoryItem CurrentInventoryItem => currentInventoryItem;

    public void HideItem()
    {
        raycastedSprite.enabled = false;
        itemName.gameObject.SetActive(false);
        itemAmount.gameObject.SetActive(false);
    }

    public void ShowItem(PlayerInventory.InventoryItem inventoryItem)
    {
        currentInventoryItem = inventoryItem;
        itemName.text = inventoryItem._toolType.ToString();
        
        raycastedSprite.enabled = true;
        if (inventoryItem.usesLeft > 0)
            itemAmount.text = inventoryItem.usesLeft.ToString();
        else
            itemAmount.text = String.Empty;
        itemName.gameObject.SetActive(true);
        itemAmount.gameObject.SetActive(true);
    }
}
