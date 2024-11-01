using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetpackController : MonoBehaviour
{
    public CharacterController controller;
    float jetpackForce = 16f;
    float maxHeight = 20f;
    float verticalVelocity = 0f;
    float gravity = -30f;
    float drag = 5f;
    float maxFallSpeed = -25f;

    private float swayAmount = 1.5f;
    private float swaySpeed = 1.2f;
    private float swayTime = 0f;
    private float originalX;

    void Start()
    {
        originalX = transform.position.x;
    }
    
    void Update()
    {
        if (transform.position.y >= maxHeight) {
            verticalVelocity = -5f;
        }
        
        if (Input.GetKey(KeyCode.Space) && transform.position.y < maxHeight) {
            verticalVelocity = Mathf.Lerp(verticalVelocity, jetpackForce, Time.deltaTime * 3f);
        } else {
            if (verticalVelocity > 0) {
                verticalVelocity += (gravity * drag) * Time.deltaTime;
            } else {
                verticalVelocity += gravity * Time.deltaTime;
                verticalVelocity = Mathf.Max(verticalVelocity, maxFallSpeed);
            }
        }

        swayTime += Time.deltaTime;
        float swayOffset = Mathf.Sin(swayTime * swaySpeed) * swayAmount;
        
        Vector3 movement = new Vector3(swayOffset - (transform.position.x - originalX), verticalVelocity, 0);
        controller.Move(movement * Time.deltaTime);

        if (controller.isGrounded && verticalVelocity < 0) {
            verticalVelocity = 0f;
        }
    }
}