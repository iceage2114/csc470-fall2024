using UnityEngine;

public class CameraControllerScript : MonoBehaviour
{
    public float moveSpeed;

    public float zoomSpeed = 10f;
    public float minZoom = 2f;
    public float maxZoom = 10f;

    public float initialYDistance = -10f;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        transform.position = new Vector3(
            transform.position.x,
            initialYDistance, 
            transform.position.z
        );
    }

    void Update()
    {
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

        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

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