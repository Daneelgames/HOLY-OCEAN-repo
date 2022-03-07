using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    public Button button;
    public Image raycastedSprite;
    public Text itemName;

    public void HideItem()
    {
        raycastedSprite.enabled = false;
        itemName.gameObject.SetActive(false);
    }

    public void ShowItem(string newText)
    {
        itemName.text = newText;
        
        raycastedSprite.enabled = true;
        itemName.gameObject.SetActive(true);
    }
}
