using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class RVScript : MonoBehaviour
{
    public GameObject rvPanel;
    public Button collectButton;
    public TextMeshProUGUI gasAmountText;
    public TextMeshProUGUI playerGasText;
    
    public float maxGasAvailable;
    public float collectionRate;
    private float currentGasAvailable;
    
    private PlayerCameraController playerController;
    private bool isPlayerInTrigger = false;
    private bool isCollecting = false;
    
    void Start()
    {
        currentGasAvailable = maxGasAvailable;
        
        if (rvPanel != null)
        {
            rvPanel.SetActive(false);
        }
        
        if (collectButton != null)
        {
            collectButton.onClick.AddListener(StartGasCollection);
        }
        
        UpdateUI();
    }
    
    void Update()
    {
        UpdateUI();

        if (isCollecting)
        {
            CollectGas();
        }
    }
    
    void OnMouseDown()
    {
        if (isPlayerInTrigger)
        {
            rvPanel.SetActive(!rvPanel.activeSelf);
        }
    }
    
    void UpdateUI()
    {
        if (gasAmountText != null)
        {
            gasAmountText.text = $"RV Gas: {currentGasAvailable:F1}/{maxGasAvailable:F1}";
        }
        
        if (playerGasText != null && playerController != null)
        {
            playerGasText.text = $"Player Gas: {playerController.currentGas:F1}/{playerController.maxGasCapacity:F1}";
        }
        
        if (collectButton != null)
        {
            collectButton.interactable = isPlayerInTrigger && currentGasAvailable > 0 && !isCollecting;
        }
    }

    void StartGasCollection()
    {
        if (playerController != null && isPlayerInTrigger && !isCollecting)
        {
            isCollecting = true;
            if (collectButton != null)
            {
                collectButton.interactable = false;
            }
        }
    }

    void CollectGas()
    {
        if (playerController != null && currentGasAvailable > 0 && playerController.currentGas < playerController.maxGasCapacity)
        {
            float transferAmount = collectionRate * Time.deltaTime;
            transferAmount = Mathf.Min(transferAmount, currentGasAvailable);
            transferAmount = Mathf.Min(transferAmount, playerController.maxGasCapacity - playerController.currentGas);

            if (transferAmount > 0)
            {
                playerController.currentGas += transferAmount;
                currentGasAvailable -= transferAmount;
                UpdateUI();
            }
            else
            {
                isCollecting = false;
            }
        }
        else
        {
            isCollecting = false;
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
            isCollecting = false;
            
            if (rvPanel != null)
            {
                rvPanel.SetActive(false);
            }
        }
    }
}