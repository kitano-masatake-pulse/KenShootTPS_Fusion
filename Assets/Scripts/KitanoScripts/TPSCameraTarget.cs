using UnityEngine;

public class TPSCameraTarget : MonoBehaviour
{
    public Transform player; // ƒvƒŒƒCƒ„[‚ÌbodyObject‚È‚Ç
    public Vector3 offset = new Vector3(0, 1.6f, 0);

    void LateUpdate()
    {
        if (player != null)
        {
            transform.position = player.position + offset;
        }
    }
}
