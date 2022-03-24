using System.Collections;
using System.Collections.Generic;
using System.Net.Configuration;
using MrPink.PlayerSystem;
using MrPink.Tools;
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
    public Text buyForText;
    public Image buyButtonImage;

    private bool isActive = false;
    private int selectedItemIndex = 0;

    public bool IsActive
    {
        get { return isActive; }
        set { isActive = value; }
    }
    private void Awake()
    {
        Instance = this;
    }

    IEnumerator Start()
    {
        toolsList = new List<Tool>(ProgressionManager.Instance.CurrentLevel.toolsInShop);
        switch (LevelGenerator.Instance.levelType)
        {
            case LevelGenerator.LevelType.Game:
                OpenShop(0);
                break;
            case LevelGenerator.LevelType.Narrative:
                CloseShop();
                break;
        }

        while (isActive)
        {
            if (Input.GetKeyDown(KeyCode.Space))
                CloseShop();
            
            yield return null;
        }
    }

    void OpenShop(int newSelectedItem)
    {
        ScoringSystem.Instance.UpdateScore();
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
            if (toolsList[i].scoreCost > ScoringSystem.Instance.CurrentScore)
                shopItemsIcons[i].raycastedSprite.color = Color.red;
            else
                shopItemsIcons[i].raycastedSprite.color = Color.white;
        }
        SelectItem(newSelectedItem);
    }

    public void CloseShop()
    {
        Debug.Log("CloseShop");
        canvasAnim.gameObject.SetActive(false);
        IsActive = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Player.ThrowableControls.Init();    
    }
    
    public void SelectItem(int index)
    {
        // select tool
        selectedItemIndex = index;
        selectedInfoNameText.text = toolsList[selectedItemIndex].toolName;
        selectedInfoDescriptionText.text = toolsList[selectedItemIndex].toolDescription;
        int amount = Player.Inventory.GetAmount(toolsList[selectedItemIndex].tool);
        selectedInfoDescriptionText.text += ". " + amount + " / " + toolsList[selectedItemIndex].maxAmount;
        
        // TODO перенести текстовые штуки в систему локализации
        
        if (!Player.Inventory.CanFitTool(toolsList[selectedItemIndex]))
        {
            buyForText.text = "Max Amount";
            buyButtonImage.color = Color.red;
        }
        else
        {
            if (toolsList[selectedItemIndex].scoreCost > ScoringSystem.Instance.CurrentScore ||
                Player.Inventory.CanFitTool(toolsList[selectedItemIndex]) == false)
            {
                buyForText.text = "Not enough dollars";   
                buyButtonImage.color = Color.red;
            }
            else
            {
                buyForText.text = "Buy for " + toolsList[selectedItemIndex].scoreCost + " dollars";
                buyButtonImage.color = Color.green;
            }
        }

        buyForText.text = buyForText.text.ToUpper();
        selectedInfoNameText.text = selectedInfoNameText.text.ToUpper();
        selectedInfoDescriptionText.text = selectedInfoDescriptionText.text.ToUpper(); 

    }

    public void BuyItem()
    {
        // buy selectedItemIndex item
        if (toolsList[selectedItemIndex].scoreCost > ScoringSystem.Instance.CurrentScore)
            return;
        if (!Player.Inventory.CanFitTool(toolsList[selectedItemIndex]))
            return;
        Player.Inventory.AddTool(toolsList[selectedItemIndex]);
        
        ScoringSystem.Instance.RemoveScore(toolsList[selectedItemIndex].scoreCost);
        OpenShop(selectedItemIndex);
    }
}