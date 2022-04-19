using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink.PlayerSystem
{
    public class PlayerLookAround : MonoBehaviour
    {
        [SerializeField, SceneObjectsOnly, Required]
        private Transform _headTransform;
        
        [SerializeField]
        public float _mouseSensitivity = 5;
        
        [SerializeField]
        private float _vertLookAngleClamp = 85;
        
        [SerializeField]
        private float _cameraFollowBodySmooth = 3;
        
        [SerializeField, BoxGroup("Высота")]
        [PropertyRange(nameof(_playerHeadHeightCrouch), 2.5f)]
        public float _playerHeadHeight = 1.8f;
        
        [SerializeField, BoxGroup("Высота")]
        [PropertyRange(0, nameof(_playerHeadHeight))]
        private float _playerHeadHeightCrouch = 0.5f;
        
        [CanBeNull]
        private Transform _killerToLookAt = null;
        
        private float _playerHeadHeightTarget;
        
        private float _verticalRotation = 0.0f;
        private float _horizontalRotation = 0.0f;

        private bool _isDead = false;
        private Transform currentCutsceneTargetTransform;

        Transform vehicleHeadDummyTransform;

        private void Awake()
        {
            SetCrouch(false);
            
            vehicleHeadDummyTransform = new GameObject("VehicleHeadDummy").transform;
            vehicleHeadDummyTransform.parent = transform.parent;
        }

        private void LateUpdate()
        {/*
            if (LevelGenerator.Instance.levelIsReady == false)
                return;*/
            /*if (ProceduralCutscenesManager.Instance.InCutScene)
            {
                FollowCutSceneTargetTransform();
                return;
            }*/
            
            if (Shop.Instance && Shop.Instance.IsActive)
                return;
        
            MouseLook();
        }

        
        public void SetCurrentCutsceneTargetTrasform(Transform _transform)
        {
            currentCutsceneTargetTransform = _transform;
        }
        void FollowCutSceneTargetTransform()
        {
            if (!currentCutsceneTargetTransform)
                return;
            
            _headTransform.position = Vector3.Lerp(_headTransform.position,currentCutsceneTargetTransform.position, Time.deltaTime);
            _headTransform.rotation = Quaternion.Slerp(_headTransform.rotation,currentCutsceneTargetTransform.rotation, Time.deltaTime);
        }


        public void SetCrouch(bool isCrouching)
        {
            _playerHeadHeightTarget = isCrouching ? _playerHeadHeightCrouch : _playerHeadHeight;
        }
        
        private void MouseLook()
        {
            // TODO не работать с this.transform
            
            if (_isDead && _killerToLookAt != null)
            {
                _headTransform.rotation = Quaternion.Lerp(_headTransform.rotation, Quaternion.LookRotation(_killerToLookAt.position - _headTransform.position), Time.deltaTime);
                return;
            }
        
            Vector3 newRotation = new Vector3(0, _horizontalRotation, 0);
            _horizontalRotation += Input.GetAxis("Mouse X") * _mouseSensitivity * Time.deltaTime;
            newRotation = new Vector3(transform.localRotation.x, _horizontalRotation, transform.localRotation.z);
            
            _verticalRotation -= Input.GetAxis("Mouse Y") * _mouseSensitivity * Time.deltaTime;
            _verticalRotation = Mathf.Clamp(_verticalRotation, -_vertLookAngleClamp, _vertLookAngleClamp);

            if (Game.Player.VehicleControls.controlledVehicle == null)
            {
                transform.localRotation = Quaternion.Euler(newRotation);
                newRotation = new Vector3(_verticalRotation, 0, 0) + transform.eulerAngles;
            }
            else
            {
                vehicleHeadDummyTransform.localRotation = Quaternion.Slerp( Quaternion.Euler(new Vector3(transform.localEulerAngles.x, newRotation.y, transform.localEulerAngles.z)), Quaternion.Euler(newRotation), Time.deltaTime);
                newRotation = new Vector3(_verticalRotation, 0, 0) + vehicleHeadDummyTransform.eulerAngles;
            }
        
            _headTransform.rotation = Quaternion.Euler(newRotation);

            float resultFollowSpeed = _cameraFollowBodySmooth;
            
            if (Game.Player.VehicleControls.controlledVehicle)
                resultFollowSpeed *= 10;
            
            _headTransform.transform.position = 
                Vector3.Lerp(_headTransform.transform.position,transform.position + transform.up * _playerHeadHeightTarget, 
                    resultFollowSpeed * Time.smoothDeltaTime);
            vehicleHeadDummyTransform.position = _headTransform.transform.position;
        }

        public void Death(Transform killer = null)
        {
            _isDead = true;
            _killerToLookAt = killer;
        }
        
    }
}