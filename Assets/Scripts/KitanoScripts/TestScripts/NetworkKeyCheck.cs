using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class NetworkKeyCheck : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void FixedUpdateNetwork()
    {
        // Check if the player is the local player
        //Debug.Log("FixedUpdateNetwork");
        // Check for input
        if (Input.GetKey(KeyCode.Space))
            {
                Debug.Log("Network Space key pressed.");
            }
        
    }
}
