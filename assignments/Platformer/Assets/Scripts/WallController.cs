using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallController : MonoBehaviour
{
    private const float ROTATION_SPEED = 10f; // Degrees per second

    void Update()
    {
        transform.Rotate(Vector3.up * ROTATION_SPEED * Time.deltaTime);
    }
}
