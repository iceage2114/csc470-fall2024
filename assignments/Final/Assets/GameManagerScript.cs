using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class GameManagerScript : MonoBehaviour
{
    public static GameManagerScript instance;

    [Header("Spawn Points")]
    public GameObject[] spawnPoints;  // Drag spawn points in the inspector

    [Header("Prefabs")]
    public GameObject deerPrefab;     // Drag deer prefab in the inspector
    public GameObject wolfPrefab;     // Drag wolf prefab in the inspector

    [Header("UI Buttons")]
    public Button startGameButton;
    public GameObject startGamePanel;
    public GameObject winPanel;
    public Button deerButton;         // Drag deer spawn button from UI
    public Button wolfButton;          // Drag wolf spawn button from UI

    [Header("Button Cooldown")]
    public float buttonCooldownTime; // 5 seconds cooldown
    private bool isDeerButtonOnCooldown = false;
    private bool isWolfButtonOnCooldown = false;

    [Header("Camera")]
    public Camera mainCamera;         // Main camera for raycasting

    [Header("Cold Weather Event")]
    public float coldWeatherProbability;  // 10% chance per update
    public int maxAnimalDeaths;              // Max number of animals that can die
    public float meadowDepletionAmount;   // 20% meadow depletion during cold event
    public TMP_Text eventNotificationText;           // UI Text to show cold weather event
    public float meadowHealth;

    [Header("Population Counter")]
    public TMP_Text populationCounterText;
    public TMP_Text populationStatusText;
    public int maxPopulation = 60;

    // Spawn mode to track which animal we're trying to spawn
    private enum SpawnMode
    {
        None,
        Deer,
        Wolf
    }
    private SpawnMode currentSpawnMode = SpawnMode.None;
    private LayerMask layerMask;
    private bool isGameStarted = false;

    // List to track spawned animals
    public List<GameObject> spawnedAnimals = new List<GameObject>();

    void OnEnable()
    {
        if (instance == null)
        {
            instance = this;
            Debug.Log("GameManagerScript instance created");
        }
        else
        {
            Destroy(this);
            Debug.LogWarning("Multiple GameManagerScript instances detected. Destroying duplicate.");
        }
    }

    void Start()
    {
        // Add listener for Start Game button
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(StartGame);
        }
        else
        {
            Debug.LogError("Start Game button is not assigned!");
        }

        // Set up layer mask
        layerMask = LayerMask.GetMask("ground");
        Debug.Log($"Layer mask for ground created: {layerMask}");

        // Add button listeners for spawning
        if (deerButton != null)
        {
            deerButton.onClick.AddListener(() =>
            {
                Debug.Log("Deer Button Clicked!");
                SetDeerSpawnMode();
            });
        }
        else
        {
            Debug.LogError("Deer button is not assigned!");
        }

        if (wolfButton != null)
        {
            wolfButton.onClick.AddListener(() =>
            {
                Debug.Log("Wolf Button Clicked!");
                SetWolfSpawnMode();
            });
        }
        else
        {
            Debug.LogError("Wolf button is not assigned!");
        }

        // Freeze game at the start
        Time.timeScale = 0;

        // Initial spawn logic is delayed until game starts
        Debug.Log("Game is frozen until the start button is clicked.");
    }

    void Update()
    {
        // Game logic runs only after the game has started
        if (!isGameStarted) return;

        HandleSpawning();
        CheckColdWeatherEvent();
    }

    public void StartGame()
    {
        isGameStarted = true; // Enable game logic
        Time.timeScale = 1; // Unfreeze the game

        // Hide the start button and panel
        if (startGameButton != null)
        {
            startGameButton.gameObject.SetActive(false);
        }

        if (startGamePanel != null)
        {
            startGamePanel.SetActive(false);
        }

        // Spawn initial animals and update UI
        SpawnAnimals();
        UpdatePopulationCounter();

        Debug.Log("Game started!");
    }

    // method to update the population counter
    void UpdatePopulationCounter()
    {
        if (populationCounterText != null)
        {
            // Remove null entries from the list first
            spawnedAnimals.RemoveAll(animal => animal == null);

            // Update the text to show current population and max population
            populationCounterText.text = $"Animals alive: {spawnedAnimals.Count}/{maxPopulation}";

            // Update population status text
            if (populationStatusText != null)
            {
                if (spawnedAnimals.Count < 10)
                {
                    populationStatusText.text = "Ecosystem Struggling";
                    populationStatusText.color = Color.red;
                }
                else if (spawnedAnimals.Count < 25)
                {
                    populationStatusText.text = "Ecosystem Recovering";
                    populationStatusText.color = Color.yellow;
                }
                else if (spawnedAnimals.Count < maxPopulation)
                {
                    populationStatusText.text = "Ecosystem Balanced";
                    populationStatusText.color = Color.green;
                }
                else
                {
                    populationStatusText.text = "Ecosystem Overcrowded";
                    populationStatusText.color = Color.red;

                    // Check for win condition
                    if (winPanel != null && !winPanel.activeSelf)
                    {
                        ShowWinPanel();
                    }
                }
            }
        }
    }

    void ShowWinPanel()
    {
        Time.timeScale = 0; // Pause the game
        winPanel.SetActive(true); // Display the Win Panel
        Debug.Log("🎉 Win Condition Reached! Congratulations!");
    }


    void CheckColdWeatherEvent()
    {
        // Random chance of cold weather event
        if (Random.value < coldWeatherProbability)
        {
            StartCoroutine(TriggerColdWeatherEvent());
        }
    }

    private IEnumerator StartButtonCooldown(Button button, string buttonType)
    {
        // Set cooldown flag to true
        if (buttonType == "Deer")
            isDeerButtonOnCooldown = true;
        else if (buttonType == "Wolf")
            isWolfButtonOnCooldown = true;

        // Store original color
        Color originalColor = button.colors.normalColor;
        
        // Create a color block with a grayed out color
        ColorBlock colors = button.colors;
        colors.normalColor = Color.gray;
        button.colors = colors;

        // Disable button interaction
        button.interactable = false;

        // Wait for cooldown time
        yield return new WaitForSeconds(buttonCooldownTime);

        // Reset color
        colors.normalColor = originalColor;
        button.colors = colors;

        // Enable button interaction
        button.interactable = true;

        // Reset cooldown flag
        if (buttonType == "Deer")
            isDeerButtonOnCooldown = false;
        else if (buttonType == "Wolf")
            isWolfButtonOnCooldown = false;
    }
    private IEnumerator TriggerColdWeatherEvent()
    {
        // Prevent multiple simultaneous events
        coldWeatherProbability = 0f;

        // Notify player
        if (eventNotificationText != null)
        {
            eventNotificationText.text = "A severe cold storm strikes the mountain!";
            eventNotificationText.color = Color.blue;
        }

        // Log the event
        Debug.Log("❄️ COLD WEATHER EVENT TRIGGERED ❄️");
        Debug.Log($"Current population: {spawnedAnimals.Count} animals");

        // Kill some random animals (only once)
        int deathCount = KillRandomAnimals();
        UpdatePopulationCounter();

        // Deplete meadow health
        meadowHealth -= meadowDepletionAmount;
        meadowHealth = Mathf.Clamp(meadowHealth, 0f, 100f);

        // Log the impact of the event
        Debug.Log($"🐺 {deathCount} animals perished");
        Debug.Log($"🌱 Meadow health reduced to: {meadowHealth}%");

        // Keep the visual effect active for a longer time
        float extendedDuration = 10f; // Duration in seconds for the cold weather effect to last
        float elapsedTime = 0f;

        while (elapsedTime < extendedDuration)
        {
            elapsedTime += Time.deltaTime;

            // Optionally, add any ongoing cold weather effects here
            // e.g., slow down animal movement, modify environment visuals, etc.

            yield return null; // Wait for the next frame
        }

        // Reset notification after the extended duration
        if (eventNotificationText != null)
        {
            eventNotificationText.text = "";
        }

        // Restore cold weather probability
        coldWeatherProbability = 0.00005f; // Restore default probability
    }


    int KillRandomAnimals()
    {
        // Shuffle the list to randomize selection
        spawnedAnimals.Shuffle();

        int deathCount = 0;
        int maxDeaths = Mathf.Min(maxAnimalDeaths, spawnedAnimals.Count);

        for (int i = 0; i < maxDeaths; i++)
        {
            if (spawnedAnimals[i] != null)
            {
                // Visual death effect
                Renderer renderer = spawnedAnimals[i].GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = Color.gray;
                }

                // Destroy after a short delay for dramatic effect
                Destroy(spawnedAnimals[i], 1f);
                
                deathCount++;
            }
        }

        // Remove destroyed animals from the list
        spawnedAnimals.RemoveAll(animal => animal == null);

        return deathCount;
    }

    void HandleSpawning()
    {
        // Check if we're in a spawn mode and the user has clicked
        if (currentSpawnMode != SpawnMode.None && Input.GetMouseButtonDown(0))
        {
            // Create a ray from the mouse position
            Ray mousePositionRay = mainCamera.ScreenPointToRay(Input.mousePosition);

            RaycastHit hitInfo;

            // Perform raycast to find where the mouse is pointing
            if (Physics.Raycast(mousePositionRay, out hitInfo, Mathf.Infinity, layerMask))
            {
                GameObject prefabToSpawn = null;

                // Determine which prefab to spawn based on the current mode
                switch (currentSpawnMode)
                {
                    case SpawnMode.Deer:
                        prefabToSpawn = deerPrefab;
                        break;
                    case SpawnMode.Wolf:
                        prefabToSpawn = wolfPrefab;
                        break;
                }

                if (prefabToSpawn != null)
                {
                    // Sample NavMesh position
                    NavMeshHit navMeshHit;
                    if (NavMesh.SamplePosition(hitInfo.point, out navMeshHit, 10f, NavMesh.AllAreas))
                    {
                        // Instantiate the selected prefab
                        GameObject spawnedAnimal = Instantiate(prefabToSpawn, navMeshHit.position, Quaternion.identity);
                        
                        // Track the spawned animal
                        spawnedAnimals.Add(spawnedAnimal);

                        // Update the population counter
                        UpdatePopulationCounter();

                        Debug.Log($"Spawned {currentSpawnMode} at {navMeshHit.position}");
                    }
                    else
                    {
                        Debug.LogWarning("Could not find a valid NavMesh position at the clicked location.");
                    }
                }

                // Reset spawn mode after spawning
                currentSpawnMode = SpawnMode.None;
            }
        }
    }
    void SpawnAnimals()
    {
        // More verbose error checking
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned in GameManagerScript!");
            return;
        }

        if (deerPrefab == null)
        {
            Debug.LogError("Deer prefab is missing! Please assign in inspector.");
            return;
        }

        if (wolfPrefab == null)
        {
            Debug.LogError("Wolf prefab is missing! Please assign in inspector.");
            return;
        }

        int deerCount = 0;
        int wolfCount = 0;

        // Spawn loop
        foreach (GameObject spawnPoint in spawnPoints)
        {
            if (spawnPoint == null)
            {
                Debug.LogWarning("Null spawn point encountered!");
                continue;
            }

            // Spawn deer
            if (deerCount < 25)
            {
                for (int i = 0; i < 5; i++)
                {
                    if (deerCount >= 25) break;

                    Vector3 spawnPosition = spawnPoint.transform.position + Random.insideUnitSphere * 5f;

                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(spawnPosition, out hit, 10f, NavMesh.AllAreas))
                    {
                        GameObject spawnedDeer = Instantiate(deerPrefab, hit.position, Quaternion.identity);
                        
                        if (spawnedDeer == null)
                        {
                            Debug.LogError("Failed to instantiate deer!");
                        }
                        else
                        {
                            spawnedAnimals.Add(spawnedDeer);
                            deerCount++;
                            Debug.Log($"Spawned deer {deerCount} at {hit.position}");
                        }
                    }
                }
            }
            // Spawn wolves
            else if (wolfCount < 8)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (wolfCount >= 8) break;

                    Vector3 spawnPosition = spawnPoint.transform.position + Random.insideUnitSphere * 5f;

                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(spawnPosition, out hit, 10f, NavMesh.AllAreas))
                    {
                        GameObject spawnedWolf = Instantiate(wolfPrefab, hit.position, Quaternion.identity);
                        
                        if (spawnedWolf == null)
                        {
                            Debug.LogError("Failed to instantiate wolf!");
                        }
                        else
                        {
                            spawnedAnimals.Add(spawnedWolf);
                            wolfCount++;
                            Debug.Log($"Spawned wolf {wolfCount} at {hit.position}");
                        }
                    }
                }
            }

            // Stop if we've reached our target counts
            if (deerCount >= 25 && wolfCount >= 8)
                break;
        }
        UpdatePopulationCounter();

        // Log the actual spawned counts
        Debug.Log($"Spawned {deerCount} deer and {wolfCount} wolves");
    }

    // Method called by the Deer button
    public void SetDeerSpawnMode()
    {
        Debug.Log("SetDeerSpawnMode method called");

        if (deerPrefab == null)
        {
            Debug.LogError("Deer prefab is missing!");
            return;
        }

        currentSpawnMode = SpawnMode.Deer;
        
        // Start cooldown for deer button
        if (deerButton != null)
        {
            StartCoroutine(StartButtonCooldown(deerButton, "Deer"));
        }

        Debug.Log("Deer spawn mode activated. Click on the ground to spawn.");
    }

    public void SetWolfSpawnMode()
    {
        Debug.Log("SetWolfSpawnMode method called");

        if (wolfPrefab == null)
        {
            Debug.LogError("Wolf prefab is missing!");
            return;
        }

        currentSpawnMode = SpawnMode.Wolf;
        
        // Start cooldown for wolf button
        if (wolfButton != null)
        {
            StartCoroutine(StartButtonCooldown(wolfButton, "Wolf"));
        }

        Debug.Log("Wolf spawn mode activated. Click on the ground to spawn.");
    }
    public void TrackNewAnimal(GameObject animal)
    {
        if (animal != null)
        {
            spawnedAnimals.Add(animal);
            UpdatePopulationCounter(); // Update the population counter immediately
            Debug.Log($"{animal.name} added to population. Current population: {spawnedAnimals.Count}");
        }
    }

}

public static class ListExtensions 
{
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
