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

        //string sceneName = SceneType.Battle.ToSceneName();
        //isBattleScene = SceneManager.GetActiveScene().name == sceneName;
        //if (isBattleScene
        //    )
        //{
        //    virtualCamera = <CinemachineVirtualCamera>();
        //    //cameraTarget = virtualCamera.LookAt;
        //    // 初期方向
        //    yaw = cameraTarget.eulerAngles.y;
        //    pitch = 0;

        //    virtualCamera.Follow = cameraTarget;
        //    virtualCamera.LookAt = cameraTarget;
        //    Cursor.lockState = CursorLockMode.Locked;
        //    Cursor.visible = false;


        //}


        LockCursor();

        string sceneName = SceneType.Battle.ToSceneName();
        isBattleScene =SceneManager.GetActiveScene().name == sceneName;

    }
    void Update()
    {

        
        if (isBattleScene && isSetCameraTarget)
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

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = true)]
    public void RPC_SetCameraToMyAvatar( RpcInfo info = default) 
    {
        string sceneName = SceneType.Battle.ToSceneName();
        isBattleScene = SceneManager.GetActiveScene().name == sceneName;
        

        if (isBattleScene)
        {
            
            //NetworkObject myAvatar=null;
            //while (myAvatar ==null)
            //{
            //    myAvatar = runner.GetPlayerObject(runner.LocalPlayer);
            //    Debug.Log($"MyAvatar :{myAvatar != null}");
            //}
            NetworkObject myAvatar = runner.GetPlayerObject(runner.LocalPlayer);
            
            PlayerAvatar myAvatarScript = myAvatar.GetComponent<PlayerAvatar>();
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
            cameraTarget = myAvatarScript.CameraTarget;
            // 初期方向
            yaw = cameraTarget.eulerAngles.y;
            pitch = 0;

            virtualCamera.Follow = cameraTarget;
            virtualCamera.LookAt = cameraTarget;
        

            isSetCameraTarget = true;
        }

    }


    public void SetCameraToMyAvatar(PlayerAvatar myAvatarScript)
    {
        string sceneName = SceneType.Battle.ToSceneName();
        isBattleScene = SceneManager.GetActiveScene().name == sceneName;

        myPlayerAvatar = myAvatarScript;


        if (isBattleScene)
        {

            //NetworkObject myAvatar = null;
            //while (myAvatar == null)
            //{
            //    myAvatar = runner.GetPlayerObject(runner.LocalPlayer);
            //    Debug.Log($"MyAvatar :{myAvatar != null}");
            //}
            //NetworkObject myAvatar = runner.GetPlayerObject(runner.LocalPlayer);

            //PlayerAvatar myAvatarScript = myAvatar.GetComponent<PlayerAvatar>();
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
            cameraTarget = myAvatarScript.CameraTarget;
            // 初期方向
            yaw = cameraTarget.eulerAngles.y;
            pitch = 0;

            virtualCamera.Follow = cameraTarget;
            virtualCamera.LookAt = cameraTarget;
            //Cursor.lockState = CursorLockMode.Locked;
            //Cursor.visible = false;

            isSetCameraTarget = true;
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