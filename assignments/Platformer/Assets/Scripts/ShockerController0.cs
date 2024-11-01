using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;

public class ShockerController : MonoBehaviour
{
    public float speed = 25f;
    private bool isGameActive = true;
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
        if (!isGameActive) return;
        
        transform.Translate(Vector3.left * speed * Time.deltaTime);

        float camDistance = Vector3.Distance(transform.position, Camera.main.transform.position);
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(transform.position);
        if (viewportPoint.x < -0.1f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isGameActive = false;
            if (gameManager != null)
            {
                gameManager.ShowLoseScreen();
            }
        }
    }
}