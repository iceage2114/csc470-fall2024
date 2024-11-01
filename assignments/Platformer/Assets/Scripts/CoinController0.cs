using UnityEngine;

public class CoinController : MonoBehaviour
{
    public float speed = 15f;
    
    void Start()
    {
        // Position coin just off the right side of the camera
        float camDistance = Vector3.Distance(transform.position, Camera.main.transform.position);
        Vector3 rightMostPoint = Camera.main.ViewportToWorldPoint(new Vector3(1.1f, 0.5f, camDistance));
        transform.position = new Vector3(rightMostPoint.x, transform.position.y, transform.position.z);
    }

    void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);
        
        // Optional: Destroy coin if it goes too far left off screen
        float camDistance = Vector3.Distance(transform.position, Camera.main.transform.position);
        Vector3 leftMostPoint = Camera.main.ViewportToWorldPoint(new Vector3(-0.1f, 0.5f, camDistance));
        if (transform.position.x < leftMostPoint.x)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // You can add score/pickup logic here
            Destroy(gameObject);
        }
    }
}