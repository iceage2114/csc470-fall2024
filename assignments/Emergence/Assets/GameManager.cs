using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public GameObject cellPrefab;
    CellScript[,] grid;
    float spacing = 1f;
    bool isAutoIterating = false; 
    float autoIterationInterval = 2f; 
    float lastIterationTime = 0f;

    public string[] activeSpeciesList = { "pink", "water", "fire", "honey", "goo" };

    private struct Zone {
        public int x, y;
        public string species;
    }

    void Start() {
        grid = new CellScript[50,50];

        Zone[] zones = new Zone[]
        {
            new Zone { x = 17, y = 34, species = null }, // 12 
            new Zone { x = 34, y = 17, species = null }, // 3 
            new Zone { x = 17, y = 0,  species = null }, // 5
            new Zone { x = 0,  y = 0,  species = null }, // 7 
            new Zone { x = 0,  y = 34, species = null }, // 10 
            new Zone { x = 17, y = 17, species = null }  // center
        };

        List<string> availableSpecies = new List<string>(activeSpeciesList);
        foreach (int i in Enumerable.Range(0, zones.Length).OrderBy(x => Random.value))
        {
            if (availableSpecies.Count > 0)
            {
                int speciesIndex = Random.Range(0, availableSpecies.Count);
                zones[i].species = availableSpecies[speciesIndex];
                availableSpecies.RemoveAt(speciesIndex);
            }
        }

        for (int x = 0; x < 50; x++)
        {
            for (int y = 0; y < 50; y++)
            {
                Vector3 pos = transform.position;
                pos.x += x * spacing;
                pos.z += y * spacing;
                GameObject cell = Instantiate(cellPrefab, pos, Quaternion.identity);
                grid[x,y] = cell.GetComponent<CellScript>();
                grid[x,y].alive = false;
                grid[x,y].xIndex = x;
                grid[x,y].yIndex = y;
                grid[x,y].AssignSpecies("");
                grid[x,y].SetColor();
            }
        }

        foreach (Zone zone in zones)
        {
            if (zone.species != null)
            {
                for (int x = zone.x; x < zone.x + 16; x++)
                {
                    for (int y = zone.y; y < zone.y + 16; y++)
                    {
                        grid[x,y].alive = (Random.value > 0.5f);
                        grid[x,y].AssignSpecies(zone.species);
                        grid[x,y].SetColor();
                    }
                }
            }
        }

        List<Zone> emptyZones = zones.Where(z => z.species == null).ToList();
        if (emptyZones.Count > 0)
        {
            Zone randomEmptyZone = emptyZones[Random.Range(0, emptyZones.Count)];
            string randomSpecies = activeSpeciesList[Random.Range(0, activeSpeciesList.Length)];
            
            for (int x = randomEmptyZone.x; x < randomEmptyZone.x + 16; x++)
            {
                for (int y = randomEmptyZone.y; y < randomEmptyZone.y + 16; y++)
                {
                    grid[x,y].alive = (Random.value > 0.5f);
                    grid[x,y].AssignSpecies(randomSpecies);
                    grid[x,y].SetColor();
                }
            }
        }
    }


    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            ToggleAutoIteration();
        }

        if (isAutoIterating && Time.time - lastIterationTime >= autoIterationInterval) {
            Simulate();
            lastIterationTime = Time.time;
        }
    }

    void Simulate() {
        bool[,] nextAlive = new bool[50,50];
        List<string>[,] nextSpecies = new List<string>[50,50];

        for(int x = 0; x < 50; x++) {
            for(int y = 0; y < 50; y++) {
                (int count, List<string> neighborSpecies) = CountNeighborsAndSpecies(x, y);
                nextSpecies[x,y] = new List<string>();

                if(grid[x,y].alive && count < 2) {//starving
                    nextAlive[x,y] = false;
                } else if(grid[x,y].alive && (count == 2 || count == 3)) {//surviving
                    nextAlive[x,y] = true;
                    nextSpecies[x,y] = new List<string>(grid[x,y].currentSpecies);
                } else if(grid[x,y].alive && count > 3) {//overcrowding
                    nextAlive[x,y] = false;
                } else if(!grid[x,y].alive && count == 3){ //reproduction
                    nextAlive[x,y] = true;
                    
                    if (neighborSpecies.Contains("goo")) {
                        nextSpecies[x,y].Add("goo");
                    } else {
                        var distinctSpecies = neighborSpecies.Distinct().ToList();
                        if (distinctSpecies.Count > 2) {
                            nextSpecies[x,y].Add("goo");
                        } else {
                            nextSpecies[x,y] = distinctSpecies;
                        }
                    }
                } else {
                    nextAlive[x,y] = grid[x,y].alive;
                    if (grid[x,y].alive) {
                        nextSpecies[x,y] = new List<string>(grid[x,y].currentSpecies);
                    }
                }
            }
        }

        for(int i = 0; i < 50; i++) {
            for(int j = 0; j < 50; j++) {
                grid[i,j].alive = nextAlive[i,j];
                grid[i,j].SetSpecies(nextSpecies[i,j]);
                grid[i,j].SetColor();
            }
        }
    }

    public (int count, List<string> species) CountNeighborsAndSpecies(int xIndex, int yIndex)
    {
        int count = 0;
        List<string> neighborSpecies = new List<string>();

        for (int x = xIndex - 1; x <= xIndex + 1; x++)
        {
            for (int y = yIndex - 1; y <= yIndex + 1; y++)
            {
                if (x >= 0 && x < 50 && y >= 0 && y < 50)
                {
                    if (!(x == xIndex && y == yIndex))
                    {
                        if (grid[x,y].alive)
                        {
                            count++;
                            neighborSpecies.AddRange(grid[x,y].currentSpecies);
                        }
                    }
                }
            }
        }

        return (count, neighborSpecies);
    }

    public int CountNeighbors(int xIndex, int yIndex) {
        return CountNeighborsAndSpecies(xIndex, yIndex).count;
    }

    void ToggleAutoIteration() {
        isAutoIterating = true;
        if (isAutoIterating){
            Debug.Log("Auto started.");
            Simulate();
            lastIterationTime = Time.time;
        }
    }
}