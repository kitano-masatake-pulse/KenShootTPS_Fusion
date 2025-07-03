using UnityEngine;

public class BouncyGrenade : MonoBehaviour
{
    private Vector3 velocity;
    private Rigidbody rb;
    private Vector3 lastPosition;

    private Vector3 twolastPosition;

    private int bounceCount = 0;    

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        ////Vector3 delta = transform.position - lastPosition;
        //Vector3 delta = lastPosition - twolastPosition;

        //float distance = delta.magnitude;

        //Vector3 direction = (distance > 0f) ? delta.normalized : Vector3.zero;

        //velocity = rb.velocity;

        //if (Physics.Raycast(lastPosition, direction, out RaycastHit hit, distance))
        //{
        //    if (hit.collider.gameObject != this.gameObject)
        //    {
        //        velocity = Vector3.Reflect(velocity, hit.normal);
        //        Debug.Log("Hit: " + hit.collider.name + " bounceCount " + bounceCount);
        //        bounceCount++;
        //    }
        //}
        //rb.velocity = velocity;

        //lastPosition = transform.position;

        ////twolastPosition = lastPosition;
    }
}
