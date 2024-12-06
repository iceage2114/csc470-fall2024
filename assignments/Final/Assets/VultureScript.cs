using UnityEngine;
using UnityEngine.AI;

public class VultureScript : MonoBehaviour
{
    [Header("Vulture Survival Settings")]
    public float maxHunger = 100f;     // Maximum hunger level
    public float currentHunger;        // Current hunger level
    public float normalHungerDecreaseRate = 5f; // Normal hunger decrease rate
    public float flyingHungerDecreaseRate = 10f; // Faster hunger decrease while flying

    [Header("Circling Settings")]
    public float circlingHeight = 50f; // Height at which vulture circles
    public float circlingRadius = 20f; // Radius of circling movement
    public float circlingSpeed = 10f;  // Speed of circling

    [Header("Eating Settings")]
    public float eatingDuration = 10f; // Time taken to eat carrion
    public float nutritionPerEat = 50f; // Amount of hunger restored per eat
    public float carrionDetectionRadius = 30f; // Radius to detect carrion
    public float descendSpeed = 15f;   // Speed of descending to carrion
    public float minimumHungerToEat = 20f; // Minimum hunger level to start seeking food

    private enum VultureState
    {
        Circling,
        Descending,
        Eating,
        Ascending
    }
    private VultureState currentState = VultureState.Circling;

    private float circlingAngle = 0f;
    private float eatingStartTime;
    private GameObject currentCarrion;
    private Vector3 originalPosition;

    private void Start()
    {
        currentHunger = maxHunger;
        originalPosition = transform.position;
    }

    private void Update()
    {
        // Determine hunger decrease rate based on current state
        float hungerDecreaseRate = (currentState == VultureState.Circling) 
            ? flyingHungerDecreaseRate 
            : normalHungerDecreaseRate;

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
            case VultureState.Circling:
                HandleCircling();
                break;
            case VultureState.Descending:
                HandleDescending();
                break;
            case VultureState.Eating:
                HandleEating();
                break;
            case VultureState.Ascending:
                HandleAscending();
                break;
        }
    }

    private void HandleCircling()
    {
        // Circle at a fixed height
        circlingAngle += circlingSpeed * Time.deltaTime;
        Vector3 circlingPosition = originalPosition + new Vector3(
            Mathf.Cos(circlingAngle) * circlingRadius, 
            circlingHeight, 
            Mathf.Sin(circlingAngle) * circlingRadius
        );
        transform.position = circlingPosition;
        transform.LookAt(originalPosition);

        // Check if hungry enough to seek carrion
        if (currentHunger <= minimumHungerToEat)
        {
            currentCarrion = FindNearestCarrion();
            if (currentCarrion != null)
            {
                currentState = VultureState.Descending;
            }
        }
    }

    private void HandleDescending()
    {
        if (currentCarrion == null)
        {
            // If carrion disappeared, return to circling
            currentState = VultureState.Circling;
            return;
        }

        // Descend towards carrion
        Vector3 descendDirection = (currentCarrion.transform.position - transform.position).normalized;
        transform.position += descendDirection * descendSpeed * Time.deltaTime;
        transform.LookAt(currentCarrion.transform);

        // Check if reached carrion
        if (Vector3.Distance(transform.position, currentCarrion.transform.position) <= 2f)
        {
            currentState = VultureState.Eating;
            eatingStartTime = Time.time;
        }
    }

    private void HandleEating()
    {
        // Check eating duration
        if (Time.time - eatingStartTime >= eatingDuration)
        {
            // Restore hunger
            currentHunger = Mathf.Min(currentHunger + nutritionPerEat, maxHunger);
            
            // Destroy carrion
            Destroy(currentCarrion);
            currentCarrion = null;

            // Begin ascending
            currentState = VultureState.Ascending;
        }
    }

    private void HandleAscending()
    {
        // Return to original circling height
        Vector3 ascentDirection = (new Vector3(originalPosition.x, circlingHeight, originalPosition.z) - transform.position).normalized;
        transform.position += ascentDirection * descendSpeed * Time.deltaTime;

        // Check if back at circling height
        if (Mathf.Abs(transform.position.y - circlingHeight) <= 1f)
        {
            currentState = VultureState.Circling;
        }
    }

    private GameObject FindNearestCarrion()
    {
        // Find all objects tagged as "Carrion"
        GameObject[] carrionObjects = GameObject.FindGameObjectsWithTag("Carrion");
        GameObject nearestCarrion = null;
        float closestDistance = carrionDetectionRadius;

        foreach (GameObject carrion in carrionObjects)
        {
            float distance = Vector3.Distance(transform.position, carrion.transform.position);
            if (distance < closestDistance)
            {
                nearestCarrion = carrion;
                closestDistance = distance;
            }
        }

        return nearestCarrion;
    }

    private void Die()
    {
        Debug.Log("Vulture has died of starvation!");
        Destroy(gameObject);
    }
}