using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellScript : MonoBehaviour
{

    public Renderer cubeRenderer;
    public bool alive = false;
    
    public int xIndex = -1;
    public int yIndex = -1;
    public Color aliveColor;
    public Color deadColor;

    GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        SetColor();
        GameObject gmObj = GameObject.Find("GameManagerObject");

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetColor() {
        if(alive) {
            cubeRenderer.material.color = aliveColor;
        }
        else {
            cubeRenderer.material.color = deadColor;
        }
    }

    void OnMouseDown() {
        alive = !alive;
        SetColor();
        int neighborCount = gameManager.CountNeighbors(xIndex, yIndex);
    }
}
