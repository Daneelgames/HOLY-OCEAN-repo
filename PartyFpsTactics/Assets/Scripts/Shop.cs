using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    public static Shop Instance;
    
    public List<Tool> toolsList;
    public List<ShopItem> shopItemsIcons;
    public Animator canvasAnim;
    public Text selectedInfoNameText;
    public Text selectedInfoDescriptionText;
    public Image buyButtonImage;

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

    void Start()
    {
        OpenShop();
    }

    void OpenShop()
    {
        canvasAnim.gameObject.SetActive(true);
        IsActive = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        for (int i = 0; i < shopItemsIcons.Count; i++)
        {
            if (i >= toolsList.Count)
            {
                shopItemsIcons[i].HideItem();
                continue;
            }
            
            shopItemsIcons[i].ShowItem(toolsList[i].toolName);
            if (toolsList[i].scoreCost > ScoringSystem.Instance.currentScore)
                shopItemsIcons[i].raycastedSprite.color = Color.red;
            else
                shopItemsIcons[i].raycastedSprite.color = Color.white;
        }
    }

    public void CloseShop()
    {
        Debug.Log("CloseShop");
        canvasAnim.gameObject.SetActive(false);
        IsActive = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    public void ShopItemClicked(int index)
    {
        // select tool
        selectedInfoNameText.text = toolsList[index].toolName;
        selectedInfoDescriptionText.text = toolsList[index].toolDescription;
        
        if (toolsList[index].scoreCost > ScoringSystem.Instance.currentScore)
            buyButtonImage.color = Color.red;
        else
            buyButtonImage.color = Color.white;
    }
}

[Serializable]
public class Tool
{
    public enum ToolType
    {
        DualWeilder, OneTimeShield, SpyCam, CustomLadder,
        FragGrenade
    }

    public ToolType tool;
    public int scoreCost = 1000;
    [Range(1, 99)]
    public int maxAmount = 1;

    public bool activeTool = false;
    
    public string toolName = "Name";
    public string toolDescription = "Description";
}