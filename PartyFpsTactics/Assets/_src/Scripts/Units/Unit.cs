using System;
using MrPink.Health;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink.Units
{
    public class Unit : MonoBehaviour
    {
        [Header("SET TRUE FOR SIMPLE MOBS")]
        [SerializeField] private bool destroyOnDistance = false;
        public bool DestroyOnDistance => destroyOnDistance;
        
        public Transform faceCam;
        [SerializeField, ChildGameObjectsOnly, Required]
        private HealthController _healthController;
        
        [SerializeField, ChildGameObjectsOnly, ]
        private UnitVision _unitVision;
        
        [SerializeField, ChildGameObjectsOnly]
        private UnitAiMovement _unitAiMovement;
        [SerializeField, ChildGameObjectsOnly]
        private UnitMovement _unitMovement;
        
        [SerializeField, ChildGameObjectsOnly]
        private UnitFollowTarget _followTarget;
        
        [SerializeField, ChildGameObjectsOnly]
        private UnitAiWeaponControls _unitWeaponControls;
        [SerializeField, ChildGameObjectsOnly]
        private AiVehicleControls _aiVehicleControls;
        public AiVehicleControls AiVehicleControls
        {
            get => _aiVehicleControls;
        }

        [SerializeField, ChildGameObjectsOnly, ]
        private HumanVisualController _humanVisualController;
        

        [SerializeField, ChildGameObjectsOnly, Required]
        private CharacterNeeds _characterNeeds;
        [SerializeField, ChildGameObjectsOnly, Required]
        private SpawnLootOnDeath _spawnLootOnDeath;
        
        [SerializeField, ChildGameObjectsOnly]
        private InteractiveObject _npcInteraction;
        

        public HealthController HealthController
            => _healthController;

        public UnitVision UnitVision
            => _unitVision;
        
        public UnitAiMovement UnitAiMovement
            => _unitAiMovement;
        public UnitAiWeaponControls UnitWeaponControls
            => _unitWeaponControls;
        
        public HumanVisualController HumanVisualController
            => _humanVisualController;
        
        public UnitMovement UnitMovement
            => _unitMovement;
        
        public UnitFollowTarget UnitFollowTarget
            => _followTarget;
        public CharacterNeeds CharacterNeeds
            => _characterNeeds;
        public SpawnLootOnDeath SpawnLootOnDeath
            => _spawnLootOnDeath;

        public InteractiveObject NpcInteraction
            => _npcInteraction;

        public void Resurrect()
        {
            if (_healthController.health > 0)
                return;
            _healthController.Resurrect();
            
            if (_unitMovement)
                _unitMovement.RestoreAgent();
            
            if (_humanVisualController)
                _humanVisualController.Restore();
        }

        public void Death()
        {
            if (_unitWeaponControls)
                _unitWeaponControls.Death();
            if (_aiVehicleControls) _aiVehicleControls.Death();
            _healthController.health = 0;
        }

        [SerializeField] [ReadOnly]private bool culled = false;
        public void Cull(bool cull)
        {
            culled = cull;
        }
        
        

        
#if UNITY_EDITOR
        [Button("Set Self Links")]
        private void SetSelf()
        {
            _healthController = GetComponent<HealthController>();
            _unitVision = GetComponent<UnitVision>();
            _unitAiMovement = GetComponent<UnitAiMovement>();
            _humanVisualController = GetComponent<HumanVisualController>();
            _unitMovement = GetComponent<UnitMovement>();
            _followTarget = GetComponent<UnitFollowTarget>();
            _characterNeeds = GetComponent<CharacterNeeds>();
            
            _unitAiMovement?.SetUnit(this);
            _unitMovement?.SetUnit(this);
        }
#endif
        
        
        
    }
}