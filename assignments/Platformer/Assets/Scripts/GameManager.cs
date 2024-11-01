using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameObject coinPrefab;
    public GameObject missilePrefab;
    public GameObject shockerPrefab;
    public TMP_Text timerText;
    public TMP_Text loseText;
    public TMP_Text coinText;
    public TMP_Text winText;
    
    float coinSpacer = 2.5f;              
    float segmentHorizontalSpacing = 30f;  
    Vector3 startPosition = new Vector3(0, 0, -2.2f);  
    float maxCoinHeight = 20f;            
    public static bool isGameActive = false;
    
    private int coinsCollected = 0;
    private const int COINS_TO_WIN = 30;

    void Start()
    {
        if (loseText != null)
        {
            loseText.gameObject.SetActive(false);
        }
        if (winText != null)
        {
            winText.gameObject.SetActive(false);
        }
        if (coinText != null)
        {
            UpdateCoinText();
        }
        
        SpawnGameObjects();
        Time.timeScale = 0f;
        StartCoroutine(CountdownToStart());
    }

    private void UpdateCoinText()
    {
        coinText.text = $"Coins: {coinsCollected} / {COINS_TO_WIN}";
    }

    public void CollectCoin()
    {
        coinsCollected++;
        UpdateCoinText();
        
        if (coinsCollected >= COINS_TO_WIN)
        {
            ShowWinScreen();
        }
    }

    public void ShowWinScreen()
    {
        isGameActive = false;
        Time.timeScale = 0f;
        
        if (winText != null)
        {
            winText.gameObject.SetActive(true);
            winText.text = "You Win!\nCollected all 30 coins!";
        }

        DisablePlayer();
    }

    public void ShowLoseScreen()
    {
        isGameActive = false;
        Time.timeScale = 0f;
        
        if (loseText != null)
        {
            loseText.gameObject.SetActive(true);
            loseText.text = "You Lose!";
        }

        DisablePlayer();
    }

    private void DisablePlayer()
    {
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

    private IEnumerator CountdownToStart()
    {
        if (timerText != null)
        {
            timerText.text = "Jetpack Joyride!          Collect coins while dodging the missiles and electric shockers!                            Press any key to start!";
        }
        yield return new WaitUntil(() => Input.anyKeyDown);
        
        isGameActive = true;
        Time.timeScale = 1f;
        
        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }
    }

    private void SpawnGameObjects()
    {
        for(int i = 0; i < 10; i++) {
            float randomHorizontalOffset = Random.Range(-5f, 5f);  
            float randomVerticalOffset = Random.Range(0f, maxCoinHeight);
            
            Vector3 segmentBasePosition = startPosition + 
                                        (transform.right * (i * segmentHorizontalSpacing + 20f)) +
                                        (transform.right * randomHorizontalOffset) +      
                                        (Vector3.up * randomVerticalOffset);              

            for(int j = 0; j < 5; j++) {
                Vector3 position = segmentBasePosition + transform.right * j * coinSpacer;
                GameObject coin = Instantiate(coinPrefab, position, Quaternion.identity);
            }
        }

        for(int i = 0; i < 10; i++) {
            float randomX = Random.Range(20f, 500f);
            float randomY = Random.Range(0f, maxCoinHeight);
            Vector3 missilePosition = new Vector3(randomX, randomY, -2.2f);
            Instantiate(missilePrefab, missilePosition, Quaternion.identity);
        }

        for(int i = 0; i < 7; i++) {
            float randomX = Random.Range(20f, 500f);
            float randomY = Random.Range(0f, maxCoinHeight);
            Vector3 shockerPosition = new Vector3(randomX, randomY, -2.2f);
            Instantiate(shockerPrefab, shockerPosition, Quaternion.identity);
        }
    }
}