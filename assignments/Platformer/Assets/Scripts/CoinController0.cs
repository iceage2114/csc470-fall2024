using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;

public class CoinController : MonoBehaviour
{
    public float speed = 15f;
    private GameManager gameManager;
    
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        
        float camDistance = Vector3.Distance(transform.position, Camera.main.transform.position);
        Vector3 rightMostPoint = Camera.main.ViewportToWorldPoint(new Vector3(1.1f, 0.5f, camDistance));
        transform.position = new Vector3(rightMostPoint.x, transform.position.y, transform.position.z);
    }

    void Update()
    {
        transform.Translate(Vector3.left * speed * Time.deltaTime);
        
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
            if (gameManager != null)
            {
                gameManager.CollectCoin();
            }
            Destroy(gameObject);
        }
    }
}