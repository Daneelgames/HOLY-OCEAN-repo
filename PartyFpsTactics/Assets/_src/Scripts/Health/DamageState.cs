using System;
using UnityEngine;

namespace MrPink.Health
{
    [Serializable]
    public class DamageState
    {
        public string name = "Normal State";
        public GameObject visual;
        [Range(0, 1)] public float healthPercentage = 1;
    }
}