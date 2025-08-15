using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KetCheck : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Update is running.");

        if (Input.GetKey(KeyCode.Space))
        {
            Debug.Log("Space key pressed.");
        }
    }
}
