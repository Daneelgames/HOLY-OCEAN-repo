using System;
using System.Collections;
using System.Collections.Generic;
using _src.Scripts.Data;
using MrPink;
using MrPink.Health;
using MrPink.PlayerSystem;
using MrPink.Units;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Random = UnityEngine.Random;
using Unit = MrPink.Units.Unit;

public class InteractableEventsManager : MonoBehaviour
{
    public static InteractableEventsManager Instance;
    public List<InteractiveObject> InteractiveObjects = new List<InteractiveObject>();
    public List<InteractiveObject> InteractivePickUps = new List<InteractiveObject>();
    private List<PropBump> _propBumps = new List<PropBump>();

    public float GetDistanceFromClosestPickUpToPosition(Vector3 askingPos)
    {
        float distance = 10000;
        foreach (var pickUp in InteractivePickUps)
        {
            float d = Vector3.Distance(askingPos, pickUp.transform.position);
            if (d < distance)
            {
                distance = d;
            }
        }

        return distance;
    }
    void Awake()
    {
        Instance = this;
    }

    public void PickUpAllPickups()
    {
        foreach (var interactivePickUp in InteractivePickUps)
        {
            interactivePickUp.PlayerInteraction();
        }
    }
    public void DestroyAllPickups()
    {
        foreach (var interactivePickUp in InteractivePickUps)
        {
            Destroy(interactivePickUp.gameObject);
        }
        InteractivePickUps.Clear();
    }

    public void AddPropBump(PropBump propBump)
    {
        if (_propBumps.Contains(propBump))
            return;
        
        _propBumps.Add(propBump);
    }
    public void RemovePropBump(PropBump propBump)
    {
        if (_propBumps.Contains(propBump))
            _propBumps.Remove(propBump);
    }
    public void AddInteractable(InteractiveObject obj)
    {
        InteractiveObjects.Add(obj);
        switch (obj.type)
        {
            case InteractiveObject.InteractableType.ItemInteractable:
                InteractivePickUps.Add(obj);
                break;
        }
        
    }
    public void RemoveInteractable(InteractiveObject obj)
    {
        if (InteractiveObjects.Contains(obj))
        {
            if (obj.type == InteractiveObject.InteractableType.ItemInteractable)
                InteractivePickUps.Remove(obj);
            InteractiveObjects.Remove(obj);
        }
    }

    public void InteractWithIO(InteractiveObject IO, bool qPressed = false, bool ePressed = false)
    {
        for (int i = 0; i < IO.eventsOnInteraction.Count; i++)
        {
            var IOevent = IO.eventsOnInteraction[i];
            
            RunEvent(IOevent, null, null, qPressed, ePressed, IO);

            if (IOevent.scriptedEventType == ScriptedEventType.DestroyOnInteraction)
            {
                Destroy(IO.gameObject);
            }
        }
    }

    public void RunEvent(ScriptedEvent IOevent, Quest quest = null, GameObject gameObjectToDestroy = null, bool qPressed = false, bool ePressed = false, InteractiveObject IO = null)
    {
        if (IOevent == null)
            return;
        
        var npcHc = IOevent.ActorNpc; 
        if (npcHc == null)
        {
            if (IOevent.actorId >= 0 && LevelEventsOnConditions.Instance.levelActors.Count > IOevent.actorId)
                npcHc = LevelEventsOnConditions.Instance.GetHcById(IOevent.actorId);
        }
        if (npcHc && !npcHc.gameObject.activeInHierarchy)
            UnitsManager.Instance.ShowUnit(npcHc, true);
        
        switch (IOevent.scriptedEventType)
        {
            case ScriptedEventType.OpenShop:

                Shop.Instance.OpenShop(0);
                break;
            case ScriptedEventType.StartDialogue:

                PhoneDialogueEvents.Instance.RunNpcDialogueCutscene(IOevent.dialogueToStart, npcHc, IOevent.destroyInteractorAfterDialogueCompleted, IOevent.scoreToAddOnDialogueCompleted, IOevent.setNextLevelOnDialogueCompleted);
                if (IOevent.maxDistanceToSpeaker > 0)
                    DialogueWindowInterface.Instance.StartCheckingDistanceToSpeaker(npcHc, IOevent.maxDistanceToSpeaker);
                break;
            
            case ScriptedEventType.SpawnObject:
                GameObject newObj;
                if (IOevent.spawnInsideCamera)
                {
                    newObj = Instantiate(IOevent.prefabToSpawn, Game.LocalPlayer.Interactor.cam.transform);
                    newObj.transform.localPosition = Vector3.zero;
                    newObj.transform.localRotation = quaternion.identity;
                }
                else if (IOevent.customSpawnPoint)
                {
                    newObj = Instantiate(IOevent.prefabToSpawn, IOevent.customSpawnPoint.position, IOevent.customSpawnPoint.rotation);   
                }
                else
                {
                    newObj = Instantiate(IOevent.prefabToSpawn, Vector3.zero, Quaternion.identity);
                }
                break;
                
                
            case ScriptedEventType.SetCurrentLevel:
                ProgressionManager.Instance.SetCurrentLevel(IOevent.currentLevelToSet);
                break;
                
            case ScriptedEventType.StartProcScene:
                // restart
                GameManager.Instance.RespawnPlayer();
                break;
                
            case ScriptedEventType.StartFlatScene:
                
                break;
            
            case ScriptedEventType.AddScore:
                ScoringSystem.Instance.AddGold(IOevent.scoreToAdd);
                break;
            
            
            case ScriptedEventType.AddHealth:
                npcHc.AddHealth(IOevent.addToStatAmount);
                break;
            case ScriptedEventType.AddToFood:
                npcHc.needs.AddToNeed(Need.NeedType.Food,IOevent.addToStatAmount);
                break;
            case ScriptedEventType.AddWater:
                npcHc.needs.AddToNeed(Need.NeedType.Water,IOevent.addToStatAmount);
                break;
            case ScriptedEventType.AddSleep:
                npcHc.needs.AddToNeed(Need.NeedType.Sleep,IOevent.addToStatAmount);
                break;
            
            case ScriptedEventType.PlaySound:
                var newGo = new GameObject("Sound " + IOevent.soundToPlay.name);
                var au = newGo.AddComponent<AudioSource>();
                au.pitch = UnityEngine.Random.Range(IOevent.auPitchMinMax.x, IOevent.auPitchMinMax.y);
                au.clip = IOevent.soundToPlay;
                au.volume = 0.66f;
                au.playOnAwake = false;
                au.loop = false;
                au.Play();
                Destroy(newGo, IOevent.soundToPlay.length);
                break;
            
            case ScriptedEventType.RideVehicle:
                Game.LocalPlayer.VehicleControls.RequestVehicleAction(IOevent.controlledMachine);
                break;
            
            case ScriptedEventType.AddTool:
                Game.LocalPlayer.Inventory.AddTool(IOevent.toolToAdd);
                break;
            case ScriptedEventType.AddWeapon:
                if (qPressed)
                    Game.LocalPlayer.Inventory.AddTool(IOevent.weaponToAdd.GetTool);
                else if (ePressed)
                    Game.LocalPlayer.Inventory.AddTool(IOevent.weaponToAdd.GetTool);
                else
                    Game.LocalPlayer.Inventory.AddTool(IOevent.weaponToAdd.GetTool);
                break;
            
            case ScriptedEventType.StartRandomQuest:
                break;
            case ScriptedEventType.AddQuestMarker:
                break;
            case ScriptedEventType.RemoveQuestMarker:
                break;
            case ScriptedEventType.SpawnQuestNpc:
                
                Debug.Log("ScriptedEventType.SpawnQuestNpc");
                break;
            case ScriptedEventType.Resurrect:
                if (IOevent.hcToResurrect.IsPlayer)
                {
                    var player = IOevent.hcToResurrect.gameObject.GetComponent<Player>();
                    if (player)
                        player.ResurrectByOtherPlayerInteraction();
                }
                else
                {
                    var unit = IOevent.hcToResurrect.gameObject.GetComponent<Unit>(); 
                    if (unit)
                        unit.Resurrect();
                }
                break;
            
            case ScriptedEventType.SpawnNewIsland:
                IslandSpawner.Instance.SpawnIslandOnServer();
                break;
            
            case ScriptedEventType.ReturnInventoryLoot:
                Game.LocalPlayer.Inventory.AddInventoryItems(IO.GetPlayerLootInventoryItems);
                ScoringSystem.Instance.AddGold(IO.GetPlayerMoneyToDrop);
                break;
            case ScriptedEventType.OpenMojoCustomization:
                MojoCustomization.Instance.OpenWindow();
                break;
            case ScriptedEventType.ExplodeIsland:
                var island = IslandSpawner.Instance.GetClosestIsland(Vector3.zero);
                if (island == null) Debug.LogError("NO CLOSEST ISLAND FOUND WTF");
                island.ExplodeIsland();
                break;
            case ScriptedEventType.GiveMojoUpgrade:
                ScoringSystem.Instance.GiveMojoRewardBossChest();
                break;
        }
        
        if (gameObjectToDestroy)
            Destroy(gameObjectToDestroy);
    }

    public void ExplosionNearInteractables(Vector3 explosionPosition, float distance = 10, float force = 10)
    {
        for (int i = 0; i < InteractivePickUps.Count; i++)
        {
            if (InteractivePickUps[i].type != InteractiveObject.InteractableType.ItemInteractable)
                continue;
            
            if (Vector3.Distance(InteractivePickUps[i].gameObject.transform.position, explosionPosition) > distance)
                continue;
            
            if (InteractivePickUps[i].rb == null)
            {
                var rb = InteractivePickUps[i].gameObject.AddComponent<Rigidbody>();
                
                if (rb == null)
                    continue;
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.drag = 1;
                rb.angularDrag = 1;

                InteractivePickUps[i].rb = rb;
            }
            InteractivePickUps[i].rb.AddExplosionForce(force, explosionPosition, distance);
        }
        for (int i = 0; i < _propBumps.Count; i++)
        {
            if (Vector3.Distance(_propBumps[i].gameObject.transform.position, explosionPosition) > distance)
                continue;
            
            if (_propBumps[i].RB == null)
            {
                _propBumps[i].SetRb();
            }
            _propBumps[i].RB.AddExplosionForce(force, explosionPosition, distance);
        }
    }
}


