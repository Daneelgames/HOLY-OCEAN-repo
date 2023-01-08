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

    }
}
