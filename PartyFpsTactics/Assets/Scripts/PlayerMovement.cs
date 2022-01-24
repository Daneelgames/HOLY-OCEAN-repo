using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public LayerMask WalkableLayerMask;

    private bool grounded;
    public Rigidbody rb;
    public float gravity = 5;
    public float walkSpeed = 5;
    public float acceleration = 1;
    
    [Header("Camera")]
    public Transform headTransform;
    public float mouseSensitivity = 5;
    public float vertLookAngleClamp = 85;
    private float playerHeadHeight;
    public float cameraFollowBodySmooth = 3;

    private Vector2 movementInput;
    private Vector3 moveVector;
    private Vector3 currentVelocity;
    private Vector3 resultVelocity;
    
    
    private float vertRotation = 0.0f;
    private float horRotation = 0.0f;

    private void Start()
    {
        playerHeadHeight = headTransform.localPosition.y;
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
            grounded = true;
        else 
            grounded = false;
    }
    
    void Movement()
    {
        movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        moveVector = (transform.right * movementInput.x + transform.forward * movementInput.y).normalized;
        
        resultVelocity = Vector3.Lerp(currentVelocity, moveVector * walkSpeed, Time.deltaTime * acceleration);
        currentVelocity = resultVelocity;

        float resultGravity = 1;
        if (!grounded)
            resultGravity = gravity;
        
        rb.velocity = resultVelocity + Vector3.down * resultGravity;
    }
    
    void MouseLook()
    {
        Vector3 newRotation = new Vector3(0, horRotation, 0);
        horRotation += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        newRotation = new Vector3(0, horRotation, 0);
        transform.localRotation = Quaternion.Euler(newRotation);
        
        vertRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        vertRotation = Mathf.Clamp(vertRotation, -vertLookAngleClamp, vertLookAngleClamp);
        
        newRotation = new Vector3(vertRotation, 0, 0) + transform.localEulerAngles;
        headTransform.localRotation = Quaternion.Euler(newRotation);

        headTransform.transform.position = Vector3.Lerp(headTransform.transform.position,transform.position + Vector3.up * playerHeadHeight, cameraFollowBodySmooth * Time.deltaTime);
    }
}