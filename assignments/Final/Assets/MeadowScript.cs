using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MeadowScript : MonoBehaviour
{
    [Header("Meadow Nutrition Settings")]
    public float maxNutrition = 100f;  // Total nutrition available in the meadow
    public float currentNutrition;     // Current nutrition level
    public float rechargeRate = 0.1f;    // How quickly meadow recovers nutrition
    public float depletionRate = 1f;  // How quickly nutrition is consumed

    [Header("Meadow Characteristics")]
    public float meadowRadius = 10f;   // Size of the meadow area

    private void Start()
    {
        currentNutrition = maxNutrition;
    }

    private void Update()
    {
        // Slowly recharge meadow nutrition
        currentNutrition = Mathf.Min(
            currentNutrition + (rechargeRate * Time.deltaTime), 
            maxNutrition
        );
    }

    // Method for deer to consume nutrition
    public float ConsumeNutrition(float amount)
    {
        // Calculate actual nutrition consumed
        float nutritionConsumed = Mathf.Min(amount, currentNutrition);
        currentNutrition -= nutritionConsumed;

        return nutritionConsumed;
    }

    // Get current nutrition percentage
    public float GetNutritionPercentage()
    {
        return (currentNutrition / maxNutrition) * 100f;
    }

    // Visualization in scene view (optional)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, meadowRadius);
    }
}