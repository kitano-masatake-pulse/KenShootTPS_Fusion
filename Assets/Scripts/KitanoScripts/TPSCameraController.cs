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

    bool isADSNow = false; //ADS�����ǂ����̃t���O


    bool cursorLocked = true;

    //���R�C���֘A�̕ϐ�(���ׂ�degree�@���@�}�E�X����ł̃J�����p�x��0�Ƃ����Ƃ��̍���)

    private float currentRecoil_Pitch = 0f; // ���݂̃��R�C���p�x�i�s�b�`�j
    private float currentRecoil_Yaw = 0f; // ���݂̃��R�C���p�x�i���[�j
    private float recoilTarget_Pitch = 0f; // ���R�C���̖ڕW�p�x�i�s�b�`�j
    private float recoilTarget_Yaw = 0f; // ���R�C���̖ڕW�p�x�i���[�j
    private float recoverTarget_Pitch = 0f; // ���R�C���񕜂̖ڕW�p�x(�s�b�`)
    private float recoverTarget_Yaw = 0f; // ���R�C���񕜂̖ڕW�p�x(���[)

    bool isRecoiling = false; // ���R�C�������ǂ����̃t���O
    bool isRecovering = false; // ���R�C���񕜒����ǂ����̃t���O


    void Start()
    {
        runner = FindObjectOfType<NetworkRunner>();
        LockCursor();

    }
    void Update()
    {
        
        ApplyRecoil(WeaponType.AssaultRifle); // ���̕���^�C�v���w��B���ۂɂ͌��݂̕���^�C�v�ɉ����ĕύX����K�v������܂�

        if ( isSetCameraTarget)
           {
            float mouseX = Input.GetAxis("Mouse X") * sensitivityX;
            float mouseY = Input.GetAxis("Mouse Y") * sensitivityY;
            yaw += mouseX;
            pitch -= mouseY;

            ApplyRecoil(WeaponType.AssaultRifle); //���R�C���̓K�p
            yaw += currentRecoil_Yaw; // ���R�C���̃��[��K�p
            pitch -= currentRecoil_Pitch; // ���R�C���̃s�b�`��K�p


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


    #region�@���R�C���֘A  

    public void ApplyRecoil(WeaponType weaponType)
    {

        if (isRecovering) { RecoverFromRecoil(weaponType); } //���R�C���񕜂��ɂ��Ȃ��ƁA���R�C�������������̂Ɠ��t���[���Ń��R�C�����J�n����Ă��܂�
        if (isRecoiling)  { Recoil(weaponType);  }



    }

    // ���R�C���J�n���̏���(�ˌ�����PlayerAvatar����Ă΂��)
    public void StartRecoil(WeaponType weaponType)
    {
        // ���R�C���J�n���̏���
        isRecoiling = true;
        isRecovering = false;

        // ���R�C���̖ڕW�p�x��ݒ�i���̒l�B���ۂɂ͕���^�C�v�ɉ����Ē�������K�v������܂��j
        recoilTarget_Pitch = currentRecoil_Pitch+weaponType.RecoilAmount_Pitch(); // �s�b�`�̃��R�C���ڕW�i���̒l�j
        recoilTarget_Yaw = currentRecoil_Yaw + weaponType.RecoilAmount_Yaw(); // ���[�̃��R�C���ڕW�i���̒l�j

    }


    // ���R�C�����̏���
    void Recoil(WeaponType weaponType)
    {
        //���R�C���̖ڕW�p�x�Ɍ������Č��݂̃��R�C���p�x���X�V
        currentRecoil_Pitch= Mathf.MoveTowards(currentRecoil_Pitch, recoilTarget_Pitch, weaponType.RecoilAngularVelocity_Pitch() * Time.deltaTime);


        //�ڕW�ɒB�����烊�R�C�����I��
        if (Mathf.Approximately( currentRecoil_Pitch, recoilTarget_Pitch  ))
        {
            //���R�C���񕜂��J�n
            StartRecoverFromRecoil(weaponType);
        }

        

    }


    // ���R�C���񕜊J�n���̏���
    public void StartRecoverFromRecoil(WeaponType weaponType)
    {
        isRecoiling = false;
        isRecovering = true;
       
        // ���R�C���̖ڕW�p�x��ݒ�i���̒l�B���ۂɂ͕���^�C�v�ɉ����Ē�������K�v������܂��j
        recoverTarget_Pitch = 0f; // �s�b�`�̃��R�C���񕜖ڕW
        recoverTarget_Yaw = 0f; // ���[�̃��R�C���񕜖ڕW
    }



    public void RecoverFromRecoil(WeaponType weaponType)
    {

        // ���R�C���񕜂̖ڕW�p�x�Ɍ������Č��݂̃��R�C���p�x���X�V
        currentRecoil_Pitch = Mathf.MoveTowards(currentRecoil_Pitch, recoverTarget_Pitch, weaponType.RecoverAngularVelocity_Pitch() * Time.deltaTime);



        // �ڕW�ɒB�����烊�R�C���񕜂��I��
        if (Mathf.Approximately(currentRecoil_Pitch, recoverTarget_Pitch))
        {
            isRecovering = false;

        }

    }


    //���R�C�������Z�b�g����(����؂�ւ�����PlayerAvatar����Ă΂��)
    public void ResetRecoil()
    {
        currentRecoil_Pitch = 0f;
        currentRecoil_Yaw = 0f;
        isRecoiling = false;
        isRecovering = false;

    }


    #endregion




    #region �J�[�\���֘A
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

    #endregion


    public Transform GetTPSCameraTransform()
    {
        return this.gameObject.transform;
    }

}