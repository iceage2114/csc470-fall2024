using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    public GameObject playerPrefab;
    public float moveSpeed = 5f;
    
    public float maxGasCapacity = 100f;
    public float gasCollectionRate = 10f;
    public float gasDepositRate = 15f;
    public float currentGas = 0f;
    private Vector3 movement;
    private GameObject spawnedPlayer;

    private GameObject collidedObject;
    
    private bool isInRVTrigger = false;
    private bool isInCarTrigger = false;

    void Start()
    {
        if (playerPrefab != null)
        {
            spawnedPlayer = Instantiate(playerPrefab, transform.position, Quaternion.identity);
        }
    }

    void Update()
    {
        float hAxis = Input.GetAxis("Horizontal");
        float vAxis = Input.GetAxis("Vertical");

        Vector3 flatCameraForward = transform.forward;
        flatCameraForward.y = 0;
        movement = flatCameraForward.normalized * moveSpeed * vAxis;
        movement += transform.right * moveSpeed * hAxis;
    }

    void FixedUpdate()
    {
        if (collidedObject == null)
        {
            transform.position += movement * Time.fixedDeltaTime;

            if (spawnedPlayer != null)
            {
                spawnedPlayer.transform.position = transform.position;
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("rv"))
        {
            isInRVTrigger = true;
            Debug.Log("Entered RV trigger - Start collecting gas");
        }
        else if (other.CompareTag("car"))
        {
            isInCarTrigger = true;
            Debug.Log("Entered Car trigger - Start depositing gas");
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("rv"))
        {
            isInRVTrigger = false;
            Debug.Log("Exited RV trigger - Stop collecting gas");
        }
        else if (other.CompareTag("car"))
        {
            isInCarTrigger = false;
            Debug.Log("Exited Car trigger - Stop depositing gas");
        }
    }
}  