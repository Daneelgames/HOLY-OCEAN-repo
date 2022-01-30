using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance;
    [Header("Movement")]
    public LayerMask WalkableLayerMask;

    public Rigidbody rb;
    public float gravity = 5;
    public float walkSpeed = 5;
    public float runSpeed = 5;
    public float acceleration = 1;
    private bool _grounded;
    private Vector3 _targetVelocity;
    private Vector2 _movementInput;
    private Vector3 _moveVector;
    private Vector3 _prevVelocity;
    private Vector3 _resultVelocity;

    [Header("Slopes")] 
    bool onSlope = false;
    private Vector3 slopeMoveDirection;
    private Vector3 slopeNormal;
    public float slopeRayHeight = 0.25f;
    public float slopeRayDistance = 0.5f;
    public float slopeRayRadius = 0.25f;

    [Header("Camera")] 
    public Camera MainCam;
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
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _playerHeadHeight = headTransform.localPosition.y;
        headTransform.parent = null;
    }

    private void Update()
    {
        GetMovement();
    }

    void GetMovement()
    {
        if (Input.GetKey(KeyCode.E))
        {
            rotator.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(rotator.localEulerAngles.z, -minMaxRotatorAngle, rotatorSpeed * Time.deltaTime));
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            rotator.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(rotator.localEulerAngles.z, minMaxRotatorAngle, rotatorSpeed * Time.deltaTime));
        }
        else
        {
            rotator.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(rotator.localEulerAngles.z, 0, rotatorSpeed * Time.deltaTime));
        }

        int hor = (int)Input.GetAxisRaw("Horizontal");
        int vert = (int)Input.GetAxisRaw("Vertical");
        
        _movementInput = new Vector2(hor, vert);
        _moveVector = transform.right * _movementInput.x + transform.forward * _movementInput.y;
        _moveVector.Normalize();
        
        if (onSlope)
            _moveVector = Vector3.ProjectOnPlane(_moveVector, slopeNormal);
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * 100, ForceMode.Impulse);
        }
        
        if (Input.GetKey(KeyCode.LeftShift))
            _targetVelocity = _moveVector * runSpeed;
        else
            _targetVelocity = _moveVector * walkSpeed;    
        
        if (goingUpHill)
            _targetVelocity += Vector3.up * 2;
        
        _resultVelocity = Vector3.Lerp(_prevVelocity, _targetVelocity, Time.deltaTime * acceleration);
        _prevVelocity = _resultVelocity;
    }

    private void FixedUpdate()
    {
        GroundCheck();
        SlopeCheck();
        ApplyMovement();
    }

    private void LateUpdate()
    {
        MouseLook();
    }

    void SlopeCheck()
    {
        if (!_grounded)
        {
            onSlope = false;
            return;
        }
        
        if (Physics.SphereCast(transform.position + Vector3.up * slopeRayHeight, slopeRayRadius, Vector3.down, out var hit, slopeRayDistance,
            WalkableLayerMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.normal != Vector3.up)
            {
                Debug.Log(hit.point);
                onSlope = true;
                slopeNormal = hit.normal;
            }
            else
            {
                Debug.Log(hit.point);
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
}