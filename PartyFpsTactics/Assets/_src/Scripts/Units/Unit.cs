using MrPink.Health;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink.Units
{
    public class Unit : MonoBehaviour
    {
        [SerializeField, ChildGameObjectsOnly, Required]
        private HealthController _healthController;
        
        [SerializeField, ChildGameObjectsOnly, Required]
        private UnitVision _unitVision;
        
        [SerializeField, ChildGameObjectsOnly, Required]
        private UnitAiMovement _unitAiMovement;
        
        [SerializeField, ChildGameObjectsOnly, Required]
        private HumanVisualController _humanVisualController;
        
        [SerializeField, ChildGameObjectsOnly, Required]
        private UnitMovement _unitMovement;
        
        [SerializeField, ChildGameObjectsOnly, Required]
        private UnitFollowTarget _followTarget;

        [SerializeField, ChildGameObjectsOnly, Required]
        private CharacterNeeds _characterNeeds;


        public HealthController HealthController
            => _healthController;

        public UnitVision UnitVision
            => _unitVision;
        
        public UnitAiMovement UnitAiMovement
            => _unitAiMovement;
        
        public HumanVisualController HumanVisualController
            => _humanVisualController;
        
        public UnitMovement UnitMovement
            => _unitMovement;
        
        public UnitFollowTarget UnitFollowTarget
            => _followTarget;
        public CharacterNeeds CharacterNeeds
            => _characterNeeds;
        
        
        #if UNITY_EDITOR

        [ContextMenu("Set Self Links")]
        private void SetSelf()
        {
            _healthController = GetComponent<HealthController>();
            _unitVision = GetComponent<UnitVision>();
            _unitAiMovement = GetComponent<UnitAiMovement>();
            _humanVisualController = GetComponent<HumanVisualController>();
            _unitMovement = GetComponent<UnitMovement>();
            _followTarget = GetComponent<UnitFollowTarget>();
            _characterNeeds = GetComponent<CharacterNeeds>();
            
            _unitAiMovement.SetUnit(this);
            _unitMovement.SetUnit(this);
        }
        
        #endif
    }
}