using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;
    private Vector3 movement;
    public Camera mainCamera;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal"); // A & D
        float verticalInput = Input.GetAxisRaw("Vertical");     // W & S

        movement = new Vector3(horizontalInput, 0f, verticalInput).normalized;
    }

    void FixedUpdate()
    {
        rb.velocity = movement * moveSpeed;
    }
}