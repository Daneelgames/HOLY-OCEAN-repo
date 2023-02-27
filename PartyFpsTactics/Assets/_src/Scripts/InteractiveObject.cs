using System;
using System.Collections;
using System.Collections.Generic;
using _src.Scripts.Data;
using MrPink;
using MrPink.Health;
using MrPink.PlayerSystem;
using Sirenix.OdinInspector;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    public enum InteractableType
    {
        ItemInteractable, NpcInteractable, VehicleInteractable
    }

    [SerializeField] private bool autoPickUpOnStart = false;
    public InteractableType type = InteractableType.ItemInteractable;

    [ShowIf("type", InteractableType.NpcInteractable)]
    public NpcDialoguesList npcDialoguesList;
    
    public string interactiveObjectName = "A THING";
    public List<ScriptedEvent> eventsOnInteraction;
    public Rigidbody rb;
    public HealthController hc;
    private void Start()
    {
        InteractableEventsManager.Instance.AddInteractable(this);
    }

    private void OnEnable()
    {
        if (autoPickUpOnStart)
            PlayerInteraction();
    }

    private void OnDestroy()
    {
        InteractableEventsManager.Instance.RemoveInteractable(this);
    }

    public void PlayerInteraction(bool qPressed = false, bool ePressed = false)
    {
        CheckNpcDialogueList();
        InteractableEventsManager.Instance.InteractWithIO(this, qPressed, ePressed);
    }

    [SerializeField] [ReadOnly] private List<PlayerInventory.InventoryItem> playerLootInventoryItems;
    private int playerMoneyToDrop = 0;
    public List<PlayerInventory.InventoryItem> GetPlayerLootInventoryItems => playerLootInventoryItems;
    public int GetPlayerMoneyToDrop => playerMoneyToDrop;
    public void SaveInventoryLoot(List<PlayerInventory.InventoryItem> inventoryItems, int moneyToDrop)
    {
        playerLootInventoryItems = new List<PlayerInventory.InventoryItem>();
        
        foreach (var item in inventoryItems)
        {
            var newItem = new PlayerInventory.InventoryItem();
            newItem._toolType = item._toolType;
            newItem.usesLeft = item.usesLeft;
            newItem.currentSlot = PlayerInventory.EquipmentSlot.Slot.Null;
            
            Debug.Log("DROP 0 " + newItem._toolType + "; uses " + newItem.usesLeft);
            playerLootInventoryItems.Add(newItem);
        }
        foreach (var item in playerLootInventoryItems)
        {
            Debug.Log("DROP 1 " + item._toolType + "; uses " + item.usesLeft);
        }
        playerMoneyToDrop = moneyToDrop;
    }
    
    [Button]
    public void DebugInventory()
    {
        foreach (var item in playerLootInventoryItems)
        {
            Debug.Log("DROP 1 " + item._toolType + "; uses " + item.usesLeft);
        }
    }

    public void CheckNpcDialogueList()
    {
        if (type == InteractableType.NpcInteractable && npcDialoguesList)
        {
            npcDialoguesList.TryToReplaceEventsBasedOnConditions(this);
        }   
    }

    public void SetNewEvents(List<ScriptedEvent> newEvents)
    {
        eventsOnInteraction = new List<ScriptedEvent>(newEvents);
    }
}