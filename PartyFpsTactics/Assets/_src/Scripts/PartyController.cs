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
    
        public IEnumerator Init(Transform roadPartToSpawnOn = null)
        {
            while (Game._instance == null || Game.LocalPlayer == null)
            {
                yield return null;
            }

            while (ProgressionManager.Instance == null)
            {
                yield return null;
            }
            if (ProgressionManager.Instance.CurrentLevel.mrCaptainPrefabToSpawn)
            {
                var captain = Instantiate(ProgressionManager.Instance.CurrentLevel.mrCaptainPrefabToSpawn,
                    playerCar.sitTransformNpc.position, playerCar.sitTransformNpc.rotation);
                //captain.aiVehicleControls.PassengerSit(playerCar);
                npcInParty = captain;
                Game.LocalPlayer.CommanderControls.unitsInParty.Add(captain);
            }
        
            Game.LocalPlayer.Movement.gameObject.SetActive(true);
            Game.LocalPlayer.Interactor.cam.gameObject.SetActive(true);
        }

        public void SetPlayerInCar(ControlledMachine machine)
        {
            if (npcInParty && Vector3.Distance(npcInParty.transform.position, Game.LocalPlayer.Position) < 15)
                npcInParty.aiVehicleControls.SetPassengerSit(machine);
        }

        public IEnumerator RespawnPlayer()
        {
            var pos = Game.LocalPlayer.Position;
            ScoringSystem.Instance.AddScore(Mathf.RoundToInt(-ScoringSystem.Instance.CurrentScore * 0.75f));
            Game.LocalPlayer.Inventory.DropAll();

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
            Game.LocalPlayer.Resurrect();
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