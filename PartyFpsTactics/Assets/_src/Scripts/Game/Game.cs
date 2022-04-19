using System;
using MrPink.PlayerSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink
{
    public class Game : MonoBehaviour
    {
        private static Game _instacne;

        private GlobalFlags _flags = new GlobalFlags();
        
        [SerializeField, SceneObjectsOnly, Required]
        private Player _player;


        public static Player Player
            => _instacne._player;

        public static GlobalFlags Flags
            => _instacne._flags;


        private void Awake()
        {
            _instacne = this;
        }
    }
}