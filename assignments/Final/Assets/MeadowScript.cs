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

    public float ConsumeNutrition(float amount)
    {
        float nutritionConsumed = Mathf.Min(amount, currentNutrition);
        currentNutrition -= nutritionConsumed;

        return nutritionConsumed;
    }

    public float GetNutritionPercentage()
    {
        return (currentNutrition / maxNutrition) * 100f;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, meadowRadius);
    }
}