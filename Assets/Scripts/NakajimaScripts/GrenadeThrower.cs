using UnityEngine;

public class GrenadeThrower : MonoBehaviour
{
    public GameObject grenadePrefab;
    public Transform throwPoint;
    public float throwForce = 15f;
    public TrajectoryDrawer trajectoryDrawer;

    private bool isAiming;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isAiming = true;
        }

        if (Input.GetMouseButton(0) && isAiming)
        {
            Vector3 throwDirection = throwPoint.forward;
            Vector3 velocity = throwDirection * throwForce;

            trajectoryDrawer.DrawTrajectory(throwPoint.position, velocity);
        }

        if (Input.GetMouseButtonUp(0) && isAiming)
        {
            ThrowGrenade();
            isAiming = false;
            trajectoryDrawer.HideTrajectory();
        }
    }

    void ThrowGrenade()
    {
        GameObject grenade = Instantiate(grenadePrefab, throwPoint.position, Quaternion.identity);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        rb.velocity = throwPoint.forward * throwForce;
    }
}
