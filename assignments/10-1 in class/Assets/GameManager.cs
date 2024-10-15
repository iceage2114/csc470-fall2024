using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject testPrefab;
    prefabScript[] array;

    // Start is called before the first frame update
    void Start()
    {
        array = new prefabScript[5];

        for(int i = 1; i < 6; i++) {
            Vector3 pos = transform.position;
            pos.x += i * 1.1f;
            //pos.z += i;
            GameObject testCell = Instantiate(testPrefab, pos, Quaternion.identity);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
