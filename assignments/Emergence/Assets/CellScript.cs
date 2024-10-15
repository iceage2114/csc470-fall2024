using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CellScript : MonoBehaviour
{
    public Renderer cubeRenderer;
    public bool alive = false;
    public static readonly string[] speciesList = {"pink", "water", "fire", "honey", "goo"};
    public int xIndex = -1;
    public int yIndex = -1;
    public List<string> currentSpecies = new List<string>();
    public Color emptyColor;
    GameManager gameManager;

    private static readonly Dictionary<string, Color> speciesColors = new Dictionary<string, Color>()
    {
        { "pink", Color.green },
        { "water", Color.blue },
        { "fire", Color.red },
        { "honey", Color.yellow },
        { "goo", Color.black}
    };

    void Start()
    {
        SetColor();

        GameObject gmObj = GameObject.Find("GameManagerObject");
        gameManager = gmObj.GetComponent<GameManager>();
    }

    void OnMouseDown() {
        if(alive) {
            alive = !alive;
            currentSpecies.Clear();
        }
        
        SetColor();
        gameManager.CountNeighbors(xIndex, yIndex);
    }

    public void AssignSpecies(string assignedSpecies) {
        if (speciesColors.ContainsKey(assignedSpecies)) {
            currentSpecies.Clear();
            currentSpecies.Add(assignedSpecies);
        }
    }

    public void SetSpecies(List<string> species) {
        currentSpecies = species.Distinct().ToList();
    }

    public Color GetCurrentColor() {
        if (currentSpecies.Count == 0) return emptyColor;
        if (currentSpecies.Contains("goo")) return speciesColors["goo"];
        
        // Blend colors if multiple species
        if (currentSpecies.Count > 1) {
            Color blendedColor = Color.black;
            foreach (string species in currentSpecies) {
                blendedColor += speciesColors[species];
            }
            blendedColor /= currentSpecies.Count;
            return blendedColor;
        }
        
        return speciesColors[currentSpecies[0]];
    }

    public void SetColor() {
        if(alive) {
            cubeRenderer.material.color = GetCurrentColor();
        } else {
            cubeRenderer.material.color = emptyColor;
        }
    }
}