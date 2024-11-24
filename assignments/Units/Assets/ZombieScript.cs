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
    public NavMeshAgent nma;

    private List<GameObject> zombies = new List<GameObject>();
    private Transform playerTransform;
    private bool isRoundComplete = false;

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
        
        SpawnZombies();
    }

    private class ZombieScript : MonoBehaviour
    {
        private const int MODE_EMERGING = 1;
        private const int MODE_CHASING = 2;

        private ZombieNorthScript manager;
        private NavMeshAgent navMeshAgent;
        private int currentMode = MODE_EMERGING;
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
            if (currentMode == MODE_EMERGING)
            {
                HandleEmergence();
            }
            else if (currentMode == MODE_CHASING)
            {
                HandleChasing();
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
            currentMode = MODE_CHASING;
            Debug.Log("Zombie starting chase mode");
            HandleChasing();
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
        RemoveDestroyedZombies();
        UpdateZombies();
        CheckRoundComplete();
    }

    private void RemoveDestroyedZombies()
    {
        List<GameObject> activeZombies = new List<GameObject>();
        foreach (GameObject zombie in zombies)
        {
            if (zombie != null)
            {
                activeZombies.Add(zombie);
            }
        }
        zombies = activeZombies;
    }

    void SpawnZombies()
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
                ZombieScript zombieScript = zombie.GetComponent<ZombieScript>();
                if (zombieScript != null)
                {
                    zombieScript.UpdateZombie();
                }
            }
        }
    }

    void CheckRoundComplete()
    {
        if (!isRoundComplete && zombies.Count == 0)
        {
            Debug.Log("Round Complete!");
            isRoundComplete = true;
            enabled = false;
        }
    }

    public bool IsComplete()
    {
        return isRoundComplete;
    }
}