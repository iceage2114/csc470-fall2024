using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : MonoBehaviour
{
    public CharacterController cc;
    float rotateSpeed = 50;
    float moveSpeed = 12;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float hAxis = Input.getAxis("Horizontal");
        float vAxis = Input.getAxis("Vertical");

        transform.Rotate(0, rotateSpeed * hAxis * Time.deltaTime, 0);

        Vector3 amountToMove = transform.forward * moveSpeed * vAxis;

        amountToMove *= Time.deltaTime;

        cc.Move(amountToMove);

        // if(Input.GetKeyDown(KeyCode.Space)) {
        //     cc.Move(transform.forward);
        // }

    }
}
