using System.Collections;
using System.Collections.Generic;
using FishNet.Object;
using MrPink;
using MrPink.Health;
using MrPink.PlayerSystem;
using MrPink.Units;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _src.Scripts
{
    public class BloodOcean : NetworkBehaviour
    {
        public float waterLevel = -25;
        [SerializeField] private float suckWaterNeedPerSecond = 10;

        public static BloodOcean Instance;
        private void Awake()
        {
            Instance = this;
        }

        void Update()
        {
            if (Game._instance == null || Game.LocalPlayer == null)
            {
                return;
            }
            if (Game.LocalPlayer.Position.y < waterLevel)
            {
                if (Game.LocalPlayer.Health.health > 0)
                {
                    if (Game.LocalPlayer.Movement.State.IsUnderWater == false)
                        Game.LocalPlayer.Movement.SetUnderWater(true);
                    Game.LocalPlayer.CharacterNeeds.AddToNeed(Need.NeedType.Water, suckWaterNeedPerSecond * Time.deltaTime);
                }
            }
            else if (Game.LocalPlayer.Movement.State.IsUnderWater)
            {
                Game.LocalPlayer.Movement.SetUnderWater(false);
            }
        }
    }
}
