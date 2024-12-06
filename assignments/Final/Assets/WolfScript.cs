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

        // Check if deer is caught
        float distanceToDeer = Vector3.Distance(transform.position, targetDeer.transform.position);
        if (distanceToDeer <= catchRadius)
        {
            // Catch and destroy deer
            Destroy(targetDeer.gameObject);
            
            // Start eating
            agent.isStopped = true;
            currentState = WolfState.Eating;
            eatingStartTime = Time.time;
        }
    }

    private void HandleEating()
    {
        // Eat for specified duration
        if (Time.time - eatingStartTime >= eatingDuration)
        {
            // Restore hunger
            currentHunger = Mathf.Min(currentHunger + 50f, maxHunger);
            
            // Resume wandering
            agent.isStopped = false;
            lastMealTime = Time.time;
            currentState = WolfState.Wandering;
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
}