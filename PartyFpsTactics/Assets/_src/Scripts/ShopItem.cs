using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    public Button button;
    public Image raycastedSprite;
    public Text itemName;
    public Text itemAmount;

    public void HideItem()
    {
        raycastedSprite.enabled = false;
        itemName.gameObject.SetActive(false);
    }

    public void ShowItem(string newText, int amount = -1)
    {
        itemName.text = newText;
        
        raycastedSprite.enabled = true;
        if (amount > 0)
            itemAmount.text = amount.ToString();
        else
            itemAmount.text = String.Empty;
        itemName.gameObject.SetActive(true);
    }
}
