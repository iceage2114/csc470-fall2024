using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerScript : MonoBehaviour
{
    public GameObject startPopupCanvas;
    public GameObject victoryPanel;
    private bool gameStarted = false;
    private bool gameEnded = false;

    void Start()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        if (startPopupCanvas != null)
        {
            startPopupCanvas.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    void Update()
    {
        if (!gameStarted && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
        {
            StartGame();
        }
    }

    void StartGame()
    {
        gameStarted = true;
        
        if (startPopupCanvas != null)
        {
            startPopupCanvas.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    public void ShowVictoryPanel()
    {
        if (!gameEnded)
        {
            gameEnded = true;
            
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }
        }
    }
}