using UnityEngine;
using System.Collections.Generic;

public class CarcassScript : MonoBehaviour
{
    [Header("Carcass Nutrition Settings")]
    public float initialNutrition = 100f;  // Total nutrition available
    public float currentNutrition;         // Remaining nutrition
    public float nutritionDecayRate = 5f;  // How quickly nutrition decreases over time

    [Header("Carcass Lifecycle Settings")]
    public float lifespanTime = 300f;      // 5 minutes before complete decay
    public float maxEaters = 4;            // Maximum number of wolves that can eat simultaneously

    private float spawnTime;
    private List<WolfScript> currentEaters = new List<WolfScript>();

    private void Awake()
    {
        // Initialize nutrition and spawn time
        currentNutrition = initialNutrition;
        spawnTime = Time.time;
    }

    private void Update()
    {
        // Decay nutrition over time
        currentNutrition -= nutritionDecayRate * Time.deltaTime;

        // Check if carcass should be destroyed
        if (currentNutrition <= 0 || Time.time - spawnTime >= lifespanTime)
        {
            Destroy(gameObject);
        }
    }

    public bool CanEat(WolfScript wolf)
    {
        // Check if wolf can eat the carcass
        return currentNutrition > 0 && 
               currentEaters.Count < maxEaters && 
               !currentEaters.Contains(wolf);
    }

    public float Eat(WolfScript wolf, float eatingRate)
    {
        // Ensure wolf can eat
        if (!CanEat(wolf))
            return 0f;

        // Add wolf to eaters if not already eating
        if (!currentEaters.Contains(wolf))
            currentEaters.Add(wolf);

        // Calculate nutrition to extract
        float nutritionExtracted = Mathf.Min(eatingRate, currentNutrition);
        currentNutrition -= nutritionExtracted;

        // Remove wolf if no more nutrition
        if (currentNutrition <= 0)
            currentEaters.Remove(wolf);

        return nutritionExtracted;
    }

    public void StopEating(WolfScript wolf)
    {
        // Remove wolf from eaters list
        currentEaters.Remove(wolf);
    }
}