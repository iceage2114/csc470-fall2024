using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class GameManagerScript : MonoBehaviour
{
    public static GameManagerScript instance;

    [Header("Spawn Points")]
    public GameObject[] spawnPoints;  // Drag spawn points in the inspector

    [Header("Prefabs")]
    public GameObject deerPrefab;     // Drag deer prefab in the inspector
    public GameObject wolfPrefab;     // Drag wolf prefab in the inspector

    [Header("UI Buttons")]
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
    public Text eventNotificationText;           // UI Text to show cold weather event
    public float meadowHealth;

    // Spawn mode to track which animal we're trying to spawn
    private enum SpawnMode
    {
        None,
        Deer,
        Wolf
    }
    private SpawnMode currentSpawnMode = SpawnMode.None;
    private LayerMask layerMask;

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
        // Set up layer mask
        layerMask = LayerMask.GetMask("ground");
        Debug.Log($"Layer mask for ground created: {layerMask}");

        // Add button listeners
        if (deerButton != null)
        {
            deerButton.onClick.AddListener(() => {
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
            wolfButton.onClick.AddListener(() => {
                Debug.Log("Wolf Button Clicked!");
                SetWolfSpawnMode();
            });
        }
        else
        {
            Debug.LogError("Wolf button is not assigned!");
        }

        // Initial spawn of animals
        SpawnAnimals();
    }

    void Update()
    {
        // Handle initial animal spawning or manual spawning
        HandleSpawning();

        CheckColdWeatherEvent();
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
    IEnumerator TriggerColdWeatherEvent()
    {
        // Prevent multiple simultaneous events
        coldWeatherProbability = 0f;

        // Notify player
        if (eventNotificationText != null)
        {
            eventNotificationText.text = "A SEVERE COLD WAVE STRIKES THE MEADOW!";
            eventNotificationText.color = Color.blue;
        }

        // Vibrant debug logging
        Debug.Log("❄️ COLD WEATHER EVENT TRIGGERED ❄️");
        Debug.Log($"Current population: {spawnedAnimals.Count} animals");

        // Kill some random animals
        int deathCount = KillRandomAnimals();

        // Deplete meadow health
        meadowHealth -= meadowDepletionAmount * 100;
        meadowHealth = Mathf.Clamp(meadowHealth, 0f, 100f);

        // Temporary visual feedback
        Debug.Log($"🐺 {deathCount} animals perished");
        Debug.Log($"🌱 Meadow health reduced to: {meadowHealth}%");

        // Reset notification after a few seconds
        yield return new WaitForSeconds(3f);
        
        if (eventNotificationText != null)
        {
            eventNotificationText.text = "";
        }

        // Restore cold weather probability
        coldWeatherProbability = 0.1f;
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
            Debug.Log($"Mouse clicked in {currentSpawnMode} spawn mode");
            Debug.Log($"Camera: {mainCamera != null}");
            Debug.Log($"Layer mask: {layerMask}");
            Debug.Log($"Layer mask value: {layerMask.value}");

            // Create a ray from the mouse position
            Ray mousePositionRay = mainCamera.ScreenPointToRay(Input.mousePosition);
            Debug.Log($"Mouse position: {Input.mousePosition}");
            Debug.Log($"Ray origin: {mousePositionRay.origin}");
            Debug.Log($"Ray direction: {mousePositionRay.direction}");

            RaycastHit hitInfo;

            // Perform raycast to find where the mouse is pointing
            bool raycastHit = Physics.Raycast(mousePositionRay, out hitInfo, Mathf.Infinity, layerMask);
            
            Debug.Log($"Raycast result: {raycastHit}");
            
            if (raycastHit)
            {
                Debug.Log($"Raycast hit point: {hitInfo.point}");
                Debug.Log($"Hit collider: {hitInfo.collider.name}");
                Debug.Log($"Hit collider layer: {hitInfo.collider.gameObject.layer}");
            }
            else
            {
                Debug.LogWarning("Raycast did not hit anything on the ground layer!");
            }
            // Perform raycast to find where the mouse is pointing
            if (Physics.Raycast(mousePositionRay, out hitInfo, Mathf.Infinity, layerMask))
            {
                Debug.Log($"Raycast hit point: {hitInfo.point}");

                // Spawn based on current mode
                GameObject prefabToSpawn = null;
                switch (currentSpawnMode)
                {
                    case SpawnMode.Deer:
                        if (deerPrefab != null)
                        {
                            prefabToSpawn = deerPrefab;
                            Debug.Log("Deer prefab selected for spawning");
                        }
                        break;
                    case SpawnMode.Wolf:
                        if (wolfPrefab != null)
                        {
                            prefabToSpawn = wolfPrefab;
                            Debug.Log("Wolf prefab selected for spawning");
                        }
                        break;
                }

                // Sample NavMesh position
                NavMeshHit navMeshHit;
                if (NavMesh.SamplePosition(hitInfo.point, out navMeshHit, 10f, NavMesh.AllAreas))
                {
                    // Instantiate the selected prefab
                    if (prefabToSpawn != null)
                    {
                        GameObject spawnedAnimal = Instantiate(prefabToSpawn, navMeshHit.position, Quaternion.identity);
                        
                        if (spawnedAnimal == null)
                        {
                            Debug.LogError($"Failed to instantiate {currentSpawnMode} animal!");
                        }
                        else
                        {
                            // Add to tracked animals
                            spawnedAnimals.Add(spawnedAnimal);
                            
                            Debug.Log($"Spawned {currentSpawnMode} at {navMeshHit.position}");
                        }
                    }

                    // Reset spawn mode
                    currentSpawnMode = SpawnMode.None;
                }
                else
                {
                    Debug.LogWarning("Could not find a valid NavMesh position at the clicked location.");
                }
                // Add this right after the NavMesh.SamplePosition() check
                if (!NavMesh.SamplePosition(hitInfo.point, out navMeshHit, 10f, NavMesh.AllAreas))
                {
                    Debug.LogError($"NavMesh sampling failed at point {hitInfo.point}");
                    Debug.LogError($"Raycast hit normal: {hitInfo.normal}");
                    Debug.LogError($"Raycast hit collider: {hitInfo.collider.name}");
                    return; // Exit the method if no valid NavMesh position is found
                }
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
