using MrPink.Health;
using UnityEngine;

namespace MrPink.PlayerSystem
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        public LayerMask WalkableLayerMask;

        public Rigidbody rb;
        public float gravity = 5;
        public float jumpForce = 300;
        public float walkSpeed = 5;
        public float runSpeed = 5;
        public float acceleration = 1;
        private bool _grounded;
        private Vector3 _targetVelocity;
        private Vector2 _movementInput;
        private Vector3 _moveVector;
        private Vector3 _prevVelocity;
        private Vector3 _resultVelocity;
    
        bool leaning = false;
        bool moving = false;
        bool running = false;

        [Header("Slopes")] 
        bool onSlope = false;
        private Vector3 slopeMoveDirection;
        private Vector3 slopeNormal;
        public float slopeRayHeight = 0.25f;
        public float slopeRayDistance = 0.5f;
        public float slopeRayRadius = 0.25f;
        
        public Transform headTransform;
        public float mouseSensitivity = 5;
        public float vertLookAngleClamp = 85;
        public float cameraFollowBodySmooth = 3;
        private float _playerHeadHeight;
        private float _vertRotation = 0.0f;
        private float _horRotation = 0.0f;
        private bool goingUpHill = false;
        public Transform rotator;
        public float rotatorSpeed = 10;
        public float minMaxRotatorAngle = 90;

        private bool dead = false;
        private Transform killerToLookAt;

        private void Start()
        {
            _playerHeadHeight = headTransform.localPosition.y;
            headTransform.parent = null;
        }

        private void Update()
        {
            if (dead)
            {
                rotator.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(rotator.localEulerAngles.z, 0, rotatorSpeed * Time.deltaTime));
                leaning = false;
                return;
            }
        
            if (!LevelGenerator.Instance.levelIsReady)
                return;
        
            /*
        if (Input.GetKeyDown(KeyCode.Z))
        {
            walkSpeed--;
            runSpeed--;
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            walkSpeed++;
            runSpeed++;
        }
        */

            if (walkSpeed < 1)
            {
                walkSpeed = 1;
                runSpeed = 1;
            }
            GetMovement();
        }
        private void FixedUpdate()
        {
            if (dead)
                return;
            if (!LevelGenerator.Instance.levelIsReady)
                return;

            GroundCheck();
            SlopeCheck();
            ApplyMovement();
        }

        private void LateUpdate()
        {
            if (Shop.Instance.IsActive)
                return;
        
            MouseLook();
        }

        void GetMovement()
        {
            if ((Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.D)) && !Physics.CheckSphere(headTransform.position + headTransform.right * 1, 0.25f, 1<<6))
            {
                rotator.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(rotator.localEulerAngles.z, -minMaxRotatorAngle, rotatorSpeed * Time.deltaTime));
                leaning = true;
            }
            else if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.A)) && !Physics.CheckSphere(headTransform.position + headTransform.right * -1, 0.25f, 1<<6))
            {
                rotator.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(rotator.localEulerAngles.z, minMaxRotatorAngle, rotatorSpeed * Time.deltaTime));
                leaning = true;
            }
            else
            {
                rotator.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(rotator.localEulerAngles.z, 0, rotatorSpeed * Time.deltaTime));
                leaning = false;
            }

            int hor = (int)Input.GetAxisRaw("Horizontal");
            int vert = (int)Input.GetAxisRaw("Vertical");
        
            bool moveInFrame = hor != 0 || vert != 0;

            _movementInput = new Vector2(hor, vert);
            _moveVector = transform.right * _movementInput.x + transform.forward * _movementInput.y;
        
            _moveVector.Normalize();
        
            if (onSlope)
                _moveVector = Vector3.ProjectOnPlane(_moveVector, slopeNormal);
        
            if (Input.GetKeyDown(KeyCode.Space) && _grounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            }
        
            if (Input.GetKey(KeyCode.LeftShift))
            {
                running = moveInFrame;
                moving = false;

                _targetVelocity = _moveVector * runSpeed;
            }
            else
            {
                moving = moveInFrame;
                running = false;
            
                _targetVelocity = _moveVector * walkSpeed;
            }    
        
            if (goingUpHill)
                _targetVelocity += Vector3.up * 2;
        
            _resultVelocity = Vector3.Lerp(_prevVelocity, _targetVelocity, Time.deltaTime * acceleration);
            _prevVelocity = _resultVelocity;
        }


        void SlopeCheck()
        {
            if (!_grounded)
            {
                onSlope = false;
                return;
            }
        
            //if (Physics.SphereCast(transform.position + Vector3.up * slopeRayHeight, slopeRayRadius, Vector3.down, out var hit, slopeRayDistance, WalkableLayerMask, QueryTriggerInteraction.Ignore))
            if (Physics.Raycast(transform.position + Vector3.up * slopeRayHeight, Vector3.down, out var hit, slopeRayDistance, WalkableLayerMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.normal != Vector3.up)
                {
                    onSlope = true;
                    slopeNormal = hit.normal;
                }
                else
                {
                    onSlope = false;
                }
            }
            else
            {
                onSlope = false;
            }
        }

        void GroundCheck()
        {
            if (Physics.CheckSphere(transform.position, 0.25f, WalkableLayerMask, QueryTriggerInteraction.Ignore))
            {
                _grounded = true;

            }
            else
            {
                _grounded = false;
            }
        }
    
        void ApplyMovement()
        {
            float resultGravity = 0;
            if (!_grounded)
                resultGravity = gravity;
            else if (!onSlope)
                resultGravity = 1;

            rb.velocity = _resultVelocity + Vector3.down * resultGravity;
        }
    
        void MouseLook()
        {
            if (dead && killerToLookAt != null)
            {
                headTransform.rotation = Quaternion.Lerp(headTransform.rotation, Quaternion.LookRotation(killerToLookAt.position - headTransform.position), Time.deltaTime);
                return;
            }
        
            Vector3 newRotation = new Vector3(0, _horRotation, 0);
            _horRotation += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            newRotation = new Vector3(0, _horRotation, 0);
            transform.localRotation = Quaternion.Euler(newRotation);
        
            _vertRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
            _vertRotation = Mathf.Clamp(_vertRotation, -vertLookAngleClamp, vertLookAngleClamp);

            //newRotation = new Vector3(_vertRotation, 0, 0) + transform.localEulerAngles;
            //headTransform.localRotation = Quaternion.Euler(newRotation);
            newRotation = new Vector3(_vertRotation, 0, 0) + transform.eulerAngles;
            headTransform.rotation = Quaternion.Euler(newRotation);

            headTransform.transform.position = Vector3.Lerp(headTransform.transform.position,transform.position + Vector3.up * _playerHeadHeight, cameraFollowBodySmooth * Time.deltaTime);
        }

        public ScoringActionType GetCurrentScoringAction()
        {
            ScoringActionType currentAction = ScoringActionType.NULL;
        
            if (!_grounded)
                currentAction = ScoringActionType.KillRangedOnJump;
            else if (running)
                currentAction = ScoringActionType.KillRangedOnRun;
            else if (moving)
                currentAction = ScoringActionType.KillRangedOnMove;
            else
                currentAction = ScoringActionType.KillRangedIdle;
        
            if (leaning)
            {
                if (!_grounded)
                    currentAction = ScoringActionType.KillLeaningRangedOnJump;
                else if (running)
                    currentAction = ScoringActionType.KillLeaningRangedOnRun;
                else if (moving)
                    currentAction = ScoringActionType.KillLeaningRangedOnMove;
                else
                    currentAction = ScoringActionType.KillLeaningRangedIdle;
            }
        
            return currentAction;
        }

        public void Death(Transform killer = null)
        {
            killerToLookAt = killer;
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.drag = 1;
            rb.angularDrag = 10;
            PlayerWeaponControls.Instance.Death();
            dead = true;
        }
    }
}