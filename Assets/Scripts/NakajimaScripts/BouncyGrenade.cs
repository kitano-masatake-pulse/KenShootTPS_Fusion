using UnityEngine;

public class BouncyGrenade : MonoBehaviour
{
    private Vector3 velocity;
    private float gravity = -9.81f;
    private float drag = 0.98f;

    public void Initialize(Vector3 initialVelocity)
    {
        velocity = initialVelocity;
    }

    void FixedUpdate()
    {
        // èdóÕâ¡éZ
        velocity.y += gravity * Time.fixedDeltaTime;

        velocity *= drag;

        Vector3 direction = velocity.normalized;
        float distance = velocity.magnitude * Time.fixedDeltaTime;

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, distance))
        {
            velocity = Vector3.Reflect(velocity, hit.normal);
            transform.position = hit.point + hit.normal * 0.01f;

        }
        else
        {
            transform.position += velocity * Time.fixedDeltaTime;
        }
    }
}
