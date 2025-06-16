using Cinemachine;
using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    



    bool isSetCameraTarget=true;

    private NetworkRunner runner;


    private PlayerAvatar myPlayerAvatar;

  


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

    [Header("���R�C����debug�p")]
    //���R�C����debug�p
    public float  debug_recoilAmount_Pitch= 5f; // ���R�C���̊p�x�i�s�b�`�j
    public float debug_recoilSpeed_Pitch = 100f; // ���R�C���̊p���x(�s�b�`�j
    public float debug_recoverSpeed_Pitch = 50f; // ���R�C���񕜂̊p���x(�s�b�`�j
    public float debug_recoilLimit_Pitch = 30f; // ���R�C���̊p�x�����i�s�b�`�j

    public float debug_recoilAmount_Yaw = 0.5f; // ���R�C���̊p�x�i���[�j
    public float debug_recoilSpeed_Yaw=10f;
    public float debug_recoverSpeed_Yaw=5f;
    public float debug_recoilLimit_Yaw = 3f;

    // �e����^�C�v�̃��R�C�������p�^�[���i���[�j
    Dictionary<WeaponType, List<float>> recoilRandomPatterns_Yaw = new Dictionary<WeaponType, List<float>>(); 

    int seed_recoilYaw = 8997; // ���R�C���̗����p�^�[���̃V�[�h�l�i���[�j


    [SerializeField]bool isDebugParameter = false; // �f�o�b�O���[�h���ǂ����̃t���O
    int recoilPatternIndex=0; // ���R�C���p�^�[���̃C���f�b�N�X�i���[�j

    

    void Start()
    {
        runner = FindObjectOfType<NetworkRunner>();
        LockCursor();


        foreach (WeaponType type in Enum.GetValues(typeof(WeaponType)))
        {
            if (type.RecoilAmount_Yaw() > 0f)
            {
                // �e����^�C�v�̃��R�C�������p�^�[���𐶐�
                recoilRandomPatterns_Yaw[type] = GenerateRandomPattern(seed_recoilYaw, type.MagazineCapacity(), -1f, 1f); 
            }
        }

    }
    void Update()
    {
        
        ApplyRecoil(WeaponType.AssaultRifle); // ���̕���^�C�v���w��B���ۂɂ͌��݂̕���^�C�v�ɉ����ĕύX����K�v������܂�

        if ( isSetCameraTarget)
           {
            float mouseX = Input.GetAxis("Mouse X") * sensitivityX;
            float mouseY = Input.GetAxis("Mouse Y") * sensitivityY;

            //Debug.Log($"Mouse X: {mouseX}, Mouse Y: {mouseY}"); // �f�o�b�O�p���O�o��
            yaw += mouseX;
            pitch -= mouseY;

            //Debug.Log($"yaw: {yaw}, pitch: {pitch}"); // �f�o�b�O�p���O�o��

            ApplyRecoil(WeaponType.AssaultRifle); //���R�C���̓K�p

            

            //yaw +=; // ���R�C���̃��[��K�p
            //pitch -= currentRecoil_Pitch; // ���R�C���̃s�b�`��K�p

            //Debug.Log($"Adjusted yaw: {yaw}, Adjusted pitch: {pitch}"); // ���R�C���K�p��̊p�x�����O�o��

            pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
            Quaternion rotation = Quaternion.Euler(pitch - currentRecoil_Pitch, yaw + currentRecoil_Yaw, 0f);
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

        if (isDebugParameter)
        {
            recoilTarget_Pitch = Mathf.Min(currentRecoil_Pitch + debug_recoilAmount_Pitch, debug_recoilLimit_Pitch); // �s�b�`�̃��R�C���ڕW�i���̒l�j
            recoilTarget_Yaw = currentRecoil_Yaw + recoilRandomPatterns_Yaw[weaponType][recoilPatternIndex] * debug_recoilAmount_Yaw; // ���[�̃��R�C���ڕW�i���̒l�j
        }
        else
        {


            recoilTarget_Pitch = Mathf.Min(currentRecoil_Pitch + weaponType.RecoilAmount_Pitch(), debug_recoilLimit_Yaw); // �s�b�`�̃��R�C���ڕW�i���̒l�j
            recoilTarget_Yaw = currentRecoil_Yaw + recoilRandomPatterns_Yaw[weaponType][recoilPatternIndex]*weaponType.RecoilAmount_Yaw(); // ���[�̃��R�C���ڕW�i���̒l�j

        }
            
        recoilPatternIndex++;
        if (recoilPatternIndex >= recoilRandomPatterns_Yaw[weaponType].Count)
        {
            recoilPatternIndex = 0; // �p�^�[���̃C���f�b�N�X�����Z�b�g
        }

    }


    // ���R�C�����̏���
    void Recoil(WeaponType weaponType)
    {
        //���R�C���̖ڕW�p�x�Ɍ������Č��݂̃��R�C���p�x���X�V
        if (isDebugParameter)
        {
            currentRecoil_Pitch = Mathf.MoveTowards(currentRecoil_Pitch, recoilTarget_Pitch, debug_recoilSpeed_Pitch * Time.deltaTime);
            currentRecoil_Yaw = Mathf.MoveTowards(currentRecoil_Yaw, recoilTarget_Yaw, debug_recoilSpeed_Yaw * Time.deltaTime); // ���[�̃��R�C���p�x���X�V

        }
        else
        {
           

            currentRecoil_Pitch = Mathf.MoveTowards(currentRecoil_Pitch, recoilTarget_Pitch, weaponType.RecoilAngularVelocity_Pitch() * Time.deltaTime);
            currentRecoil_Yaw = Mathf.MoveTowards(currentRecoil_Yaw, recoilTarget_Yaw, weaponType.RecoilAngularVelocity_Yaw() * Time.deltaTime); // ���[�̃��R�C���p�x���X�V
        }

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
        
        if (isDebugParameter)
        {
            currentRecoil_Pitch = Mathf.MoveTowards(currentRecoil_Pitch, recoverTarget_Pitch, debug_recoverSpeed_Pitch * Time.deltaTime);
            currentRecoil_Yaw = Mathf.MoveTowards(currentRecoil_Yaw, recoverTarget_Yaw, debug_recoverSpeed_Yaw * Time.deltaTime); // ���[�̃��R�C���񕜊p�x���X�V

        }
        else
        {
            currentRecoil_Pitch = Mathf.MoveTowards(currentRecoil_Pitch, recoverTarget_Pitch, weaponType.RecoverAngularVelocity_Pitch() * Time.deltaTime);
            currentRecoil_Yaw = Mathf.MoveTowards(currentRecoil_Yaw, recoverTarget_Yaw, weaponType.RecoverAngularVelocity_Yaw() * Time.deltaTime); // ���[�̃��R�C���񕜊p�x���X�V
        }

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
        recoilPatternIndex = 0; //���R�C���p�^�[���̃C���f�b�N�X�����Z�b�g

    }

    List<float> GenerateRandomPattern(int seed, int count, float min, float max)
    {
        List<float> patternList = new List<float>(); //�p�^�[�����i�[���郊�X�g
        patternList.Clear(); //���X�g���N���A

        System.Random rand = new System.Random(seed); //�V�[�h�l��ݒ�
        for (int i = 0; i < count; i++)
        {
            float randomValue = (float)rand.NextDouble(); //0����1�͈̔͂Ń����_���Ȕ��a�𐶐�
            randomValue = Mathf.Lerp(min, max, randomValue); //�ŏ��l�ƍő�l�̊Ԃŕ��
            patternList.Add(randomValue); //���X�g�ɒǉ�
        }

        return patternList;


    }

    public void EndFiring()
    {
        recoilPatternIndex= 0; // ���R�C���p�^�[���̃C���f�b�N�X�����Z�b�g


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