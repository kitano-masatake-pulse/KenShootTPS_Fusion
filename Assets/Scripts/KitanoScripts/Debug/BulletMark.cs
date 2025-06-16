using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMark : MonoBehaviour
{

    [SerializeField] float duration = 3f; // Duration before the bullet mark is destroyed

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DestroyCoRoutine());
    }


    IEnumerator DestroyCoRoutine()
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
