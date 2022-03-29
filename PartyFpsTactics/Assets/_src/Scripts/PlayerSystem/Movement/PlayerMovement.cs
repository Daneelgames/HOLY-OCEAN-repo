using System.Collections;
using System.Collections.Generic;
using MrPink.Health;
using Sirenix.OdinInspector;
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
        public float runSpeed = 8;
        public float crouchSpeed = 2;
        public float crouchRunSpeed = 3.5f;
        public float acceleration = 1;
        public float groundCheckRadius = 0.25f;
        private Vector3 _targetVelocity;
        private Vector2 _movementInput;
        private Vector3 _moveVector;
        private Vector3 _prevVelocity;
        private Vector3 _resultVelocity;
        public float coyoteTimeMax = 0.5f;
        private float coyoteTime = 0;

        [Header("Slopes")] 
        bool onSlope = false;
        private Vector3 slopeMoveDirection;
        private Vector3 slopeNormal;
        public float slopeRayHeight = 0.25f;
        public float slopeRayDistance = 0.5f;

        [Header("Crouching")] 
        public bool crouching = false;
        public CapsuleCollider topCollider;
        public CapsuleCollider bottomCollider;

        private Vector3 topColliderCenterStanding = new Vector3(0, 1.15f, 0);
        private float topColliderHeightStanding = 1.55731f;
        private Vector3 topColliderCenterCrouching = new Vector3(0, 0.3424235f, 0);
        private float topColliderHeightCrouching = 0.6848469f;
        
        private Vector3 bottomColliderCenterStanding = new Vector3(0, 0.5f, 0);
        private float bottomColliderHeightStanding = 1;
        private Vector3 bottomColliderCenterCrouching = new Vector3(0, 0.25f, 0);
        private float bottomColliderHeightCrouching = 0.5f;
        
        public Transform headTransform;


        private bool goingUpHill = false;
        public Transform rotator;
        public float rotatorSpeed = 10;
        public float minMaxRotatorAngle = 90;

        private bool _isDead = false;
        private bool canUseCoyoteTime = true;
        private float additinalFallForce;

        private GrindRail activeGrindRail;

        [ShowInInspector, ReadOnly]
        private MovementsState _state = new MovementsState();


        private void Start()
        {
            SetCrouch(false);
        }

        private void Update()
        {
            if (_isDead)
            {
                rotator.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(rotator.localEulerAngles.z, 0, rotatorSpeed * Time.deltaTime));
                _state.IsLeaning = false;
                return;
            }
        
            if (!LevelGenerator.Instance.levelIsReady)
                return;

            HandleCrouch();
            HandleMovement();
        }
        
        private void FixedUpdate()
        {
            if (_isDead)
                return;
            
            if (!LevelGenerator.Instance.levelIsReady)
                return;

            GroundCheck();
            SlopeCheck();
            
            if (activeGrindRail == null)
                ApplyFreeMovement();
            else
                ApplyGrindRailMovement();
        }
        

        private void HandleCrouch()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
                SetCrouch(!crouching);
        }
        
        private void SetCrouch(bool crouch)
        {
            if (narrativePlayer)
                return;
            
            if (crouching == crouch)
                return;

            if (!crouch)
            {
                if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.up, out var hit, 1f, 1 << 6))
                {
                    // found obstacle, can't stand
                    Debug.Log(hit.collider.name);
                    return;
                }
            }
            
            crouching = crouch;

            if (crouching)
            {
                topCollider.center =  topColliderCenterCrouching;
                topCollider.height = topColliderHeightCrouching;
                bottomCollider.center = bottomColliderCenterCrouching;
                bottomCollider.height = bottomColliderHeightCrouching;
            }
            else
            {
                topCollider.center =  topColliderCenterStanding;
                topCollider.height = topColliderHeightStanding;
                bottomCollider.center = bottomColliderCenterStanding;
                bottomCollider.height = bottomColliderHeightStanding;
            }
            
            Player.LookAround.SetCrouch(crouching);
        }
        
        private void HandleMovement()
        {
            float targetAngle;
            _state.IsLeaning = true;
            
            if (Input.GetKey(KeyCode.D) && !Physics.CheckSphere(headTransform.position + headTransform.right * 1, 0.25f, 1<<6))
                targetAngle = -minMaxRotatorAngle;
            else if (Input.GetKey(KeyCode.A) && !Physics.CheckSphere(headTransform.position + headTransform.right * -1, 0.25f, 1<<6))
                targetAngle = minMaxRotatorAngle;
            else
            {
                targetAngle = 0;
                _state.IsLeaning = false;
            }
            rotator.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(rotator.localEulerAngles.z, targetAngle, rotatorSpeed * Time.deltaTime));

            int hor = (int)Input.GetAxisRaw("Horizontal");
            int vert = (int)Input.GetAxisRaw("Vertical");
        
            bool moveInFrame = hor != 0 || vert != 0;

            _movementInput = new Vector2(hor, vert);
            _moveVector = transform.right * _movementInput.x + transform.forward * _movementInput.y;
        
            _moveVector.Normalize();
        
            if (onSlope)
                _moveVector = Vector3.ProjectOnPlane(_moveVector, slopeNormal);
        
            // jump
            if (Input.GetKeyDown(KeyCode.Space) && (_state.IsGrounded || coyoteTime > 0))
            {
                SetGrindRail(null);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
                StartCoroutine(CoyoteTimeCooldown());
                coyoteTime = 0;
            }
        
            if (Input.GetKey(KeyCode.LeftShift))
            {
                _state.IsRunning = moveInFrame;
                _state.IsMoving = false;

                if (!crouching)
                    _targetVelocity = _moveVector * runSpeed;
                else
                    _targetVelocity = _moveVector * crouchRunSpeed;
            }
            else
            {
                _state.IsMoving = moveInFrame;
                _state.IsRunning = false;
            
                if (!crouching)
                    _targetVelocity = _moveVector * walkSpeed;
                else
                    _targetVelocity = _moveVector * crouchSpeed;
            }    
        
            if (goingUpHill)
                _targetVelocity += Vector3.up * 2;
        
            _resultVelocity = Vector3.Lerp(_prevVelocity, _targetVelocity, Time.deltaTime * acceleration);
            _prevVelocity = _resultVelocity;
        }


        private void SlopeCheck()
        {
            if (!_state.IsGrounded)
            {
                onSlope = false;
                return;
            }
        
            if (Physics.Raycast(transform.position + Vector3.up * slopeRayHeight, Vector3.down, out var hit, slopeRayDistance, WalkableLayerMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.normal != Vector3.up)
                {
                    onSlope = true;
                    slopeNormal = hit.normal;
                }
                else
                    onSlope = false;
            }
            else
                onSlope = false;
        }

        private IEnumerator CoyoteTimeCooldown()
        {
            canUseCoyoteTime = false;
            yield return new WaitForSeconds(coyoteTimeMax);
            
            canUseCoyoteTime = true;
        }

        private void GroundCheck()
        {
            if (Physics.CheckSphere(transform.position, groundCheckRadius, WalkableLayerMask, QueryTriggerInteraction.Ignore))
            {
                _state.IsGrounded = true;
                additinalFallForce = 0;
                if (canUseCoyoteTime)
                    coyoteTime = 0;
            }
            else
            {
                if (_state.IsGrounded && canUseCoyoteTime)
                    coyoteTime = coyoteTimeMax;

                additinalFallForce += Time.deltaTime;
                _state.IsGrounded = false;
                
                if (canUseCoyoteTime && coyoteTime > 0)
                {
                    coyoteTime -= Time.deltaTime;
                }
            }
        }
    
        private void ApplyFreeMovement()
        {
            float resultGravity = 0;
            if (!_state.IsGrounded)
                resultGravity = gravity + additinalFallForce;
            else if (!onSlope)
                resultGravity = 1;

            rb.velocity = _resultVelocity + Vector3.down * resultGravity;
        }

        private void ApplyGrindRailMovement()
        {
            var targetTransform = activeGrindRail.GetTargetNode();
            if (targetTransform == null)
            {
                SetGrindRail(null);
                return;
            }
            
            rb.velocity = (targetTransform.position - transform.position).normalized * 10;
        }

        public void SetGrindRail(GrindRail rail)
        {
            activeGrindRail = rail;
        }
    
        
        public ScoringActionType GetCurrentScoringAction()
        {
            if (_state.IsLeaning)
            {
                if (!_state.IsGrounded)
                    return ScoringActionType.KillLeaningRangedOnJump;
                
                if (_state.IsRunning)
                    return ScoringActionType.KillLeaningRangedOnRun;
                
                if (_state.IsMoving)
                    return ScoringActionType.KillLeaningRangedOnMove;
                
                return ScoringActionType.KillLeaningRangedIdle;
            }
            
            if (!_state.IsGrounded)
                return ScoringActionType.KillRangedOnJump;
            
            if (_state.IsRunning)
                return ScoringActionType.KillRangedOnRun;
            
            if (_state.IsMoving)
                return ScoringActionType.KillRangedOnMove;
            
            return ScoringActionType.KillRangedIdle;
            
        }
        
        public void Death(Transform killer = null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.drag = 1;
            rb.angularDrag = 10;
            _isDead = true;
        }
    }
}