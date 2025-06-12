using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;
using Fusion;

//CinemachineVirtualCamera�ɃA�^�b�`���邱��
public class TPSCameraController : MonoBehaviour
{



    [Header("Cinemachine Virtual Camera")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [Header("�}�E�X����")]
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

            //myPlayerAvatar.ChangeTransformLocally(); // �A�o�^�[�̌������J���������ɍ��킹�鏈�����Ăяo��


        }

        // Escape�L�[�Ń��[�h�؂�ւ�
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
        Cursor.lockState = CursorLockMode.Locked; // �����Œ�
        Cursor.visible = false;
        cursorLocked = true;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None; // �ʏ�J�[�\��
        Cursor.visible = true;
        cursorLocked = false;
    }

}