using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject dominoPrefab;

    GameObject firstDomino;

    // Start is called before the first frame update
    void Start()
    {
        // GameObject domino = Instantiate(dominoPrefab, transform.position, Quaternion.identity);
        // Vector3 inFrontOfDomino1 = domino.transform.position + domino.transform.forward;
        // //Instantiate(dominoPrefab, inFrontOfDomino1, Quaternion.identity);
        // GameObject domino2 = Instantiate(dominoPrefab, transform.position, Quaternion.identity);
        // Vector3 inFrontOfDomino2 = domino.transform.position + domino.transform.forward;

        Vector3 startPosition = transform.position;


        for (int i = 0; i < 100; i++) {
            //make a domino
            //makes a position
            
            Vector3 position = startPosition + transform.forward * i;
            //GameObject domino = Instantiate(dominoPrefab, transform.position, Quaternion.identity); 

            float amp = 2f;
            float freq = 0.5f;
            position += transform.right * amp * Mathf.Sin(i * freq);

            GameObject domino = Instantiate(dominoPrefab, transform.position, Quaternion.identity);
            Renderer rend = domino.GetComponentInChildren<Renderer>();
            rend.getMaterial.color = Color.HSVToRGB(i * 0.01f, 0.4f, 1f);

            if(i == 0) {
                firstDomino = domino;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)) {
            //gets rigidbody component with getComponent
            RigidBody rb = firstDomino.getComponent<Rigidbody>();
            //add force takes vector as input
            rb.AddForce(firstDomino.transform.forward * 300);
        }
    }
}
