using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoScript : MonoBehaviour
{

    //public Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // this.gameObject.transform.position = new Vector3(this.gameObject.transform.position.x + 0.001f, 
        //                                                 this.gameObject.transform.position.y, 
        //                                                 this.gameObject.transform.position.z);

        if(Input.GetKeyDown(KeyCode.Space)) {
            Rigidbody rb = this.gameObject.GetComponent<Rigidbody>();
            if (rb != null) {
                rb.useGravity = true;
            }
        }

 
    }
}
