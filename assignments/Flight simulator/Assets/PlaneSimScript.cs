using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlaneScript : MonoBehaviour
{
    public GameObject cameraObject;
    public Terrain terrain; 
    public TMP_Text scoreText;
    public TMP_Text winText;
    public TMP_Text loseText;
    public TMP_Text timerText;

    private int score = 0;
    private const int totalCollectibles = 7;
    private HashSet<Collider> collectedObjects = new HashSet<Collider>();
    private string scorePrefix = "Score: ";
    private string scoreSuffix = "";
    public float gameTime = 100f;

    float forwardSpeed = 35f;
    float xRotationSpeed = 90f;
    float yRotationSpeed = 70f;
    float zRotationSpeed = 90f; 
    float boostTime;
    private float remainingTime;
    private bool isGameActive = false;

    void Start()
    {
        ExtractText();
        UpdateScoreText();
        winText.gameObject.SetActive(false);
        loseText.gameObject.SetActive(false);
        remainingTime = gameTime;
        UpdateTimerText();
        StartCoroutine(CountdownToStart());
    }

    private IEnumerator CountdownToStart()
    {
        timerText.text = "Press any key to start!";
        yield return new WaitUntil(() => Input.anyKeyDown);
        isGameActive = true;
        remainingTime = gameTime;
    }

    private void ExtractText()
    {
        string initialText = scoreText.text;
        int scoreIndex = initialText.IndexOf("0/7");
        if (scoreIndex != -1)
        {
            scorePrefix = initialText.Substring(0, scoreIndex);
            scoreSuffix = initialText.Substring(scoreIndex + 3);
        }
    }

    void Update()
    {
        if (!isGameActive || score >= totalCollectibles) 
        {
            return;
        }

        remainingTime -= Time.deltaTime;
        UpdateTimerText();

        if (remainingTime <= 0)
        {
            LoseGame();
            return;
        }

        float hAxis = Input.GetAxis("Horizontal");
        float vAxis = Input.GetAxis("Vertical");

        float pitchInput = Input.GetAxis("Vertical");
        float yawInput = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
        float rollInput = Input.GetKey(KeyCode.E) ? -1 : Input.GetKey(KeyCode.Q) ? 1 : 0;
        
        Vector3 amountToRotate = new Vector3(0,0,0);
        amountToRotate.x = vAxis * xRotationSpeed;
        amountToRotate.y = yawInput * yRotationSpeed;
        amountToRotate.z = rollInput * zRotationSpeed;
        amountToRotate *= Time.deltaTime;
        transform.Rotate(amountToRotate, Space.Self);

        transform.position += transform.forward * forwardSpeed * Time.deltaTime;

        Vector3 cameraPosition = transform.position;
        cameraPosition += -transform.forward * 15f;
        cameraPosition += Vector3.up * 5f;
        cameraObject.transform.position = cameraPosition;
        cameraObject.transform.LookAt(transform.position);

        
    }

    public void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("collectable") && !collectedObjects.Contains(other))
        {
            collectedObjects.Add(other); 
            score++;
            UpdateScoreText();
            Destroy(other.gameObject);

            remainingTime += 10f;

            if (score >= totalCollectibles)
            {
                WinGame();
            }
        }
        else if (!other.CompareTag("collectable"))
        {
            LoseGame();
        }
    }


    private void UpdateTimerText()
    {
        if (isGameActive)
        {
            timerText.text = $"Time: {Mathf.Ceil(remainingTime)}";
        }
    }

    private void UpdateScoreText()
    {
        scoreText.text = $"{scorePrefix}{score}/{totalCollectibles}{scoreSuffix}";
    }

    private void WinGame()
    {
        isGameActive = false;
        winText.gameObject.SetActive(true);
        winText.text = "You Win!";
        Time.timeScale = 0f;
    }

    private void LoseGame()
    {
        isGameActive = false;
        loseText.gameObject.SetActive(true);
        loseText.text = "You Lose!";
        Time.timeScale = 0f;  
    }
}