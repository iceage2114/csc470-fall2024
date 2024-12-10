using UnityEngine;
using UnityEngine.AI;

public class WolfScript : MonoBehaviour
{
    [Header("Wolf Survival Settings")]
    public float maxHunger = 100f;     // Maximum hunger level
    public float currentHunger;        // Current hunger level
    public float hungerDecreaseRate = 15f; // How quickly wolf gets hungry
    public float timeBetweenMeals = 90f;   // Time between needed meals

    [Header("Movement Settings")]
    public float wanderRadius = 30f;   // Area wolf can wander
    public float minWaitTime = 3f;     // Minimum wait time between movements
    public float maxWaitTime = 10f;    // Maximum wait time between movements
    public float minPauseDuration = 1f;// Minimum pause duration
    public float maxPauseDuration = 4f;// Maximum pause duration

    [Header("Hunting Settings")]
    public float huntRadius = 50f;     // Detection radius for deer
    public float huntDuration = 60f;   // Maximum time spent hunting
    public float eatingDuration = 20f; // Time spent eating a caught deer
    public float catchRadius = 2f;     // Distance to catch a deer

    private NavMeshAgent agent;
    private DeerScript targetDeer;
    private float lastMealTime;
    private float nextWanderTime;
    private float huntStartTime;
    private float eatingStartTime;
    private float pauseStartTime;
    private float randomPauseDuration;

    [Header("Reproduction Settings")]
    public float reproductionRadius = 20f;  // How close wolves must be to reproduce
    public float reproductionCooldown = 100f;  // 5 minutes between reproductions
    public GameObject wolfPrefab;  // Drag the wolf prefab in the inspector
    private float lastReproductionTime;

    [Header("Carcass Eating Settings")]
    public float carcassEatingRadius = 3f;  // Radius to detect and eat carcasses
    public float carcassEatingRate = 10f;   // How quickly wolf can consume carcass

    private CarcassScript currentCarcass;
    private bool isEatingCarcass = false;

    private enum WolfState
    {
        Wandering,
        Hunting,
        Eating,
        Paused,
        Dying
    }
    private WolfState currentState = WolfState.Wandering;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        currentHunger = maxHunger;
        lastMealTime = Time.time;
    }

    private void Update()
    {
        // Decrease hunger over time
        currentHunger -= hungerDecreaseRate * Time.deltaTime;

        // Check survival state
        if (currentHunger <= 0)
        {
            Die();
            return;
        }

        // State machine
        switch (currentState)
        {
            case WolfState.Wandering:
                HandleWandering();
                break;
            case WolfState.Hunting:
                HandleHunting();
                break;
            case WolfState.Eating:
                HandleEating();
                break;
            case WolfState.Paused:
                HandlePause();
                break;
        }

        CheckReproduction();

        if (currentState == WolfState.Wandering && currentHunger < maxHunger)
        {
            CheckForCarcasses();
        }
    }

    private void HandleWandering()
    {
        // Check for nearby deer to hunt
        DeerScript nearestDeer = FindNearestDeer();
        if (nearestDeer != null)
        {
            targetDeer = nearestDeer;
            currentState = WolfState.Hunting;
            huntStartTime = Time.time;
            return;
        }

        // Check if it's time to wander or pause
        if (Time.time >= nextWanderTime)
        {
            // Random chance to pause instead of moving
            if (Random.value < 0.4f)
            {
                StartPause();
                return;
            }

            // Wander to a new location
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += transform.position;

            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, 1);

            agent.SetDestination(hit.position);
            
            // Set next wander/pause time with more variability
            nextWanderTime = Time.time + Random.Range(minWaitTime, maxWaitTime);
        }

        // Check if hungry and needs to find food
        if (Time.time - lastMealTime >= timeBetweenMeals)
        {
            // Try to find prey if no nearby deer found in first pass
            DeerScript findPrey = FindNearestDeer();
            if (findPrey != null)
            {
                targetDeer = findPrey;
                currentState = WolfState.Hunting;
                huntStartTime = Time.time;
            }
        }
    }

    private void HandleHunting()
    {
        // Check if hunt duration exceeded
        if (Time.time - huntStartTime >= huntDuration)
        {
            currentState = WolfState.Wandering;
            targetDeer = null;
            return;
        }

        // Check if target deer still exists
        if (targetDeer == null)
        {
            currentState = WolfState.Wandering;
            return;
        }

        // Set destination to deer
        agent.SetDestination(targetDeer.transform.position);

        // When deer is caught, create a carcass instead of destroying
        float distanceToDeer = Vector3.Distance(transform.position, targetDeer.transform.position);
        if (distanceToDeer <= catchRadius)
        {
            // Create carcass where deer was
            GameObject carcassPrefab = Resources.Load<GameObject>("CarcassPrefab");
            if (carcassPrefab != null)
            {
                Instantiate(carcassPrefab, targetDeer.transform.position, Quaternion.identity);
            }
            
            // Destroy the deer
            Destroy(targetDeer.gameObject);
            
            // Start eating
            agent.isStopped = true;
            currentState = WolfState.Eating;
            eatingStartTime = Time.time;
        }
    }

    private void HandleEating()
    {
        // Check if eating a regular meal
        if (currentCarcass == null)
        {
            // Existing meal eating logic
            if (Time.time - eatingStartTime >= eatingDuration)
            {
                currentHunger = Mathf.Min(currentHunger + 50f, maxHunger);
                agent.isStopped = false;
                lastMealTime = Time.time;
                currentState = WolfState.Wandering;
            }
            return;
        }

        // Carcass eating logic
        float distanceToCarcass = Vector3.Distance(transform.position, currentCarcass.transform.position);
        
        if (distanceToCarcass <= carcassEatingRadius)
        {
            // Stop moving and start eating
            agent.isStopped = true;
            
            // Start eating carcass
            if (!isEatingCarcass)
            {
                currentCarcass.StartEating();
                isEatingCarcass = true;
            }

            // Consume carcass nutrition
            float nutritionEaten = currentCarcass.Eat(carcassEatingRate * Time.deltaTime);
            currentHunger = Mathf.Min(currentHunger + nutritionEaten, maxHunger);

            // Check if carcass is consumed
            if (currentCarcass == null)
            {
                // Reset to wandering
                agent.isStopped = false;
                currentState = WolfState.Wandering;
                isEatingCarcass = false;
            }
        }
    }

    private void CheckForCarcasses()
    {
        CarcassScript[] carcasses = FindObjectsOfType<CarcassScript>();
        CarcassScript nearestCarcass = null;
        float closestDistance = carcassEatingRadius;

        foreach (CarcassScript carcass in carcasses)
        {
            float distance = Vector3.Distance(transform.position, carcass.transform.position);
            if (distance < closestDistance && carcass.CanEat())
            {
                nearestCarcass = carcass;
                closestDistance = distance;
            }
        }

        if (nearestCarcass != null)
        {
            currentCarcass = nearestCarcass;
            currentState = WolfState.Eating;
            agent.SetDestination(currentCarcass.transform.position);
        }
    }

    private void StartPause()
    {
        currentState = WolfState.Paused;
        agent.isStopped = true;
        
        // Set a random pause duration
        randomPauseDuration = Random.Range(minPauseDuration, maxPauseDuration);
        pauseStartTime = Time.time;
    }

    private void HandlePause()
    {
        // Check if pause duration is complete
        if (Time.time - pauseStartTime >= randomPauseDuration)
        {
            agent.isStopped = false;
            currentState = WolfState.Wandering;
            
            // Set next wander/pause time
            nextWanderTime = Time.time + Random.Range(minWaitTime, maxWaitTime);
        }
    }

    private DeerScript FindNearestDeer()
    {
        DeerScript[] deers = FindObjectsOfType<DeerScript>();
        DeerScript nearestDeer = null;
        float closestDistance = huntRadius;

        foreach (DeerScript deer in deers)
        {
            float distance = Vector3.Distance(transform.position, deer.transform.position);
            if (distance < closestDistance)
            {
                nearestDeer = deer;
                closestDistance = distance;
            }
        }

        return nearestDeer;
    }

    private void Die()
    {
        currentState = WolfState.Dying;
        Debug.Log("Wolf has died of starvation!");
        Destroy(gameObject);
    }

    private void CheckReproduction()
    {
        // Only attempt reproduction if enough time has passed
        if (Time.time - lastReproductionTime < reproductionCooldown)
            return;

        // Find nearby wolves for potential reproduction
        WolfScript[] nearbyWolves = FindObjectsOfType<WolfScript>();
        foreach (WolfScript otherWolf in nearbyWolves)
        {
            // Skip self and check reproduction readiness
            if (otherWolf == this)
                continue;

            float distance = Vector3.Distance(transform.position, otherWolf.transform.position);
            
            // Check if close enough and other wolf is also ready to reproduce
            if (distance <= reproductionRadius && 
                Time.time - otherWolf.lastReproductionTime >= reproductionCooldown)
            {
                StartReproduction(otherWolf);
                break;
            }
        }
    }

    private void StartReproduction(WolfScript partnerWolf)
    {
        // Set reproduction time for both wolves
        lastReproductionTime = Time.time;
        partnerWolf.lastReproductionTime = Time.time;

        // Move close to partner
        agent.SetDestination(partnerWolf.transform.position);

        // Use Invoke to delay offspring creation
        Invoke("CreateOffspring", 20f);
    }

    private void CreateOffspring()
    {
        if (wolfPrefab != null)
        {
            // Instantiate new wolf between current wolf and partner
            Vector3 spawnPosition = (transform.position + transform.position) / 2;
            Instantiate(wolfPrefab, spawnPosition, Quaternion.identity);
        }
    }

        // Add a method to stop eating carcass if interrupted
    private void OnDisable()
    {
        if (isEatingCarcass && currentCarcass != null)
        {
            currentCarcass.StopEating();
            isEatingCarcass = false;
        }
    }
}