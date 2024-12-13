using UnityEngine;
using UnityEngine.AI;

public class WolfScript : MonoBehaviour
{
    [Header("Wolf Survival Settings")]
    public float maxHunger;     // Maximum hunger level
    public float currentHunger;        // Current hunger level
    public float hungerDecreaseRate; // How quickly wolf gets hungry
    public float timeBetweenMeals;   // Time between needed meals

    [Header("Movement Settings")]
    public float wanderRadius;   // Area wolf can wander
    public float minWaitTime;     // Minimum wait time between movements
    public float maxWaitTime;    // Maximum wait time between movements
    public float minPauseDuration;// Minimum pause duration
    public float maxPauseDuration;// Maximum pause duration

    [Header("Hunting Settings")]
    public float huntRadius;     // Detection radius for deer
    public float huntDuration;   // Maximum time spent hunting
    public float eatingDuration; // Time spent eating a caught deer
    public float catchRadius;     // Distance to catch a deer

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

        // When deer is caught
        float distanceToDeer = Vector3.Distance(transform.position, targetDeer.transform.position);
        if (distanceToDeer <= catchRadius)
        {
            // Ensure the targetDeer executes its Die method
            targetDeer.Die();

            // Start eating
            agent.isStopped = true;
            currentState = WolfState.Eating;
            eatingStartTime = Time.time;
        }
    }


    private void HandleEating()
    {
        // Check if carcass still exists
        if (currentCarcass == null)
        {
            agent.isStopped = false;
            currentState = WolfState.Wandering;
            isEatingCarcass = false;
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
                // Check if wolf can eat this carcass
                if (currentCarcass != null && currentCarcass.CanEat(this))
                {
                    isEatingCarcass = true;
                }
                else
                {
                    // Cannot eat, return to wandering
                    agent.isStopped = false;
                    currentState = WolfState.Wandering;
                    currentCarcass = null;
                    isEatingCarcass = false;
                    return;
                }
            }

            // Consume carcass nutrition
            float nutritionEaten = currentCarcass.Eat(this, carcassEatingRate * Time.deltaTime);
            currentHunger = Mathf.Min(currentHunger + nutritionEaten, maxHunger);

            // Check if carcass is consumed or destroyed
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
            if (distance < closestDistance && carcass.CanEat(this)) // Pass 'this' to represent the current wolf
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
        // Prevent reproduction during hunting
        if (currentState == WolfState.Hunting)
        {
            return;
        }

        // Existing reproduction check remains the same
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
            
            // Additional check to ensure partner is not hunting
            if (distance <= reproductionRadius && 
                Time.time - otherWolf.lastReproductionTime >= reproductionCooldown &&
                otherWolf.currentState != WolfState.Hunting)
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
            GameObject newWolf = Instantiate(wolfPrefab, spawnPosition, Quaternion.identity);

            // Notify GameManager to track the new wolf
            if (GameManagerScript.instance != null)
            {
                GameManagerScript.instance.TrackNewAnimal(newWolf);
            }
        }
    }


        // Add a method to stop eating carcass if interrupted
    private void OnDisable()
    {
        if (isEatingCarcass && currentCarcass != null)
        {
            currentCarcass.StopEating(this);
            isEatingCarcass = false;
        }
    }
}