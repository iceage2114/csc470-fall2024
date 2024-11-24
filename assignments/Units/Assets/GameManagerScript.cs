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
        // Ensure victory panel is hidden at start
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        // Show start popup
        if (startPopupCanvas != null)
        {
            startPopupCanvas.SetActive(true);
        }

        // Set time scale to 0 to pause the game
        Time.timeScale = 0f;
    }

    void Update()
    {
        // Check for any key press to start the game
        if (!gameStarted && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
        {
            StartGame();
        }
    }

    void StartGame()
    {
        gameStarted = true;
        
        // Hide start popup
        if (startPopupCanvas != null)
        {
            startPopupCanvas.SetActive(false);
        }

        // Resume game time
        Time.timeScale = 1f;
    }

    public void ShowVictoryPanel()
    {
        if (!gameEnded)
        {
            gameEnded = true;
            
            // Show victory panel
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }
            
            // Keep time running so player can still look around
            // Time.timeScale = 1f; // Game continues running
        }
    }
}