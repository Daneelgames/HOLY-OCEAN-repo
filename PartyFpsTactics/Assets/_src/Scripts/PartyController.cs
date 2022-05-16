using System;
using System.Collections;
using System.Collections.Generic;
using _src.Scripts;
using MrPink.Health;
using MrPink.PlayerSystem;
using MrPink.Units;
using UnityEngine;

namespace MrPink
{
    public class PartyController : MonoBehaviour
    {
        public static PartyController Instance;
    
        public ControlledMachine playerCar;
        public HealthController npcInParty;

        private void Awake()
        {
            Instance = this;
        }    
    
        public void Init(Transform roadPartToSpawnOn = null)
        {
            /*
            if (roadPartToSpawnOn == null)
            {
                //Debug.LogError("NO STRAIGHT ROADS HERE");
            }
            Vector3 newCarPos = roadPartToSpawnOn.position + roadPartToSpawnOn.forward * 5;
            Quaternion newCarRot = roadPartToSpawnOn.rotation;
            newCarRot.eulerAngles = new Vector3(newCarRot.eulerAngles.x, newCarRot.eulerAngles.y + 180, newCarRot.eulerAngles.z);
        
            // PLACE PLAYER INSIDE THE CAR
            // PLACE NPC INSIDE THE CAR
            // MOVE THE CAR TO NEW TRANSFORM

            playerCar.transform.position = newCarPos;
            playerCar.transform.rotation = newCarRot;
            */

            if (ProgressionManager.Instance.CurrentLevel.mrCaptainPrefabToSpawn)
            {
                var captain = Instantiate(ProgressionManager.Instance.CurrentLevel.mrCaptainPrefabToSpawn,
                    playerCar.sitTransformNpc.position, playerCar.sitTransformNpc.rotation);
                //captain.aiVehicleControls.PassengerSit(playerCar);
                npcInParty = captain;
                Game.Player.CommanderControls.unitsInParty.Add(captain);
            }
        
            Game.Player.Movement.gameObject.SetActive(true);
            Game.Player.Interactor.cam.gameObject.SetActive(true);
        }

        public void SetPlayerInCar(ControlledMachine machine)
        {
            if (npcInParty && Vector3.Distance(npcInParty.transform.position, Game.Player.Position) < 15)
                npcInParty.aiVehicleControls.SetPassengerSit(machine);
        }

        public IEnumerator RespawnPlayer()
        {
            var pos = Game.Player.Position;
            ScoringSystem.Instance.AddScore(Mathf.RoundToInt(-ScoringSystem.Instance.CurrentScore * 0.75f));
            Game.Player.Inventory.DropRandomTools();

            if (npcInParty)
            {
                if (npcInParty.health > 0)
                {
                    npcInParty.selfUnit.UnitMovement.TeleportNearPosition(pos);
                }
                else
                {
                    npcInParty.selfUnit.Resurrect();
                    npcInParty.selfUnit.UnitMovement.TeleportNearPosition(pos);
                }
                if (npcInParty.npcInteraction && npcInParty.npcInteraction.npcDialoguesList)
                {
                    npcInParty.npcInteraction.CheckNpcDialogueList();
                }

            }

            if (playerCar)
            {
                playerCar.rb.velocity = Vector3.zero;
                playerCar.rb.angularVelocity = Vector3.zero;
                playerCar.transform.rotation = Quaternion.identity;
                playerCar.transform.position = pos;
            }
            Game.Player.Resurrect();
            UnitsManager.Instance.MoveUnitsToRespawnPoints(true, true);
            yield return new WaitForSeconds(1f);
            if (npcInParty && npcInParty.npcInteraction)
            {
                npcInParty.npcInteraction.PlayerInteraction();
            }
            
            LevelTitlesManager.Instance.HideIntro();
        }
    }
}