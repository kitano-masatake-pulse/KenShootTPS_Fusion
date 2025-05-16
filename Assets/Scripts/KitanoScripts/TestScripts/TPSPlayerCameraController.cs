using Cinemachine;
using UnityEngine;

public class TPSPlayerCameraController : MonoBehaviour
{
    public static TPSPlayerCameraController Instance { get; private set; }

    public CinemachineVirtualCamera virtualCam;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject); // ìÒèdê∂ê¨ñhé~
            return;
        }
        Instance = this;
    }
}
