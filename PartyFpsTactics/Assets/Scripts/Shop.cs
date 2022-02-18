using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    public static Shop Instance;
    
    public List<Tool> toolsList;
    public List<ShopItem> shopItemsIcons;
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
        Debug.Log("ShopItemClicked " + index);
        
    }
}

[Serializable]
public class Tool
{
    public enum ToolType
    {
        DualWeilder, OneTimeShield, SpyCam, CustomLadder 
    }

    public ToolType tool;
    [Range(1, 99)]
    public int maxAmount = 1;

    public bool activeTool = false;
    
    public string toolName = "Name";
    public string toolDescription = "Description";
}