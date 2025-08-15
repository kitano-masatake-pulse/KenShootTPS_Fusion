using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class LineOfFire : MonoBehaviour
{


    LineRenderer lineRenderer;
    [SerializeField] Vector3 startPoint;
    [SerializeField] Vector3 endPoint;
    float lifeTime= 0.1f; // Duration for which the line will be visible   


    // Start is called before the first frame update
    void Start()
    {
        
        

        

            StartCoroutine(DestroyLineAfterTime(lifeTime)); // Start the coroutine to destroy the line after a certain time
        
    }




    private IEnumerator DestroyLineAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false; // Disable the line renderer
        }
       
       Destroy(this.gameObject); // Destroy the GameObject
    }

    public void SetLinePoints(Vector3 start, Vector3 end)
    {
       
        startPoint = start;
        endPoint = end;
        lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, startPoint); // Update start point
            lineRenderer.SetPosition(1, endPoint); // Update end point
            lineRenderer.enabled = true; // Enable the line renderer to make it visible
        }

    }




    // Update is called once per frame
    void Update()
    {
        
    }
}
