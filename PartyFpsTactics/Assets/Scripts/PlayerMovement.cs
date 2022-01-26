using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
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
    public Transform headTransform;
    public float mouseSensitivity = 5;
    public float vertLookAngleClamp = 85;
    public float cameraFollowBodySmooth = 3;
    private float _playerHeadHeight;
    private float _vertRotation = 0.0f;
    private float _horRotation = 0.0f;

    private void Start()
    {
        _playerHeadHeight = headTransform.localPosition.y;
        headTransform.parent = null;
    }

    private void Update()
    {
        Movement();
        MouseLook();
        GroundCheck();
    }

    void GroundCheck()
    {
        if (Physics.CheckSphere(transform.position, 0.5f, WalkableLayerMask))
            _grounded = true;
        else 
            _grounded = false;
    }
    
    void Movement()
    {
        _movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        _moveVector = (transform.right * _movementInput.x + transform.forward * _movementInput.y).normalized;
        
        if (Input.GetKey(KeyCode.LeftShift))
            _targetVelocity = _moveVector * runSpeed;
        else
            _targetVelocity = _moveVector * walkSpeed;
        _resultVelocity = Vector3.Lerp(_currentVelocity, _targetVelocity, Time.deltaTime * acceleration);
        _currentVelocity = _resultVelocity;

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