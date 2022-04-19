using System;
using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using MrPink.PlayerSystem;
using UnityEngine;

namespace MrPink
{
    public class PartyController : MonoBehaviour
    {
        public static PartyController Instance;
    
        public ControlledVehicle playerCar;
        public HealthController npcInParty;

        private void Awake()
        {
            Instance = this;
        }    
    
        public void Init(Transform roadPartToSpawnOn)
        {
            if (roadPartToSpawnOn == null)
            {
                Debug.LogError("NO STRAIGHT ROADS HERE");
            }
            Vector3 newCarPos = roadPartToSpawnOn.position + roadPartToSpawnOn.forward * 5;
            Quaternion newCarRot = roadPartToSpawnOn.rotation;
            newCarRot.eulerAngles = new Vector3(newCarRot.eulerAngles.x, newCarRot.eulerAngles.y + 180, newCarRot.eulerAngles.z);
        
            // PLACE PLAYER INSIDE THE CAR
            // PLACE NPC INSIDE THE CAR
            // MOVE THE CAR TO NEW TRANSFORM

            /*
        playerCar.transform.position = newCarPos;
        playerCar.transform.rotation = newCarRot;*/


            if (ProgressionManager.Instance.CurrentLevel.mrCaptainPrefabToSpawn)
            {
                var captain = Instantiate(ProgressionManager.Instance.CurrentLevel.mrCaptainPrefabToSpawn,
                    playerCar.sitTransformNpc.position, playerCar.sitTransformNpc.rotation);
                //captain.aiVehicleControls.PassengerSit(playerCar);
                npcInParty = captain;
            }
        
            Game.Player.Movement.gameObject.SetActive(true);
            Game.Player.Interactor.cam.gameObject.SetActive(true);
            Game.Player.VehicleControls.RequestVehicleAction(playerCar);
        }

        public void SetPlayerInCar(ControlledVehicle vehicle)
        {
            if (npcInParty)
                npcInParty.aiVehicleControls.SetPassengerSit(vehicle);
        }
    }
}