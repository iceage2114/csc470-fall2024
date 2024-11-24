using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class ZombieNorthScript : MonoBehaviour
{
    public GameObject zombiePrefab;
    public Transform spawnPoint;
    public float spawnHeight = -2f;    
    public float surfaceHeight = 0f;   
    public float emergenceSpeed = 2f;   
    public int currentRound = 0;
    public int maxRounds = 3;
    public float timeBetweenRounds = 5f;
    public NavMeshAgent nma;

    private List<GameObject> zombies = new List<GameObject>();
    private bool waitingForNextRound = false;
    private float roundTimer = 0f;
    private Transform playerTransform;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("No 'Player' tag found");
        }
        
        waitingForNextRound = true;
        roundTimer = timeBetweenRounds;
    }

    private class ZombieScript : MonoBehaviour
    {
        private enum ZombieMode
        {
            Emerging,
            Chasing
        }

        private ZombieNorthScript manager;
        private NavMeshAgent navMeshAgent;
        private ZombieMode currentMode = ZombieMode.Emerging;
        private float emergenceDelay;

        public void Initialize(ZombieNorthScript manager)
        {
            this.manager = manager;
            this.navMeshAgent = GetComponent<NavMeshAgent>();
            this.emergenceDelay = Random.Range(0f, 1f);
            
            navMeshAgent.speed = 2f;
            navMeshAgent.angularSpeed = 120f;
            navMeshAgent.stoppingDistance = 1f;
            navMeshAgent.enabled = false;
        }

        public void UpdateZombie()
        {
            switch (currentMode)
            {
                case ZombieMode.Emerging:
                    HandleEmergence();
                    break;
                case ZombieMode.Chasing:
                    HandleChasing();
                    break;
            }
        }

        private void HandleEmergence()
        {
            if (emergenceDelay > 0)
            {
                emergenceDelay -= Time.deltaTime;
                return;
            }

            if (transform.position.y < manager.surfaceHeight)
            {
                Vector3 targetPos = transform.position;
                targetPos.y = manager.surfaceHeight;
                
                transform.position = Vector3.Lerp(
                    transform.position,
                    targetPos,
                    Time.deltaTime * manager.emergenceSpeed
                );

                if (Mathf.Abs(transform.position.y - manager.surfaceHeight) < 0.01f)
                {
                    navMeshAgent.enabled = true;
                    StartChasing();
                }
            }
        }

        private void StartChasing()
        {
            currentMode = ZombieMode.Chasing;
            Debug.Log("Zombie starting chase mode");
            HandleChasing(); // Start chasing immediately
        }

        private void HandleChasing()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                navMeshAgent.SetDestination(player.transform.position);
            }
        }
    }

    void Update()
    {
        zombies.RemoveAll(z => z == null);
        ManageRoundProgression();
        UpdateZombies();
    }

    void ManageRoundProgression()
    {
        if (waitingForNextRound)
        {
            roundTimer -= Time.deltaTime;
            
            if (roundTimer <= 0f && currentRound < maxRounds)
            {
                currentRound++;
                Debug.Log($"Round {currentRound} starting");
                SpawnZombiesForRound();
                waitingForNextRound = false;
            }
        }
        else if (zombies.Count == 0 && currentRound < maxRounds)
        {
            Debug.Log("Round Complete");
            waitingForNextRound = true;
            roundTimer = timeBetweenRounds;
        }
        else if (zombies.Count == 0 && currentRound >= maxRounds)
        {
            Debug.Log("All rounds complete");
            enabled = false;
        }
    }

    void SpawnZombiesForRound()
    {
        int zombieCount = Random.Range(3, 7);
        
        for (int i = 0; i < zombieCount; i++)
        {
            Vector3 spawnPos = spawnPoint.position;
            spawnPos.x += Random.Range(-5f, 5f);
            spawnPos.z += Random.Range(-5f, 5f);
            spawnPos.y = spawnHeight;
            
            GameObject zombie = Instantiate(zombiePrefab, spawnPos, Quaternion.identity);
            ZombieScript zombieScript = zombie.AddComponent<ZombieScript>();
            zombieScript.Initialize(this);
            
            zombies.Add(zombie);
        }
    }

    void UpdateZombies()
    {
        foreach (GameObject zombie in zombies)
        {
            if (zombie != null)
            {
                zombie.GetComponent<ZombieScript>()?.UpdateZombie();
            }
        }
    }

    public void ForceStartNextRound()
    {
        if (!waitingForNextRound && currentRound < maxRounds)
        {
            waitingForNextRound = false;
            roundTimer = 0f;
        }
    }

    public bool AreAllRoundsComplete()
    {
        return currentRound >= maxRounds && zombies.Count == 0;
    }
}