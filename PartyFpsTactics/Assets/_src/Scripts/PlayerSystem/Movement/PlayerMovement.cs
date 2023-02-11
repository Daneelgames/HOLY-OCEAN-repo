using System.Collections;
using Crest;
using FishNet.Object;
using MrPink.Health;
using NWH.DWP2.WaterObjects;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;

namespace MrPink.PlayerSystem
{
    public class PlayerMovement : NetworkBehaviour
    {
        [Header("Movement")] public LayerMask WalkableLayerMask;
        [SerializeField][ReadOnly]private Rigidbody movingPlatformRigidbody;
        public Rigidbody rb;
        
        [SerializeField] private WaterObject playerWaterObject;
        [BoxGroup("VERTICAL")]public float jumpForce = 300;
        [BoxGroup("VERTICAL")]public float dashForce = 100;
        [BoxGroup("VERTICAL")] [SerializeField] [ReadOnly] private Vector3 jumpVelocity;
        [BoxGroup("VERTICAL")] [SerializeField] private float _coyoteTimeMax = 0.5f;
        [BoxGroup("VERTICAL")] [SerializeField] [ReadOnly] private bool canUseCoyoteTime = true;
        [BoxGroup("VERTICAL")] [SerializeField] [ReadOnly] private float _coyoteTime = 0;
        
        [BoxGroup("VERTICAL")] public bool useGravity { get; private set; } = true;
        [BoxGroup("VERTICAL")]public float gravity = 5;
        [BoxGroup("VERTICAL")][SerializeField]float gravityChangeSmooth = 5f;
        [BoxGroup("VERTICAL")][SerializeField][ReadOnly]float newTargetGravity = 0f;
        [BoxGroup("VERTICAL")][SerializeField][ReadOnly]float resultGravity = 0;
        [BoxGroup("VERTICAL")]public float fallDamageThreshold = 10;
        [BoxGroup("VERTICAL")]public int fallDamage = 100;
        [SerializeField] private Vector3 currentVehicleExitVelocity;
        [BoxGroup("HORIZONTAL")]public float walkSpeed = 5;
        [BoxGroup("HORIZONTAL")]public float runSpeed = 8;
        [BoxGroup("HORIZONTAL")]public float crouchSpeed = 2;
        [BoxGroup("HORIZONTAL")]public float crouchRunSpeed = 3.5f;
        [BoxGroup("HORIZONTAL")]public float acceleration = 1;
        [BoxGroup("HORIZONTAL")][SerializeField] private float slowDownModifier = 1;
        
        public float groundCheckRadius = 0.25f;
        public float climbCheckRadius = 1;
        private Vector3 _targetVelocity;
        private Vector2Int _movementInput;
        private Vector3 _moveVector;
        private Vector3 _prevVelocity;
        private Vector3 _resultVelocity;


        private GrindRail activeGrindRail;


        [BoxGroup("STAMINA")] public float stamina = 100;
        [HideInInspector] public float staminaMax = 100;
        [BoxGroup("STAMINA")]public float staminaMin = -20;
        [BoxGroup("STAMINA")][SerializeField] float climbStaminaCost = 5;
        [BoxGroup("STAMINA")][SerializeField] float climbMoveStaminaCost = 10;
        [BoxGroup("STAMINA")][SerializeField] float climbRunStaminaCost = 15;
        [BoxGroup("STAMINA")][SerializeField] float runStaminaCost = 10;
        [BoxGroup("STAMINA")][SerializeField] float runCrouchStaminaCost = 5;
        [BoxGroup("STAMINA")][SerializeField] private float jumpStaminaCost = 10;
        [BoxGroup("STAMINA")][SerializeField] private float idleStaminaRegen = 33;
        [BoxGroup("STAMINA")][SerializeField] private float moveStaminaRegen = 25;

        [Header("Slopes")]
        [SerializeField] [ReadOnly] private Vector3 slopeMoveDirection;
        [SerializeField] [ReadOnly] private Vector3 slopeNormal;
        [SerializeField] private float maxSlopeAngle = 50f;
        [SerializeField] private float _slopeRayHeight = 0.25f;
        [SerializeField] private float _slopeRayDistance = 0.5f;
        
        [BoxGroup("VAULTING")] [SerializeField] private bool checkVault = false;
        [BoxGroup("VAULTING")] [SerializeField] private float vaultRaycastDistance = 0.75f;
        [BoxGroup("VAULTING")] [SerializeField] private float middleRaycastHeight = 1f;
        [BoxGroup("VAULTING")] [SerializeField] private float bottomRaycastHeight = 0.5f;
        [BoxGroup("VAULTING")] [SerializeField] private float autoVaultPower = 5;

        [BoxGroup("CROUCH")] public CapsuleCollider topCollider;
        [BoxGroup("CROUCH")] public CapsuleCollider bottomCollider;

        private Vector3 topColliderCenterStanding = new Vector3(0, 1.15f, 0);
        private float topColliderHeightStanding = 1.55731f;
        private Vector3 topColliderCenterCrouching = new Vector3(0, 0.3424235f, 0);
        private float topColliderHeightCrouching = 0.6848469f;

        private Vector3 bottomColliderCenterStanding = new Vector3(0, 0.5f, 0);
        private float bottomColliderHeightStanding = 1;
        private Vector3 bottomColliderCenterCrouching = new Vector3(0, 0.25f, 0);
        private float bottomColliderHeightCrouching = 0.5f;

        public Transform headTransform;

        public Transform rotator;
        public float rotatorSpeed = 10;
        public float minMaxRotatorAngle = 90;

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

        [ShowInInspector, ReadOnly] public MovementsState State { get; private set; } = new MovementsState();

        public Vector3 MoveVector => _moveVector;

        private float heightToFallFrom = 0;

        private float rbInitAngularDrag;
        private float rbInitDrag;

        private void Start()
        {
            staminaMax = stamina;
            rbInitDrag = rb.drag;
            rbInitAngularDrag = rb.angularDrag;
            SetCrouch(false);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (IsOwner == false)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }   

            //temp?
            rb.useGravity = false;
        }

        private void Update()
        {
            if (IsOwner == false)
                return;

            if (Game.LocalPlayer.VehicleControls.controlledMachine != null)
            {
                if (rb.isKinematic == false || rb.useGravity)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }       
            }
            else
            {
                if (rb.isKinematic || rb.useGravity == false)
                {
                    rb.isKinematic = false;
                    rb.useGravity = false;
                }          
            }

            if (_isDead == false)
                HandleJump();
            HandleStamina();
            HandleMovement();
            if (Game.LocalPlayer.VehicleControls.controlledMachine)
            {
                State.IsRunning = false;
                _resultVelocity = Vector3.zero;
                /*
                rb.MovePosition(Game.LocalPlayer.VehicleControls.controlledMachine.sitTransform.position);
                rb.MoveRotation(Game.LocalPlayer.VehicleControls.controlledMachine.sitTransform.rotation);
                */

                return;
            }

            if (_isDead)
            {
                rb.isKinematic = false;
                rb.useGravity = false;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rotator.localEulerAngles = new Vector3(0, 0,
                    Mathf.LerpAngle(rotator.localEulerAngles.z, 0, rotatorSpeed * Time.deltaTime));
                State.IsLeaning = false;
                return;
            }
            
            HandleCrouch();
            GetUnderwater();


            if (Shop.Instance.IsActive || PlayerInventoryUI.Instance.IsActive)
                return;
        }

        private void FixedUpdate()
        {
            if (IsOwner == false)
                return;

            if (_isDead)
                return;

            ReduceJumpVelocity();
            
            if (Game.LocalPlayer.VehicleControls.controlledMachine)
            {
                rb.MovePosition(Game.LocalPlayer.VehicleControls.controlledMachine.sitTransform.position);
                rb.MoveRotation(Game.LocalPlayer.VehicleControls.controlledMachine.sitTransform.rotation);
                return;
            }
            
            GroundCheck();
            if (checkVault)
                AutoVaultCheck();
            ClimbingCheck();
            SlopeCheck();


            if (activeGrindRail == null)
                ApplyFreeMovement();
            else
                ApplyGrindRailMovement();
        }

        void ReduceJumpVelocity()
        {
            if (State.IsGrounded && canUseCoyoteTime)
                jumpVelocity = Vector3.zero;
            if (jumpVelocity.magnitude > 0)
                jumpVelocity = Vector3.Lerp(jumpVelocity, Vector3.zero, Time.fixedUnscaledDeltaTime);
        }
        
        public void AddVehicleExitForce(Vector3 machineVelocity)
        {
            return;
            currentVehicleExitVelocity = machineVelocity;
        }
        
        void GetUnderwater()
        {
            if (OceanRenderer.Instance == null)
                State.IsUnderwater = false;
            else
            {
                State.IsUnderwater = OceanRenderer.Instance.ViewerHeightAboveWater < 1;
            }
            
            if (State.IsUnderwater && playerWaterObject.gameObject.activeInHierarchy == false)
            {
                playerWaterObject.gameObject.SetActive(true);
                return;
            }
            if (State.IsUnderwater == false && playerWaterObject.gameObject.activeInHierarchy)
                playerWaterObject.gameObject.SetActive(false);
            
        }
        
        void HandleJump()
        {
            if (Input.GetKeyDown(KeyCode.Space) && (Game.LocalPlayer.VehicleControls.controlledMachine != null || State.IsGrounded || State.IsClimbing || _coyoteTime > 0) && stamina > 0)
            {
                if (Game.LocalPlayer.VehicleControls.controlledMachine != null)
                    Game.LocalPlayer.VehicleControls.RequestVehicleAction(Game.LocalPlayer.VehicleControls.controlledMachine);
                var vel = rb.velocity;
                vel.y = 0;
                rb.velocity = vel;

                var x = _movementInput.x;
                var z = _movementInput.y;
                if (x == 0 && z == 0)
                    z = -1;
                var additionalForce = new Vector3(x, 0, z).normalized;
                Jump(additionalForce);
            }
        }

        private float targetStaminaScaler = 1;

        void HandleStamina()
        {
            if (Game.LocalPlayer.VehicleControls.controlledMachine)
            {
                ChangeStamina(idleStaminaRegen * Time.deltaTime);
                return;
            }

            if (State.IsClimbing && !State.IsGrounded)
            {
                targetStaminaScaler = climbStaminaCost;

                if (State.IsRunning)
                    targetStaminaScaler = climbRunStaminaCost;
                else if (State.IsMoving)
                    targetStaminaScaler = climbMoveStaminaCost;

                ChangeStamina(-1 * targetStaminaScaler * Time.deltaTime);

                return;
            }

            if (!State.IsRunning)
            {
                // MOVE
                targetStaminaScaler = idleStaminaRegen;
                if (State.IsMoving)
                    targetStaminaScaler = moveStaminaRegen;
                if (stamina < staminaMax)
                {
                    ChangeStamina(targetStaminaScaler * Time.deltaTime);
                }

                return;
            }

            // IF RUN
            if (!State.IsCrouching)
                targetStaminaScaler = runStaminaCost;
            else
                targetStaminaScaler = runCrouchStaminaCost;
            ChangeStamina(-1 * targetStaminaScaler * Time.deltaTime);
        }

        public void ChangeStamina(float offset)
        {
            stamina = Mathf.Clamp(stamina + offset, staminaMin, staminaMax);
        }


        private void HandleCrouch()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
                SetCrouch(!State.IsCrouching);
        }

        public void SetCollidersTrigger(bool trigger)
        {
            bottomCollider.isTrigger = trigger;
            topCollider.isTrigger = trigger;
        }
        public void DisableColliders(bool enable)
        {
            bottomCollider.enabled = enable;
            topCollider.enabled = enable;
        }

        public void SetCrouch(bool crouch)
        {
            if (State.IsCrouching == crouch)
                return;

            if (!crouch)
            {
                if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.up, out var hit, 1f,
                    WalkableLayerMask))
                {
                    // found obstacle, can't stand
                    Debug.Log(hit.collider.name);
                    return;
                }
            }

            State.IsCrouching = crouch;

            if (State.IsCrouching)
            {
                topCollider.center = topColliderCenterCrouching;
                topCollider.height = topColliderHeightCrouching;
                bottomCollider.center = bottomColliderCenterCrouching;
                bottomCollider.height = bottomColliderHeightCrouching;
            }
            else
            {
                topCollider.center = topColliderCenterStanding;
                topCollider.height = topColliderHeightStanding;
                bottomCollider.center = bottomColliderCenterStanding;
                bottomCollider.height = bottomColliderHeightStanding;
            }

            Game.LocalPlayer.LookAround.SetCrouch(State.IsCrouching);
        }

    private void HandleMovement()
        {
            float targetAngle;
            State.IsLeaning = true;

            if (Input.GetKey(KeyCode.D) && !Physics.CheckSphere(headTransform.position + headTransform.right * 1, 0.25f,
                WalkableLayerMask))
                targetAngle = -minMaxRotatorAngle;
            else if (Input.GetKey(KeyCode.A) && !Physics.CheckSphere(headTransform.position + headTransform.right * -1,
                0.25f, WalkableLayerMask))
                targetAngle = minMaxRotatorAngle;
            else
            {
                targetAngle = 0;
                State.IsLeaning = false;
            }

            rotator.localEulerAngles = new Vector3(0, 0,
                Mathf.LerpAngle(rotator.localEulerAngles.z, targetAngle, rotatorSpeed * Time.fixedUnscaledDeltaTime));

            int hor = (int)Input.GetAxisRaw("Horizontal");
            int vert = (int)Input.GetAxisRaw("Vertical");

            bool moveInFrame = hor != 0 || vert != 0;

            _movementInput = new Vector2Int(hor, vert);

            if (State.IsClimbing || State.IsGrounded == false)
                _moveVector = Game.LocalPlayer.MainCamera.transform.right * _movementInput.x +
                              Game.LocalPlayer.MainCamera.transform.forward * _movementInput.y;
            else
                _moveVector = transform.right * _movementInput.x + transform.forward * _movementInput.y;

            _moveVector.Normalize();

            if (State.IsOnSlope)
                _moveVector = GetSlopeMovementDirection(_moveVector);

            float scaler = 1;

            if (stamina < 0.1f && State.IsGrounded)
                scaler = 0.66f;
            else if (Game.LocalPlayer.Interactor.carryingPortableRb)
                scaler = 0.66f;

            // RUNNING
            if (Input.GetKey(KeyCode.LeftShift) && stamina > 0 && (State.IsGrounded || State.IsClimbing == false))
            {
                State.IsRunning = moveInFrame;
                State.IsMoving = false;

                if (!State.IsCrouching)
                    _targetVelocity = _moveVector * runSpeed * scaler;
                else
                    _targetVelocity = _moveVector * crouchRunSpeed * scaler;
            }
            else
            {
                State.IsMoving = moveInFrame;
                State.IsRunning = false;

                if (!State.IsCrouching)
                    _targetVelocity = _moveVector * walkSpeed * scaler;
                else
                    _targetVelocity = _moveVector * crouchSpeed * scaler;
            }

            _resultVelocity = Vector3.Lerp(_prevVelocity, _targetVelocity, Time.fixedUnscaledDeltaTime * acceleration);

            if (State.IsClimbing)
                newTargetGravity = 0;
            else if (State.CanVault)
            {
                newTargetGravity = 0;
                _resultVelocity += Vector3.up * autoVaultPower;
            }
            else if (State.IsUnderwater)
                newTargetGravity = 1f;
            else if (State.IsGrounded)
            {
                newTargetGravity = State.IsOnSlope ? 0.1f : 1f;
            }
            else
                newTargetGravity = gravity;

            resultGravity = State.IsClimbing ? newTargetGravity : Mathf.Lerp(resultGravity, newTargetGravity, gravityChangeSmooth * Time.fixedUnscaledDeltaTime);
            
            if (useGravity == false)
                resultGravity = 0;
            
            if (currentVehicleExitVelocity.magnitude > 0)
                currentVehicleExitVelocity = Vector3.Lerp(currentVehicleExitVelocity, Vector3.zero, 10f * Time.fixedUnscaledDeltaTime);
            
            _resultVelocity += currentVehicleExitVelocity + Vector3.down * resultGravity;

            _prevVelocity = _resultVelocity;
        }


        public void Jump(Vector3 dashDir)
        {
            SetGrindRail(null);
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            //rb.AddRelativeForce(Vector3.up * jumpForce + additionalForce, ForceMode.Impulse);

            StartCoroutine(CoyoteTimeCooldown());
            ChangeStamina(-1 * jumpStaminaCost);
            _coyoteTime = 0;
            
            var jumpLocalVel = Vector3.up * jumpForce + dashDir * dashForce;
            jumpVelocity = headTransform.TransformDirection(jumpLocalVel);
        }

        private void SlopeCheck()
        {
            if (!State.IsGrounded)
            {
                State.IsOnSlope = false;
                return;
            }

            if (Physics.Raycast(transform.position + Vector3.up * _slopeRayHeight, Vector3.down, out var hit,
                _slopeRayDistance, WalkableLayerMask, QueryTriggerInteraction.Ignore))
            {
                var slopeAngle = Vector3.Angle(Vector3.up, hit.normal);
                if (slopeAngle < maxSlopeAngle && slopeAngle > 0)
                {
                    State.IsOnSlope = true;
                    slopeNormal = hit.normal;
                }
                else
                    State.IsOnSlope = false;
            }
            else
                State.IsOnSlope = false;
        }

        Vector3 GetSlopeMovementDirection(Vector3 moveDir)
        {
            return Vector3.ProjectOnPlane(moveDir, slopeNormal).normalized;
        }

        private IEnumerator CoyoteTimeCooldown()
        {
            canUseCoyoteTime = false;
            yield return new WaitForSeconds(_coyoteTimeMax);

            canUseCoyoteTime = true;
        }

        private float lastVelocityInAirY = 0;

        private void GroundCheck()
        {
            if (Game._instance == null || Game.LocalPlayer == null)
                return;

            if (Physics.CheckSphere(transform.position, groundCheckRadius, WalkableLayerMask,
                QueryTriggerInteraction.Ignore))
            {
                if (!State.IsGrounded && Game.LocalPlayer.VehicleControls.controlledMachine == null &&
                    !State.IsClimbing)
                {
                    if (PlayerFootsteps.Instance)
                        PlayerFootsteps.Instance.PlayLanding();

                    if (transform.position.y + fallDamageThreshold < heightToFallFrom)
                    {
                        Game.LocalPlayer.Health.Damage(fallDamage, DamageSource.Environment);
                    }
                    if (canUseCoyoteTime)
                        rb.velocity = Vector3.zero;
                }

                lastVelocityInAirY = 1;
                heightToFallFrom = transform.position.y;
                State.IsGrounded = true;
                if (canUseCoyoteTime)
                    _coyoteTime = 0;
            }
            else if (!State.IsClimbing)
            {
                if (lastVelocityInAirY >= 0 && rb.velocity.y < 0)
                {
                    lastVelocityInAirY = rb.velocity.y;
                    heightToFallFrom = transform.position.y;
                }

                if (State.IsGrounded && canUseCoyoteTime)
                    _coyoteTime = _coyoteTimeMax;

                State.IsGrounded = false; 
                // in air, not climbing

                if (canUseCoyoteTime && _coyoteTime > 0)
                {
                    _coyoteTime -= Time.deltaTime;
                }
            }
            else
            {
                State.IsGrounded = false;
            }
        }

    void AutoVaultCheck()
    {
        if (Physics.Raycast(transform.position + Vector3.up * bottomRaycastHeight, _moveVector.normalized, vaultRaycastDistance, WalkableLayerMask))
        {
            // has an obstacle on bottom
            Debug.DrawLine(transform.position + Vector3.up * bottomRaycastHeight, transform.position + Vector3.up * bottomRaycastHeight + _moveVector.normalized * vaultRaycastDistance, Color.cyan);
            
            if (Physics.Raycast(transform.position + Vector3.up * middleRaycastHeight, _moveVector.normalized, vaultRaycastDistance, WalkableLayerMask))
            {
                // has an obstacle on the middle
                // too high for auto climbing
                
                State.CanVault = false;
                Debug.DrawLine(transform.position + Vector3.up * middleRaycastHeight, transform.position + Vector3.up * middleRaycastHeight +_moveVector.normalized* vaultRaycastDistance, Color.red);
                return;
            }
            State.CanVault = true;
            Debug.DrawLine(transform.position + Vector3.up * middleRaycastHeight, transform.position + Vector3.up * middleRaycastHeight + _moveVector.normalized * vaultRaycastDistance, Color.cyan);
            return;
        }
        
        State.CanVault = false;
        
        Debug.DrawLine(transform.position + Vector3.up * bottomRaycastHeight, transform.position + Vector3.up * bottomRaycastHeight + _moveVector.normalized * vaultRaycastDistance, Color.red);
        Debug.DrawLine(transform.position + Vector3.up * middleRaycastHeight, transform.position + Vector3.up * middleRaycastHeight + _moveVector.normalized * vaultRaycastDistance, Color.red);
    }

    private RaycastHit[] hitInfoClimb;
        void ClimbingCheck()    
        {

            hitInfoClimb = Physics.SphereCastAll(Game.LocalPlayer.MainCamera.transform.position, climbCheckRadius,
                Vector3.up, climbCheckRadius, GameManager.Instance.AllSolidsMask, QueryTriggerInteraction.Ignore);
            
            
            if (stamina <= 0 || Input.GetKey(KeyCode.LeftShift) == false)
            {
                State.IsClimbing = false;
                return;
            }
            
            var newClimbing = hitInfoClimb.Length > 0;
            if (newClimbing && State.IsClimbing == false)
                rb.velocity = Vector3.zero;
            State.IsClimbing = newClimbing;
            if (State.IsClimbing)
            {
                heightToFallFrom = transform.position.y;
            }
        }

        private Vector3 movingPlatformVelocity;        
        private void ApplyFreeMovement()
        {
            //rb.useGravity = false;
            
            if (State.IsUnderwater) // IN WATER
            {
                rb.AddForce(_resultVelocity);
                return;
            }

            movingPlatformVelocity = movingPlatformRigidbody ? movingPlatformRigidbody.velocity : Vector3.zero;
            
            if (State.IsMoving == false && State.IsRunning == false && State.IsGrounded) // slow down
            {
                rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero + movingPlatformVelocity, slowDownModifier * Time.unscaledDeltaTime);   
            }
            else
            {
                //rb.AddForce(_resultVelocity, ForceMode.VelocityChange);   
                rb.velocity = _resultVelocity + jumpVelocity + movingPlatformVelocity;   
            }
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

        public void SetMovingPlatform(Rigidbody platformRigidbody, bool exit = false)
        {
            if (exit)
            {
                if (platformRigidbody == movingPlatformRigidbody)
                    movingPlatformRigidbody = null;
                return;
            }
            
            // enter
            if (platformRigidbody == movingPlatformRigidbody)
                return;

            movingPlatformRigidbody = platformRigidbody;
        }
        
        public IEnumerator TeleportToPosition(Vector3 pos)
        {
            Debug.Log("TeleportToPosition " + pos);
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            yield return null;
            yield return new WaitForFixedUpdate();
            rb.MovePosition(pos);
            rb.transform.position = pos;
            yield return new WaitForFixedUpdate();
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        public void Death(Transform killer = null)
        {
            SetCollidersTrigger(false);
            rb.isKinematic = false;
            //rb.useGravity = true;
            rb.drag = 1;
            rb.angularDrag = 10;
        }

        public void Resurrect()
        {
            SetCollidersTrigger(false);
            rb.isKinematic = false;
            //rb.useGravity = true;
            rb.drag = rbInitDrag;
            rb.angularDrag = rbInitAngularDrag;
        }

        public void SetNoGravity(bool noGravity)
        {
            useGravity = !noGravity;
        }
    }
}