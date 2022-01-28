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
    private Vector3 _currentVelocity;
    private Vector3 _resultVelocity;

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
        
        _movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        _moveVector = (transform.right * _movementInput.x + transform.forward * _movementInput.y).normalized;
        
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
        
        _resultVelocity = Vector3.Lerp(_currentVelocity, _targetVelocity, Time.deltaTime * acceleration);
        _currentVelocity = _resultVelocity;
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        GroundCheck();
    }

    private void LateUpdate()
    {
        MouseLook();
    }

    void GroundCheck()
    {
        if (Physics.Raycast(transform.position,Vector3.down, out var hit, 0.1f, WalkableLayerMask))
        {
            _grounded = true;

            /*
            if (Vector3.Angle(rb.velocity, hit.normal) > 5)
                goingUpHill = true;
                */
        }
        else 
            _grounded = false;
    }
    
    void ApplyMovement()
    {
        float resultGravity = 1;
        if (!_grounded)
            resultGravity = gravity;
        
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
        
        newRotation = new Vector3(_vertRotation, 0, 0) + transform.localEulerAngles;
        headTransform.localRotation = Quaternion.Euler(newRotation);

        headTransform.transform.position = Vector3.Lerp(headTransform.transform.position,transform.position + Vector3.up * _playerHeadHeight, cameraFollowBodySmooth * Time.deltaTime);
    }
}