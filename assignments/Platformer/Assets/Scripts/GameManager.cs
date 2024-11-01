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
    
    float coinSpacer = 2.5f;              
    float segmentHorizontalSpacing = 30f;  
    Vector3 startPosition = new Vector3(0, 0, -2.2f);  
    float maxCoinHeight = 20f;            
    public static bool isGameActive = false;

    void Start()
    {
        SpawnGameObjects();
        Time.timeScale = 0f;
        StartCoroutine(CountdownToStart());
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
            float randomX = Random.Range(20f, 600f);
            float randomY = Random.Range(0f, maxCoinHeight);
            Vector3 missilePosition = new Vector3(randomX, randomY, -2.2f);
            Instantiate(missilePrefab, missilePosition, Quaternion.identity);
        }

        for(int i = 0; i < 7; i++) {
            float randomX = Random.Range(20f, 600f);
            float randomY = Random.Range(0f, maxCoinHeight);
            Vector3 shockerPosition = new Vector3(randomX, randomY, -2.2f);
            Instantiate(shockerPrefab, shockerPosition, Quaternion.identity);
        }
    }

    void Update()
    {
        
    }
}