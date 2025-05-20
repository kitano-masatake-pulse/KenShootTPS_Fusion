using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;
using Fusion;
public class CinemachineTPSMouseLook : MonoBehaviour
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
    bool isBattleScene = false;
    private NetworkRunner runner;
    void Start()
    {
        runner = FindObjectOfType<NetworkRunner>();
        string sceneName = SceneType.Battle.ToSceneName();
        isBattleScene = SceneManager.GetActiveScene().name == sceneName;
        virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        //cameraTarget = virtualCamera.LookAt;
        // èâä˙ï˚å¸
        yaw = cameraTarget.eulerAngles.y;
        pitch = 0;
        virtualCamera.Follow = cameraTarget;
        virtualCamera.LookAt = cameraTarget;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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






