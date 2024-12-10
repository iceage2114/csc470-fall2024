using UnityEngine;
using UnityEngine.AI;

public class DeerScript : MonoBehaviour
{
    [Header("Deer Survival Settings")]
    public float maxHunger = 100f;     // Maximum hunger level
    public float currentHunger;        // Current hunger level
    public float normalHungerDecreaseRate = 0.5f; // Normal hunger decrease rate
    public float escapingHungerDecreaseRate = 1.5f; // Faster hunger decrease while escaping
    public float timeBetweenMeals = 40f;   // Time between needed meals
    public float minEatingDuration = 5f;   // Minimum eating time
    public float maxEatingDuration = 10f;  // Maximum eating time

    [Header("Movement Settings")]
    public float wanderRadius = 700f;   // Area deer can wander
    public float minWaitTime = 3f;     // Minimum wait time between movements
    public float maxWaitTime = 10f;    // Maximum wait time between movements
    public float minPauseDuration = 1f;// Minimum pause duration
    public float maxPauseDuration = 4f;// Maximum pause duration

    [Header("Escape Settings")]
    public float wolfDetectionRadius = 30f; // Radius to detect wolves
    public float escapeSpeed = 5f;     // Speed when running from wolves
    public float escapeTime = 100f;      // Duration of escape attempt
    private float lastEscapeTime;  // Track when last escape was initiated
    public float escapeCooldown = 1f;

    private NavMeshAgent agent;
    private MeadowScript currentMeadow;
    private float lastMealTime;
    private float nextWanderTime;
    private float eatingStartTime;
    private float randomEatingDuration;
    private float pauseStartTime;
    private float randomPauseDuration;
    private float escapeStartTime;

    [Header("Reproduction Settings")]
    public float reproductionRadius = 20f;  // How close deer must be to reproduce
    public float reproductionCooldown = 100f;  // 5 minutes between reproductions
    public GameObject deerPrefab;  // Drag the deer prefab in the inspector
    private float lastReproductionTime;
    public GameObject carcassPrefab;
    
    private enum DeerState
    {
        Wandering,
        SeekingMeadow,
        Eating,
        Dying,
        Paused,
        Escaping
    }
    private DeerState currentState = DeerState.Wandering;
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        if (agent == null)
        {
            return;
        }

        currentHunger = maxHunger;
        lastMealTime = Time.time;

        // Immediately set a destination to test movement
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
    private void Update()
    {
        // Determine hunger decrease rate based on current state
        float hungerDecreaseRate = (currentState == DeerState.Escaping) 
            ? escapingHungerDecreaseRate 
            : normalHungerDecreaseRate;

        // Decrease hunger over time
        currentHunger -= hungerDecreaseRate * Time.deltaTime;

        // Check survival state
        if (currentHunger <= 0)
        {
            Die();
            return;
        }

        // Check for nearby wolves
        WolfScript nearestWolf = FindNearestWolf();
        if (nearestWolf != null)
        {
            float wolfDistance = Vector3.Distance(transform.position, nearestWolf.transform.position);
            
            if (Time.time - lastEscapeTime > escapeTime + escapeCooldown)
            {
                StartEscaping(nearestWolf);
                return;
            }
        }

        // State machine
        switch (currentState)
        {
            case DeerState.Wandering:
                HandleWandering();
                break;
            case DeerState.SeekingMeadow:
                FindMeadow();
                break;
            case DeerState.Eating:
                HandleEating();
                break;
            case DeerState.Paused:
                HandlePause();
                break;
            case DeerState.Escaping:
                HandleEscaping();
                break;
        }

        CheckReproduction();
    }

    private void StartEscaping(WolfScript wolf)
    {
        
        currentState = DeerState.Escaping;
        lastEscapeTime = Time.time;  // Record when escape started
        
        // Calculate escape direction (directly away from wolf)
        Vector3 escapeDirection = transform.position - wolf.transform.position;
        escapeDirection.Normalize();

        // Find an escape point far from the wolf
        Vector3 escapePoint = transform.position + escapeDirection * wanderRadius;


        NavMeshHit hit;
        if (NavMesh.SamplePosition(escapePoint, out hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.speed = escapeSpeed;
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }
    }

    private void HandleEscaping()
    {
        
        // Escape for a set duration
        if (Time.time - lastEscapeTime >= escapeTime)
        {
            
            // Reset to normal speed and wandering
            agent.speed = agent.GetComponent<NavMeshAgent>().speed;
            currentState = DeerState.Wandering;
            return;
        }

        // If no longer in immediate danger, return to wandering
        if (!IsWolfNearby())
        {
            agent.speed = agent.GetComponent<NavMeshAgent>().speed;
            currentState = DeerState.Wandering;
        }
    }    
    private WolfScript FindNearestWolf()
    {
        WolfScript[] wolves = FindObjectsOfType<WolfScript>();
        WolfScript nearestWolf = null;
        float closestDistance = wolfDetectionRadius;

        foreach (WolfScript wolf in wolves)
        {
            float distance = Vector3.Distance(transform.position, wolf.transform.position);
            if (distance < closestDistance)
            {
                nearestWolf = wolf;
                closestDistance = distance;
            }
        }

        return nearestWolf;
    }

    private bool IsWolfNearby()
    {
        WolfScript[] wolves = FindObjectsOfType<WolfScript>();

        foreach (WolfScript wolf in wolves)
        {
            float distance = Vector3.Distance(transform.position, wolf.transform.position);
            if (distance <= wolfDetectionRadius)
            {
                return true;
            }
        }

        return false;
    }

    private void InitialWander()
    {
        if (agent != null)
        {
            // Force initial wander
            currentState = DeerState.Wandering;
            nextWanderTime = 0f;  // Ensure immediate wandering
        }
    }

    private void HandleWandering()
    {
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
            currentState = DeerState.SeekingMeadow;
        }
    }

    private void StartPause()
    {
        currentState = DeerState.Paused;
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
            currentState = DeerState.Wandering;
            
            // Set next wander/pause time
            nextWanderTime = Time.time + Random.Range(minWaitTime, maxWaitTime);
        }
    }

    private void FindMeadow()
    {
        // Re-enable NavMesh agent if it was disabled
        if (!agent.enabled)
        {
            agent.enabled = true;
        }

        // Find nearest meadow
        MeadowScript[] meadows = FindObjectsOfType<MeadowScript>();
        MeadowScript nearestMeadow = null;
        float closestDistance = Mathf.Infinity;

        foreach (MeadowScript meadow in meadows)
        {
            float distance = Vector3.Distance(transform.position, meadow.transform.position);
            if (distance < closestDistance && meadow.GetNutritionPercentage() > 0)
            {
                nearestMeadow = meadow;
                closestDistance = distance;
            }
        }

        if (nearestMeadow != null)
        {
            agent.SetDestination(nearestMeadow.transform.position);
            currentMeadow = nearestMeadow;
            currentState = DeerState.Eating;
        }
    }
    private void HandleEating()
    {
        // Check if reached meadow
        if (Vector3.Distance(transform.position, currentMeadow.transform.position) <= 2f)
        {
            // Stop moving when first reaching the meadow
            if (agent.isStopped == false)
            {
                agent.isStopped = true;
                // Set a random eating duration between min and max
                randomEatingDuration = Random.Range(minEatingDuration, maxEatingDuration);
                eatingStartTime = Time.time;
            }

            // Consume meadow nutrition
            float nutritionEaten = currentMeadow.ConsumeNutrition(1f * Time.deltaTime);
            currentHunger = Mathf.Min(currentHunger + nutritionEaten, maxHunger);

            // Check if meal is complete
            if (Time.time - eatingStartTime >= randomEatingDuration)
            {
                agent.isStopped = false;
                lastMealTime = Time.time;
                currentState = DeerState.Wandering;
            }
        }
    }
    private void Die()
    {
        currentState = DeerState.Dying;
        Debug.Log("Deer has died!");

        // Create carcass instead of just destroying
        GameObject carcassPrefab = Resources.Load<GameObject>("CarcassPrefab");
        if (carcassPrefab != null)
        {
            Instantiate(carcassPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private void CheckReproduction()
    {
        // Only attempt reproduction if enough time has passed
        if (Time.time - lastReproductionTime < reproductionCooldown)
            return;

        // Find nearby deer for potential reproduction
        DeerScript[] nearbyDeers = FindObjectsOfType<DeerScript>();
        foreach (DeerScript otherDeer in nearbyDeers)
        {
            // Skip self and check reproduction readiness
            if (otherDeer == this)
                continue;

            float distance = Vector3.Distance(transform.position, otherDeer.transform.position);
            
            // Check if close enough and other deer is also ready to reproduce
            if (distance <= reproductionRadius && 
                Time.time - otherDeer.lastReproductionTime >= reproductionCooldown)
            {
                StartReproduction(otherDeer);
                break;
            }
        }
    }

    private void StartReproduction(DeerScript partnerDeer)
    {
        // Set reproduction time for both deer
        lastReproductionTime = Time.time;
        partnerDeer.lastReproductionTime = Time.time;

        // Move close to partner
        agent.SetDestination(partnerDeer.transform.position);

        // Use Invoke to delay offspring creation
        Invoke("CreateOffspring", 20f);
    }

    private void CreateOffspring()
    {
        if (deerPrefab != null)
        {
            // Instantiate new deer between current deer and partner
            Vector3 spawnPosition = (transform.position + transform.position) / 2;
            Instantiate(deerPrefab, spawnPosition, Quaternion.identity);
        }
    }
    }