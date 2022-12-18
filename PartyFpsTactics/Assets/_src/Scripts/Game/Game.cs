using System;
using MrPink.PlayerSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink
{
    public class Game : MonoBehaviour
    {
        public static Game _instance;
        [SerializeField]
        private GlobalFlags _flags = new GlobalFlags();
        
        [SerializeField]
        private Player _localPlayer;
        [SerializeField, SceneObjectsOnly]
        
        private LightManager _lightManager;

        public static LightManager LightManager => _instance._lightManager;

        public static Player LocalPlayer
            => _instance._localPlayer;

        public static GlobalFlags Flags
            => _instance._flags;


        private void Awake()
        {
            _instance = this;
        }

        public void SetLocalPlayer(Player p)
        {
            _localPlayer = p;
        }
    }
}