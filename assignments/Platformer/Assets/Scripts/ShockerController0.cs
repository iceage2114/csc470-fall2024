using UnityEngine;
using TMPro;

public class ShockerController : MonoBehaviour
{
    public TMP_Text loseText;
    public float speed = 10f;
    private bool isGameActive = true;

    void Start()
    {
        if (loseText != null)
        {
            loseText.gameObject.SetActive(false);
        }

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
            LoseGame();
        }
    }

    private void LoseGame()
    {
        isGameActive = false;
        
        if (loseText != null)
        {
            loseText.gameObject.SetActive(true);
            loseText.text = "You Lose!";
        }

        Time.timeScale = 0;
            
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            MonoBehaviour[] scripts = player.GetComponents<MonoBehaviour>();
            foreach(MonoBehaviour script in scripts)
            {
                script.enabled = false;
            }
        }
    }
}