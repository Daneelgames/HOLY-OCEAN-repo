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
                npcInParty = captain;
                Game.LocalPlayer.CommanderControls.unitsInParty.Add(captain);
            }
        
            Game.LocalPlayer.Movement.gameObject.SetActive(true);
            Game.LocalPlayer.Interactor.cam.gameObject.SetActive(true);
        }

        public void SetPlayerInCar(ControlledMachine machine)
        {
            // put npc in car if there are multiple sits
            /*
            if (npcInParty && Vector3.Distance(npcInParty.transform.position, Game.LocalPlayer.Position) < 15)
                npcInParty.aiVehicleControls.SetPassengerSit(machine);*/
        }
    }
}