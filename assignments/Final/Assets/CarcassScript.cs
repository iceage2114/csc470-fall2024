using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarcassScript : MonoBehaviour
{
    [Header("Carcass Settings")]
    public float lifespanTime = 300f;  // 5 minutes before despawning
    public float nutritionalValue = 50f;  // Total nutrition available
    public float maxNutritionPerWolf = 25f;  // Max nutrition per wolf
    
    private float spawnTime;
    private int currentEaters = 0;
    private const int MAX_EATERS = 4;

    private void Start()
    {
        spawnTime = Time.time;
    }

    private void Update()
    {
        // Despawn carcass after lifespan
        if (Time.time - spawnTime >= lifespanTime)
        {
            Destroy(gameObject);
        }
    }

    public bool CanEat()
    {
        return currentEaters < MAX_EATERS && nutritionalValue > 0;
    }

    public float Eat(float eatingRate)
    {
        // Limit nutrition extraction
        float nutritionExtracted = Mathf.Min(eatingRate, nutritionalValue, maxNutritionPerWolf);
        nutritionalValue -= nutritionExtracted;

        // If completely consumed, destroy carcass
        if (nutritionalValue <= 0)
        {
            Destroy(gameObject);
        }

        return nutritionExtracted;
    }

    public void StartEating()
    {
        currentEaters++;
    }

    public void StopEating()
    {
        currentEaters--;
    }
}