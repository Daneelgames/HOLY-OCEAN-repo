using System;
using MrPink.PlayerSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink
{
    public class Game : MonoBehaviour
    {
        private static Game _instance;

        private GlobalFlags _flags = new GlobalFlags();
        
        [SerializeField, SceneObjectsOnly, Required]
        private Player _player;
        [SerializeField, SceneObjectsOnly]
        
        private LightManager _lightManager;

        public static LightManager LightManager => _instance._lightManager;

        public static Player Player
            => _instance._player;

        public static GlobalFlags Flags
            => _instance._flags;


        private void Awake()
        {
            _instance = this;
        }
    }
}