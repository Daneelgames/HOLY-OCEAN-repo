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

public class InteractableEventsManager : MonoBehaviour
{
    public static InteractableEventsManager Instance;
    public List<InteractiveObject> InteractiveObjects = new List<InteractiveObject>();
    void Awake()
    {
        Instance = this;
    }

    public void AddInteractable(InteractiveObject obj)
    {
        InteractiveObjects.Add(obj);
    }
    public void RemoveInteractable(InteractiveObject obj)
    {
        if (InteractiveObjects.Contains(obj))
            InteractiveObjects.Remove(obj);
    }

    public void InteractWithIO(InteractiveObject IO)
    {
        for (int i = 0; i < IO.eventsOnInteraction.Count; i++)
        {
            var IOevent = IO.eventsOnInteraction[i];
            
            RunEvent(IOevent);

            if (IOevent.scriptedEventType == ScriptedEventType.DestroyOnInteraction)
            {
                Destroy(IO.gameObject);
            }
        }
    }

    public void RunEvent(ScriptedEvent IOevent, Quest quest = null, GameObject gameObjectToDestroy = null)
    {
        var npcHc = IOevent.ActorNpc; 
        if (npcHc == null)
        {
            if (quest != null && IOevent.spawnedQuestHcId >= 0)
            {
                npcHc = quest.spawnedQuestNpcs[IOevent.spawnedQuestHcId];
            }
            else if (IOevent.actorId >= 0 && LevelEventsOnConditions.Instance.levelActors.Count > IOevent.actorId)
                npcHc = LevelEventsOnConditions.Instance.GetHcById(IOevent.actorId);
        }
        if (npcHc && !npcHc.gameObject.activeInHierarchy)
            UnitsManager.Instance.ShowUnit(npcHc, true);
        
        switch (IOevent.scriptedEventType)
        {
            case ScriptedEventType.StartDialogue:

                PhoneDialogueEvents.Instance.RunNpcDialogueCutscene(IOevent.dialogueToStart, npcHc, IOevent.destroyInteractorAfterDialogueCompleted, IOevent.scoreToAddOnDialogueCompleted, IOevent.setNextLevelOnDialogueCompleted);
                if (IOevent.maxDistanceToSpeaker > 0)
                    DialogueWindowInterface.Instance.StartCheckingDistanceToSpeaker(npcHc, IOevent.maxDistanceToSpeaker);
                break;
            
            case ScriptedEventType.SpawnObject:
                GameObject newObj;
                if (IOevent.spawnInsideCamera)
                {
                    newObj = Instantiate(IOevent.prefabToSpawn, Game.Player.Interactor.cam.transform);
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
                GameManager.Instance.StartBuildingScene();
                break;
                
            case ScriptedEventType.StartFlatScene:
                GameManager.Instance.StartFlatScene();
                break;
            
            case ScriptedEventType.AddScore:
                ScoringSystem.Instance.AddScore(IOevent.scoreToAdd);
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
                Game.Player.VehicleControls.RequestVehicleAction(IOevent.controlledMachine);
                break;
            
            case ScriptedEventType.AddTool:
                // todo - вынести ScoringSystem.Instance.ItemFoundSound() в другое место
                ScoringSystem.Instance.ItemFoundSound();
                Game.Player.Inventory.AddTool(IOevent.toolToAdd);
                break;
            case ScriptedEventType.AddWeapon:
                // todo - вынести ScoringSystem.Instance.ItemFoundSound() в другое место
                ScoringSystem.Instance.ItemFoundSound();
                Game.Player.Inventory.SpawnPlayerWeapon(IOevent.weaponToAdd);
                break;
            
            case ScriptedEventType.StartRandomQuest:
                QuestManager.Instance.StartRandomQuest();
                break;
            case ScriptedEventType.AddQuestMarker:
                int AddQuestMarkerHcIndex = IOevent.questMarkerTargetHcIndex;
                QuestMarkers.Instance.AddMarker(quest.spawnedQuestNpcs[AddQuestMarkerHcIndex].visibilityTrigger.transform, quest);
                break;
            case ScriptedEventType.RemoveQuestMarker:
                int RemoveQuestMarkerHcIndex = IOevent.questMarkerTargetHcIndex;
                QuestMarkers.Instance.RemoveMarker(quest.spawnedQuestNpcs[RemoveQuestMarkerHcIndex].visibilityTrigger.transform);
                
                break;
            case ScriptedEventType.SpawnQuestNpc:
                
                Debug.Log("ScriptedEventType.SpawnQuestNpc");
                Vector3 pos = Game.Player.Position;
                
                var npc = UnitsManager.Instance.SpawnUnit(IOevent.NpcPrefabsToSpawn[Random.Range(0,IOevent.NpcPrefabsToSpawn.Count)], pos);
                
                if (quest != null)
                {
                    npc.name += ". QuestNpc";
                    quest.AddSpawnedHc(npc);
                }
                
                Debug.Log("ScriptedEventType.SpawnQuestNpc; " + npc.gameObject.name + "; quest is " + quest);
                
                break;
        }
        
        if (gameObjectToDestroy)
            Destroy(gameObjectToDestroy);
    }

    public void ExplosionNearInteractables(Vector3 explosionPosition, float distance = 10, float force = 10)
    {
        for (int i = 0; i < InteractiveObjects.Count; i++)
        {
            if (InteractiveObjects[i].type != InteractiveObject.InteractableType.ItemInteractable)
                continue;
            
            if (InteractiveObjects[i].rb == null)
            {
                var rb = InteractiveObjects[i].gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.drag = 1;
                rb.angularDrag = 1;

                InteractiveObjects[i].rb = rb;
                
                rb.AddExplosionForce(force, explosionPosition, distance);
            }
        }
    }
}


