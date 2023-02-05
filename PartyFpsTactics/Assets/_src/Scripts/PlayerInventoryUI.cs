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
    [SerializeField] private List<ShopItem> inventoryButtons;
    [ReadOnly][SerializeField] private List<PlayerInventory.InventoryItem> toolAmounts;
    
    public Animator canvasAnim;
    private bool isActive = false;
    
    private PlayerInventory.InventoryItem selectedInventoryItem;
    [SerializeField] private List<EquipmentSlotUI> _equipmentSlotUis;

    [SerializeField]private List<InventoryItemActionButtonUi> _inventoryItemActionButtonUis;
    [Serializable] public class EquipmentSlotUI
    {
        public PlayerInventory.EquipmentSlot.Slot slot = PlayerInventory.EquipmentSlot.Slot.Null;
        public ShopItem slotItemUi;
        public PlayerInventory.InventoryItem equippedItem;
    }

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

        toolAmounts = new List<PlayerInventory.InventoryItem>(Game.LocalPlayer.Inventory.inventoryItems);   
        for (int i = 0; i < inventoryButtons.Count; i++)
        {
            var t = inventoryButtons[i];
            if (i >= toolAmounts.Count)
            {
                inventoryButtons[i].HideItem();
                continue;
            }
            
            inventoryButtons[i].ShowItem(toolAmounts[i]);
            int index = i;
            t.button.onClick.AddListener(delegate { SlotSelectedDelegate(index);});
        }
        UpdateEquipmentSlotsUi();
        HideItemActions();
    }

    void UpdateEquipmentSlotsUi()
    {
        foreach (var equipmentSlotUi in _equipmentSlotUis)
        {
            if (equipmentSlotUi.equippedItem != null && equipmentSlotUi.equippedItem.usesLeft > 0)
            {
                equipmentSlotUi.slotItemUi.ShowItem(equipmentSlotUi.equippedItem);
            }
            else
                equipmentSlotUi.slotItemUi.HideItem();
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
        
        for (var index = 0; index < inventoryButtons.Count; index++)
        {
            var t = inventoryButtons[index];
            t.button.onClick.RemoveAllListeners();
        }
    }
    
    public void SlotSelectedDelegate(int idx_)
    {
        //Here i want to know w$$anonymous$$ch button was Clicked.
        //or how to pass a param through addListener
        Debug.Log(idx_  + " SELECTED");
        SlotSelected(idx_);
    } 
    void SlotSelected(int index)
    {
        if (index >= toolAmounts.Count)
            return;
        
        selectedInventoryItem = inventoryButtons[index].CurrentInventoryItem;
        
        // show actions list:
        // equip as weapon / armour
        // place to quick slots
        // drop
        
        Debug.Log(selectedInventoryItem._toolType  + " SELECTED");
        ShowItemActions(selectedInventoryItem.ItemActions);
        //Game.LocalPlayer.Inventory.EquipTool(selectedInventoryItem._toolType, selectedInventoryItem.usesLeft);
    }

    void HideItemActions()
    {
        for (int i = 0; i < _inventoryItemActionButtonUis.Count; i++)
        {
            _inventoryItemActionButtonUis[i].gameObject.SetActive(false);
            _inventoryItemActionButtonUis[i].GetButton.onClick.RemoveAllListeners();
        }
    }
    void ShowItemActions(List<PlayerInventory.ItemAction> itemActions)
    {
        for (int i = 0; i < _inventoryItemActionButtonUis.Count; i++)
        {
            if (i >= itemActions.Count)
            {
                _inventoryItemActionButtonUis[i].gameObject.SetActive(false);
                continue;
            }
            
            
            
            _inventoryItemActionButtonUis[i].gameObject.SetActive(true);
            _inventoryItemActionButtonUis[i].GetButton.onClick.RemoveAllListeners();
            int index = i;
            _inventoryItemActionButtonUis[i].GetButton.onClick.AddListener(delegate { ActionButtonClicked(itemActions[index]);});
            _inventoryItemActionButtonUis[i].SetButtonName(itemActions[i].ToString());
        }
    }

    public void ActionButtonClicked(PlayerInventory.ItemAction itemAction)
    {
        switch (itemAction)
        {
            case PlayerInventory.ItemAction.ArmLeft:
                EquipLeftHand();
                HideItemActions();
                break;
            case PlayerInventory.ItemAction.ArmRight:
                EquipRightHand();
                HideItemActions();
                break;
            case PlayerInventory.ItemAction.Use:
                PlayerToolsControls.Instance.UseTool(selectedInventoryItem._toolType);
                HideItemActions();
                ShowInventory();
                break;
            case PlayerInventory.ItemAction.QuickSlot:
                PlayerToolsControls.Instance.ToggleToQuickSlots(selectedInventoryItem);
                HideItemActions();
                ShowInventory();
                break;
        }
    }
    

    public void EquipLeftHand()
    {
        if (selectedInventoryItem.currentSlot != PlayerInventory.EquipmentSlot.Slot.Null)
        {
            ClearSlot(selectedInventoryItem.currentSlot);
            Game.LocalPlayer.Inventory.ClearSlot(selectedInventoryItem.currentSlot);
        }
        Game.LocalPlayer.Inventory.EquipToolByUiButton(selectedInventoryItem._toolType, selectedInventoryItem.usesLeft, selectedInventoryItem.ItemActions, 0);
        EquipItemUI(PlayerInventory.EquipmentSlot.Slot.LeftHand, selectedInventoryItem);
    }
    public void EquipRightHand()
    {
        if (selectedInventoryItem.currentSlot != PlayerInventory.EquipmentSlot.Slot.Null)
        { 
            ClearSlot(selectedInventoryItem.currentSlot);
            Game.LocalPlayer.Inventory.ClearSlot(selectedInventoryItem.currentSlot);
        }
        Game.LocalPlayer.Inventory.EquipToolByUiButton(selectedInventoryItem._toolType, selectedInventoryItem.usesLeft, selectedInventoryItem.ItemActions, 1);
        EquipItemUI(PlayerInventory.EquipmentSlot.Slot.RightHand, selectedInventoryItem);
    }

    void ClearSlot(PlayerInventory.EquipmentSlot.Slot slot)
    {
        foreach (var equipmentSlotUi in _equipmentSlotUis)
        {
            if (equipmentSlotUi.slot == slot)
            {
                equipmentSlotUi.equippedItem = null;
            }
        }
    }

    public void EquipItemUI(PlayerInventory.EquipmentSlot.Slot slot, PlayerInventory.InventoryItem inventoryItem)
    {
        inventoryItem.currentSlot = slot;
        foreach (var equipmentSlotUi in _equipmentSlotUis)
        {
            if (equipmentSlotUi.slot != slot)
                continue;
            equipmentSlotUi.equippedItem = inventoryItem;
            equipmentSlotUi.slotItemUi.ShowItem(inventoryItem);
        }

        UpdateEquipmentSlotsUi();
    }
}
