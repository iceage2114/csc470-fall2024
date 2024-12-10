using UnityEngine;

public class CameraControllerScript : MonoBehaviour
{
    // Movement speed of the camera
    public float moveSpeed = 5f;

    // Zoom speed and limits
    public float zoomSpeed = 10f;
    public float minZoom = 2f;
    public float maxZoom = 10f;

    // Initial camera distance
    public float initialYDistance = -10f;

    // Camera component reference
    private Camera mainCamera;

    void Start()
    {
        // Get the main camera component
        mainCamera = Camera.main;

        // Set initial camera position
        // Moves the camera back along the Z-axis
        transform.position = new Vector3(
            transform.position.x,
            initialYDistance, 
            transform.position.z
        );
    }

    void Update()
    {
        // Movement and zoom code remains the same as in the previous script
        Vector3 moveDirection = Vector3.zero;

        float scale = 9f;

        if (Input.GetKey(KeyCode.W))
        {
            moveDirection += Vector3.forward * scale;
        }

        if (Input.GetKey(KeyCode.S))
        {
            moveDirection += Vector3.back * scale;
        }

        if (Input.GetKey(KeyCode.A))
        {
            moveDirection += Vector3.left * scale;
        }

        if (Input.GetKey(KeyCode.D))
        {
            moveDirection += Vector3.right * scale;
        }

        // Apply movement
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        // Handle zooming
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        
        if (mainCamera.orthographic)
        {
            mainCamera.orthographicSize = Mathf.Clamp(
                mainCamera.orthographicSize - scrollWheel * zoomSpeed, 
                minZoom, 
                maxZoom
            );
        }
        else
        {
            mainCamera.fieldOfView = Mathf.Clamp(
                mainCamera.fieldOfView - scrollWheel * zoomSpeed, 
                minZoom, 
                maxZoom
            );
        }
    }
}