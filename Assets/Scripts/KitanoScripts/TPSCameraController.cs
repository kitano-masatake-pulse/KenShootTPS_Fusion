using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;
using Fusion;

//CinemachineVirtualCameraにアタッチすること
public class TPSCameraController : MonoBehaviour
{



    [Header("Cinemachine Virtual Camera")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [Header("マウス操作")]
    [SerializeField] private float sensitivityX = 3f;
    [SerializeField] private float sensitivityY = 1.5f;
    [SerializeField] private float minVerticalAngle = -40f;
    [SerializeField] private float maxVerticalAngle = 75f;
    private float yaw = 0f;
    private float pitch = 0f;
    public Transform cameraTarget;
    

    bool isBattleScene = false;

    bool isSetCameraTarget=true;

    private NetworkRunner runner;


    private PlayerAvatar myPlayerAvatar;


    bool cursorLocked = true;
    void Start()
    {
        runner = FindObjectOfType<NetworkRunner>();
        LockCursor();

    }
    void Update()
    {

        
        if ( isSetCameraTarget)
           {
            float mouseX = Input.GetAxis("Mouse X") * sensitivityX;
            float mouseY = Input.GetAxis("Mouse Y") * sensitivityY;
            yaw += mouseX;
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            cameraTarget.rotation = rotation;

            //myPlayerAvatar.ChangeTransformLocally(); // アバターの向きをカメラ方向に合わせる処理を呼び出す


        }

        // Escapeキーでモード切り替え
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (cursorLocked)
                UnlockCursor();
            else
                LockCursor();
        }
    }




    public Transform GetTPSCameraTransform() 
    { 
        return this.gameObject.transform;
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked; // 中央固定
        Cursor.visible = false;
        cursorLocked = true;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None; // 通常カーソル
        Cursor.visible = true;
        cursorLocked = false;
    }

}