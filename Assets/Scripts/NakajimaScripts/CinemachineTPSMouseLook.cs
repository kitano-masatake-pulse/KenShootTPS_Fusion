using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;
using Fusion;
public class CinemachineTPSMouseLook : NetworkBehaviour
{
    [Header("Cinemachine Virtual Camera")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [Header("É}ÉEÉXëÄçÏ")]
    [SerializeField] private float sensitivityX = 3f;
    [SerializeField] private float sensitivityY = 1.5f;
    [SerializeField] private float minVerticalAngle = -40f;
    [SerializeField] private float maxVerticalAngle = 75f;
    private float yaw = 0f;
    private float pitch = 0f;
    public Transform cameraTarget;
    void Start()
    {
        string sceneName = SceneType.Battle.ToSceneName();
        if (HasInputAuthority && SceneManager.GetActiveScene().name == sceneName)
        {
            virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
            virtualCamera.Follow = cameraTarget;
            virtualCamera.LookAt = cameraTarget;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            cameraTarget = virtualCamera.LookAt;
            // èâä˙ï˚å¸
            yaw = cameraTarget.eulerAngles.y;
            pitch = 0;
        }
    }
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivityX;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivityY;
        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        cameraTarget.rotation = rotation;
    }
}