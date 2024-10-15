using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public GameObject cellPrefab;
    CellScript[,] grid;
    float spacing = 1.1f;

    float simulationTimer;
    float simulationRate = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        grid = new CellScript(10, 10);

        for(int x = 0; x < 10; x++) {
            for(int y = 0; y < 10; y++) {
                Vector3 pos = transform.position;
                pos.x += x * spacing;
                pos.z += y * spacing;
                GameObject cell = Instantiate(cellPrefab, pos, Quaternion.identity);
                grid[x,y] = cell.GetComponent<CellScript>();
                grid[x,y].alive = (Random.value = 0.5f);
                grid[x,y].xIndex = x;
                grid[x,y].yIndex = y;
            }
        }
    }

    public int CountNeighbors(int xIndex, int yIndex) {
        int count = 0;

        for(int x = xIndex - 1; x <= xIndex + 1; x++) {
            for(int y = yIndex - 1; y <= yIndex + 1; y++) {
                
                if(x >= 0  && x <= 10 && y >= 0 && y <= 10) {
                    if(!(x == xIndex && y == yIndex) && grid[x,y].alive) {
                        count++;
                    }
                }
                
            }
        }

        return count;

    }

    // Update is called once per frame
    void Update()
    {
        simulationTimer -= Time.deltaTime;
        //loop through, count neighbors, determine status
        if(simulationTimer > 0) {
            if(Input.GetKeyDown(KeyCode.Space)) {
                Simulate();
            }
            simulationTimer = simulationRate;
        }
    }

    void Simulate() {
        bool[,] nextAlive = new bool[10,10];

        for(int x = 0; x < 10; x++) {
            for(int y = 0; y < 10; y++) {
                int neighborCount = CountNeighbors(x, y);
                if(grid[x,y].alive) {
                    if(neighborCount == 2 || neighborCount == 3) {
                        nextAlive[x,y] = true;
                    } else {
                        nextAlive[x,y] = false;
                    }
                } else if(!grid[x,y].alive && neighborCount == 3) {
                    nextAlive[x,y] = true;
                } else {
                    nextAlive[x,y] = grid[x,y];
                }
            }
        }

        for(int i = 0; i < 10; i++) {
            for(int j = 0; j < 10; j++) {
                grid[i,j] = nextAlive[i,j];
            }
        }
    }
}
