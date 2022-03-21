using System.Collections;
using MrPink.Health;
using UnityEngine;

namespace MrPink.PlayerSystem
{
    public class PlayerMovement : MonoBehaviour
    {
        public bool narrativePlayer = false;
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
        public float coyoteTimeMax = 0.5f;
        private float coyoteTime = 0;
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

        [Header("Crouching")] 
        public bool crouching = false;
        public CapsuleCollider topCollider;
        public CapsuleCollider bottomCollider;

        private Vector3 topColliderCenterStanding = new Vector3(0, 1.273521f, 0);
        private float topColliderHeightStanding = 1.55731f;
        private Vector3 topColliderCenterCrouching = new Vector3(0, 0.3424235f, 0);
        private float topColliderHeightCrouching = 0.6848469f;
        
        private Vector3 bottomColliderCenterStanding = new Vector3(0, 0.5f, 0);
        private float bottomColliderHeightStanding = 1;
        private Vector3 bottomColliderCenterCrouching = new Vector3(0, 0.25f, 0);
        private float bottomColliderHeightCrouching = 0.5f;
        
        public Transform headTransform;
        public float _playerHeadHeight = 1.8f;
        public float _playerHeadHeightCrouch = 0.5f;
        float _playerHeadHeightTarget;
        
        public float mouseSensitivity = 5;
        public float vertLookAngleClamp = 85;
        public float cameraFollowBodySmooth = 3;
        private float _vertRotation = 0.0f;
        private float _horRotation = 0.0f;
        private bool goingUpHill = false;
        public Transform rotator;
        public float rotatorSpeed = 10;
        public float minMaxRotatorAngle = 90;

        private bool dead = false;
        private Transform killerToLookAt;
        private bool canUseCoyoteTime = true;

        private void Start()
        {
            _playerHeadHeightTarget = _playerHeadHeight;
            headTransform.parent = null;
            SetCrouch(false);
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

            if (walkSpeed < 1)
            {
                walkSpeed = 1;
                runSpeed = 1;
            }

            GetCrouch();
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
            if (LevelGenerator.Instance.levelIsReady == false)
                return;
            
            if (Shop.Instance && Shop.Instance.IsActive)
                return;
        
            MouseLook();
        }

        void GetCrouch()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                SetCrouch(!crouching);
            }
        }
        
        void SetCrouch(bool crouch)
        {
            if (narrativePlayer)
                return;
            
            if (crouching == crouch)
                return;

            if (!crouch)
            {
                if (Physics.CheckBox(transform.position + Vector3.up * 1.25f, new Vector3(0.3f, 1f, 0.3f), transform.rotation , 1 << 6))
                {
                    // found obstacle, can't stand
                    return;
                }
            }
            
            crouching = crouch;

            if (crouching)
            {
                _playerHeadHeightTarget = _playerHeadHeightCrouch;
                
                /*
                print("topColliderCenterCrouching " + topColliderCenterCrouching);
                print("topColliderHeightCrouching " + topColliderHeightCrouching);
                
                print("topCollider.center crouch " + topCollider.center);
                print("topCollider.height crouch " + topCollider.height);
                */
                
                topCollider.center =  topColliderCenterCrouching;
                topCollider.height = topColliderHeightCrouching;
                bottomCollider.center = bottomColliderCenterCrouching;
                bottomCollider.height = bottomColliderHeightCrouching;
            }
            else
            {
                _playerHeadHeightTarget = _playerHeadHeight;
                
                /*
                print("topColliderCenterStanding " + topColliderCenterStanding);
                print("topColliderHeightStanding " + topColliderHeightStanding);
                
                print("topCollider.center stand " + topCollider.center);
                print("topCollider.height stand " + topCollider.height);
                */
                
                topCollider.center =  topColliderCenterStanding;
                topCollider.height = topColliderHeightStanding;
                bottomCollider.center = bottomColliderCenterStanding;
                bottomCollider.height = bottomColliderHeightStanding;
            }
        }
        
        void GetMovement()
        {
            if (Input.GetKey(KeyCode.D) && !Physics.CheckSphere(headTransform.position + headTransform.right * 1, 0.25f, 1<<6))
            {
                rotator.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(rotator.localEulerAngles.z, -minMaxRotatorAngle, rotatorSpeed * Time.deltaTime));
                leaning = true;
            }
            else if (Input.GetKey(KeyCode.A) && !Physics.CheckSphere(headTransform.position + headTransform.right * -1, 0.25f, 1<<6))
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
        
            // jump
            if (Input.GetKeyDown(KeyCode.Space) && (_grounded || coyoteTime > 0))
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
                StartCoroutine(CoyoteTimeCooldown());
                coyoteTime = 0;
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

        IEnumerator CoyoteTimeCooldown()
        {
            canUseCoyoteTime = false;
            yield return new WaitForSeconds(coyoteTimeMax);
            
            canUseCoyoteTime = true;
        }

        void GroundCheck()
        {
            if (Physics.CheckSphere(transform.position, 0.25f, WalkableLayerMask, QueryTriggerInteraction.Ignore))
            {
                _grounded = true;
                
                if (canUseCoyoteTime)
                    coyoteTime = 0;
            }
            else
            {
                if (_grounded && canUseCoyoteTime)
                {
                    coyoteTime = coyoteTimeMax;   
                }
                
                _grounded = false;
                
                if (canUseCoyoteTime && coyoteTime > 0)
                {
                    coyoteTime -= Time.deltaTime;
                }
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

            headTransform.transform.position = 
                Vector3.Lerp(headTransform.transform.position,transform.position + Vector3.up * _playerHeadHeightTarget, 
                    cameraFollowBodySmooth * Time.deltaTime);
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

        // TODO роутить смерть сверху-вниз, возможно компонентам добавить интерфейс
        public void Death(Transform killer = null)
        {
            killerToLookAt = killer;
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.drag = 1;
            rb.angularDrag = 10;
            Player.Weapon.Death();
            dead = true;
        }
    }
}