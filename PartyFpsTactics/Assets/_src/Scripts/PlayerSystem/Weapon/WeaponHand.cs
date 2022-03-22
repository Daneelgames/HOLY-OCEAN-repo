using System;
using Brezg.Serialization;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink.PlayerSystem
{
    public class WeaponHand : MonoBehaviour
    {
        [SerializeField, SceneObjectsOnly]
        [CanBeNull]
        private WeaponController _weapon;

        [SerializeField, ChildGameObjectsOnly, Required]
        private UnityDictionary<WeaponPosition, Transform> _positions = new UnityDictionary<WeaponPosition, Transform>();

        
        [ShowInInspector, ReadOnly] 
        public WeaponPosition CurrentPosition { get; set; } = WeaponPosition.Idle;

        public Transform CurrentTransform
            => this[CurrentPosition];

        public bool IsWeaponEquipped
            => _weapon != null;

        public Transform this[WeaponPosition position]
            => _positions[position];

        
        public WeaponController Weapon
        {
            get => _weapon;
            set => _weapon = value;
        }

    }
}