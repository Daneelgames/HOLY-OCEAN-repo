using System;
using System.Collections;
using FishNet.Object;
using JetBrains.Annotations;
using MrPink.Health;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MrPink.PlayerSystem
{
    public class PlayerLookAround : NetworkBehaviour
    {
        [SerializeField, Required]
        private Transform _headTransform;

        public Vector3 HeadPos => _headTransform.position;
        public Quaternion HeadRot => _headTransform.rotation;
        
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

        private HealthController localPlayerHealth;

        private bool _isDead
        {
            get
            {
                if (localPlayerHealth)
                    return localPlayerHealth.health <= 0;

                localPlayerHealth = gameObject.GetComponent<HealthController>();
                return localPlayerHealth.health <= 0;
            }
        }
        private Transform currentCutsceneTargetTransform;

        Transform vehicleHeadDummyTransform;

        public override void OnStartClient() { 
            base.OnStartClient();
            Init();
        }
        private void Awake()
        {
            _headTransform.gameObject.SetActive(false);
        }

        void Init()
        {
            if (base.IsOwner)
            {
                Debug.Log(gameObject.name + " PLAYER PUT HIMSELF AS LOCAL PLAYER");
                _headTransform.gameObject.SetActive(true);
                _headTransform.parent = null;
            }
            else
            {
                Debug.Log(gameObject.name + " PLAYER PUT HIMSELF AS NON LOCAL PLAYER");
                _headTransform.gameObject.SetActive(false);
                return;
            }
            SetCrouch(false);
            
            if (vehicleHeadDummyTransform == null)
            {
                vehicleHeadDummyTransform = new GameObject("VehicleHeadDummy").transform;
            }
            vehicleHeadDummyTransform.parent = transform/*.parent*/;
        }

        [Client(RequireOwnership = true)]
        private void Update()
        {
            if (base.IsOwner == false)
                return;
            
            /*
            if (LevelGenerator.Instance.levelIsReady == false)
                return;*/
            /*if (ProceduralCutscenesManager.Instance.InCutScene)
            {
                FollowCutSceneTargetTransform();
                return;
            }*/
            
            if (Shop.Instance && Shop.Instance.IsActive)
                return;
            if (PlayerInventoryUI.Instance && PlayerInventoryUI.Instance.IsActive)
                return;
        
            MouseLook();
        }

        private void MouseLook()
        {
            _horizontalRotation += Input.GetAxis("Mouse X") * _mouseSensitivity * Time.fixedUnscaledDeltaTime;
            _verticalRotation -= Input.GetAxis("Mouse Y") * _mouseSensitivity * Time.fixedUnscaledDeltaTime;
            _verticalRotation = Mathf.Clamp(_verticalRotation, -_vertLookAngleClamp, _vertLookAngleClamp);
            
            var newRotation = new Vector3(0, _horizontalRotation, 0);
            transform.localRotation = Quaternion.Euler(newRotation);
            newRotation = new Vector3(_verticalRotation, 0, 0) + transform.eulerAngles;
            
            _headTransform.rotation = Quaternion.Euler(newRotation);

            float resultFollowSpeed = _cameraFollowBodySmooth;
            
            if (Game.LocalPlayer.VehicleControls.controlledMachine)
                resultFollowSpeed *= 10;
            
            _headTransform.transform.position = Vector3.Lerp(_headTransform.transform.position,transform.position + transform.up * _playerHeadHeightTarget, resultFollowSpeed * Time.unscaledDeltaTime);
            vehicleHeadDummyTransform.position = _headTransform.transform.position;
        }

        
        public void SetCurrentCutsceneTargetTrasform(Transform _transform)
        {
            currentCutsceneTargetTransform = _transform;
        }
        void FollowCutSceneTargetTransform()
        {
            if (!currentCutsceneTargetTransform)
                return;
            
            _headTransform.position = Vector3.Lerp(_headTransform.position,currentCutsceneTargetTransform.position, Time.fixedUnscaledDeltaTime);
            _headTransform.rotation = Quaternion.Slerp(_headTransform.rotation,currentCutsceneTargetTransform.rotation, Time.fixedUnscaledDeltaTime);
        }


        public void SetCrouch(bool isCrouching)
        {
            _playerHeadHeightTarget = isCrouching ? _playerHeadHeightCrouch : _playerHeadHeight;
        }
        
        

        public void Death(Transform killer = null)
        {
            _killerToLookAt = killer;
            _headTransform.parent = transform;
        }
        public void Resurrect()
        {
            _headTransform.parent = null;
        }
    }
}