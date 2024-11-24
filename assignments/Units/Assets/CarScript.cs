using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CarScript : MonoBehaviour
{
    public GameObject carPanel;
    public Button depositButton;
    public Button driveAwayButton;
    public TextMeshProUGUI carGasAmountText;
    public TextMeshProUGUI playerGasText;
    
    public float maxGasCapacity;
    public float depositRate;
    private float currentGasAmount;
    
    private PlayerCameraController playerController;
    private bool isPlayerInTrigger = false;
    private bool isDepositing = false;
    
    // Reference to the GameManager
    private GameManagerScript gameManager;
    
    void Start()
    {
        currentGasAmount = 0f; // Car starts with empty tank
        
        if (carPanel != null)
        {
            carPanel.SetActive(false);
        }
        
        if (depositButton != null)
        {
            depositButton.onClick.AddListener(StartGasDeposit);
        }

        if (driveAwayButton != null)
        {
            driveAwayButton.onClick.AddListener(DriveAway);
        }

        // Find the GameManager in the scene
        gameManager = FindObjectOfType<GameManagerScript>();
        
        UpdateUI();
    }
    
    void Update()
    {
        UpdateUI();

        if (isDepositing)
        {
            DepositGas();
        }
    }
    
    void OnMouseDown()
    {
        if (isPlayerInTrigger)
        {
            carPanel.SetActive(!carPanel.activeSelf);
        }
    }
    
    void UpdateUI()
    {
        if (carGasAmountText != null)
        {
            carGasAmountText.text = $"Car Gas: {currentGasAmount:F1}/{maxGasCapacity:F1}";
        }
        
        if (playerGasText != null && playerController != null)
        {
            playerGasText.text = $"Player Gas: {playerController.currentGas:F1}/{playerController.maxGasCapacity:F1}";
        }
        
        if (depositButton != null)
        {
            depositButton.interactable = isPlayerInTrigger && playerController != null && 
                                       playerController.currentGas > 0 && 
                                       currentGasAmount < maxGasCapacity && 
                                       !isDepositing;
        }

        if (driveAwayButton != null)
        {
            if (currentGasAmount == 100) {
                driveAwayButton.interactable = true;
            }
        }
    }

    void StartGasDeposit()
    {
        if (playerController != null && isPlayerInTrigger && !isDepositing)
        {
            isDepositing = true;
            if (depositButton != null)
            {
                depositButton.interactable = false;
            }
        }
    }

    void DepositGas()
    {
        if (playerController != null && playerController.currentGas > 0 && currentGasAmount < maxGasCapacity)
        {
            float transferAmount = depositRate * Time.deltaTime;
            transferAmount = Mathf.Min(transferAmount, playerController.currentGas);
            transferAmount = Mathf.Min(transferAmount, maxGasCapacity - currentGasAmount);

            if (transferAmount > 0)
            {
                playerController.currentGas -= transferAmount;
                currentGasAmount += transferAmount;
                UpdateUI();
            }
            else
            {
                isDepositing = false;
            }
        }
        else
        {
            isDepositing = false;
        }
    }

    void DriveAway()
    {
        if (Mathf.Approximately(currentGasAmount, maxGasCapacity))
        {
            if (carPanel != null)
            {
                carPanel.SetActive(false);
            }

            if (gameManager != null)
            {
                gameManager.ShowVictoryPanel();
            }
            
            gameObject.SetActive(false);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        PlayerCameraController controller = other.GetComponent<PlayerCameraController>();
        if (controller != null)
        {
            isPlayerInTrigger = true;
            playerController = controller;
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        PlayerCameraController controller = other.GetComponent<PlayerCameraController>();
        if (controller != null)
        {
            isPlayerInTrigger = false;
            playerController = null;
            isDepositing = false;
            
            if (carPanel != null)
            {
                carPanel.SetActive(false);
            }
        }
    }
}