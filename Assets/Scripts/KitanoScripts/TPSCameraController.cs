using Cinemachine;
using Fusion;
using RootMotion.Demos;
using System;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.UI.Image;

//CinemachineVirtualCamera�ɃA�^�b�`���邱��
public class TPSCameraController : MonoBehaviour
{



    [Header("Cinemachine Virtual Camera")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    Vector3 initialShoulderOffset=Vector3.zero; // �����̃V�����_�[�I�t�Z�b�g

    [Header("�}�E�X����")]



    [SerializeField] private float minVerticalAngle = -40f;
    [SerializeField] private float maxVerticalAngle = 75f;
    private float yaw = 0f;
    private float pitch = 0f;
    public Transform cameraTarget;

    bool isSetCameraTarget=true;
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



    [Header("��Q������")]
    [SerializeField] private LayerMask obstacleLayer; // ��Q���̃��C���[
    [SerializeField] private float obstacleBuffer = 0.05f; // ��Q������̋���

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


    #region ADS�֘A

    [Header("References")]
    private Cinemachine3rdPersonFollow thirdPersonFollow;

    [Header("Camera Transition Settings")]
    [SerializeField] private Vector3 normalShoulderOffset = new Vector3(0.5f, 0, 0f);
    [SerializeField] private Vector3 adsShoulderOffset = new Vector3(0f, 0f, 0f);
    [SerializeField] private float normalDistance = 3.5f;
    [SerializeField] private float adsDistance = 2.0f;
    [SerializeField] private float offsetLerpSpeed = 10f;

    [Header("FOV Settings")]
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float adsFOV = 30f;
    [SerializeField] private float fovLerpSpeed = 15f;

    [Header("Mouse Sensitivity")]
    [SerializeField] private Vector2 normalSensitivity = new Vector2(3.0f,1.5f);
    [SerializeField] private Vector2 adsSensitivity = new Vector2(1.5f, 0.75f);
    private Vector2 currentSensitivity;
    [SerializeField] private float mouseSenRange = 4.0f; // �}�E�X���x�͈̔� 
    // �}�E�X���x�iX, Y�j��␳����ϐ�
    private float sensitivityMultiplier;
    // X�EY���̔��]�i1: ����, -1: ���]�j
    private int directionX = 1; 
    private int directionY = 1;


    [SerializeField]float ADSRecoilMultiplier = 0.5f; // ADS���̃��R�C���{��
    float currentRecoilMultiplier = 1f; // ���݂̃��R�C���{��


    private Vector3 targetOffset;
    private float targetDistance;
    private float targetFOV;

    private bool isADS = false;

    WeaponType currentRecoilingWeapon= WeaponType.AssaultRifle; // ���݃��R�C�����̕���^�C�v�i����AssaultRifle���g�p�j



    CameraInputData cameraInputData; // �J�������̓f�[�^

    #endregion
    private void OnEnable()
    {
        OptionsManager.Instance.OnApplied -= UpdateMouseOption; // �I�v�V�����K�p�C�x���g���w�ǉ���
        OptionsManager.Instance.OnApplied += UpdateMouseOption; // �I�v�V�����K�p�C�x���g���w��

    }
    private void OnDisable()
    {
        OptionsManager.Instance.OnApplied -= UpdateMouseOption; // �I�v�V�����K�p�C�x���g���w�ǉ���
    }


    void Start()
    {
        thirdPersonFollow = virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();


        

        InitializeADS(); // ADS�̏�����

        foreach (WeaponType type in Enum.GetValues(typeof(WeaponType)))
        {
           
                // �e����^�C�v�̃��R�C�������p�^�[���𐶐�
                recoilRandomPatterns_Yaw[type] = GenerateRandomPattern(seed_recoilYaw, type.MagazineCapacity(), -1f, 1f); 
            
        }

    }
    void Update()
    {

        cameraInputData = CameraInputData.Default();
        if (!LocalInputHandler.isOpenMenu) { 
           cameraInputData = LocalInputHandler.CollectCameraInput(); // �J�������̓f�[�^���擾
        }
        //ApplyRecoil(WeaponType.AssaultRifle); // ���̕���^�C�v���w��B���ۂɂ͌��݂̕���^�C�v�ɉ����ĕύX����K�v������܂�
        ApplyRecoil(currentRecoilingWeapon); //���R�C���̓K�p

        if ( isSetCameraTarget)
           {
            float mouseX = cameraInputData.mouseMovement.x * currentSensitivity.x * sensitivityMultiplier*directionX;
            float mouseY = cameraInputData.mouseMovement.y * currentSensitivity.y * sensitivityMultiplier*directionY;

            //Debug.Log($"Mouse X: {mouseX}, Mouse Y: {mouseY}"); // �f�o�b�O�p���O�o��
            yaw += mouseX;
            pitch -= mouseY;

            //Debug.Log($"yaw: {yaw}, pitch: {pitch}"); // �f�o�b�O�p���O�o��

            //ApplyRecoil(currentRecoilingWeapon); //���R�C���̓K�p

            

            //yaw +=; // ���R�C���̃��[��K�p
            //pitch -= currentRecoil_Pitch; // ���R�C���̃s�b�`��K�p

            pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);
            Quaternion rotation = Quaternion.Euler(pitch - currentRecoil_Pitch, yaw + currentRecoil_Yaw, 0f);
            cameraTarget.rotation = rotation;

            //myPlayerAvatar.ChangeTransformLocally(); // �A�o�^�[�̌������J���������ɍ��킹�鏈�����Ăяo��
            if (thirdPersonFollow == null) return;



            

            ADStransition(); // ADS�̕�ԏ������Ăяo��

            
        }

    }

    #region �}�E�X���x����
    public void UpdateMouseOption(OptionData data)
    {  
        float mouseSensitivity = data.mouseSensitivity;
        //�ΐ��Ń}�E�X���x��1/4�`4�{�ɒ���(�f�t�H���g�l)
        sensitivityMultiplier = Mathf.Pow(mouseSenRange, mouseSensitivity);

        bool inverX = data.invertX; 
        bool inverY = data.invertY;
        directionX = inverX ? -1 : 1; 
        directionY = inverY ? -1 : 1;
    }
    #endregion

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
        currentRecoilingWeapon = weaponType; 



        // ���R�C���̖ڕW�p�x��ݒ�i���̒l�B���ۂɂ͕���^�C�v�ɉ����Ē�������K�v������܂��j

        if (isDebugParameter)
        {
            recoilTarget_Pitch = Mathf.Min(currentRecoil_Pitch + debug_recoilAmount_Pitch, debug_recoilLimit_Pitch) * currentRecoilMultiplier; // �s�b�`�̃��R�C���ڕW�i���̒l�j
            if (weaponType.RecoilAmount_Yaw() > 0f) recoilTarget_Yaw = currentRecoil_Yaw + recoilRandomPatterns_Yaw[weaponType][recoilPatternIndex] * debug_recoilAmount_Yaw * currentRecoilMultiplier; // ���[�̃��R�C���ڕW�i���̒l�j
        }
        else
        {


            recoilTarget_Pitch = Mathf.Min(currentRecoil_Pitch + weaponType.RecoilAmount_Pitch(), weaponType.RecoilLimit_Pitch()) * currentRecoilMultiplier; // �s�b�`�̃��R�C���ڕW�i���̒l�j
            if (weaponType.RecoilAmount_Yaw() > 0f) recoilTarget_Yaw = currentRecoil_Yaw + recoilRandomPatterns_Yaw[weaponType][recoilPatternIndex]*weaponType.RecoilAmount_Yaw() *currentRecoilMultiplier ; // ���[�̃��R�C���ڕW�i���̒l�j

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
            currentRecoil_Pitch = Mathf.MoveTowards(currentRecoil_Pitch, recoilTarget_Pitch, debug_recoilSpeed_Pitch * currentRecoilMultiplier * Time.deltaTime);
           currentRecoil_Yaw = Mathf.MoveTowards(currentRecoil_Yaw, recoilTarget_Yaw, debug_recoilSpeed_Yaw * currentRecoilMultiplier * Time.deltaTime); // ���[�̃��R�C���p�x���X�V

        }
        else
        {
           

            currentRecoil_Pitch = Mathf.MoveTowards(currentRecoil_Pitch, recoilTarget_Pitch, weaponType.RecoilAngularVelocity_Pitch() * currentRecoilMultiplier * Time.deltaTime);
            currentRecoil_Yaw = Mathf.MoveTowards(currentRecoil_Yaw, recoilTarget_Yaw, weaponType.RecoilAngularVelocity_Yaw() * currentRecoilMultiplier * Time.deltaTime); // ���[�̃��R�C���p�x���X�V
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
            currentRecoil_Pitch = Mathf.MoveTowards(currentRecoil_Pitch, recoverTarget_Pitch, debug_recoverSpeed_Pitch * currentRecoilMultiplier * Time.deltaTime);
            currentRecoil_Yaw = Mathf.MoveTowards(currentRecoil_Yaw, recoverTarget_Yaw, debug_recoverSpeed_Yaw * currentRecoilMultiplier * Time.deltaTime); // ���[�̃��R�C���񕜊p�x���X�V

        }
        else
        {
            currentRecoil_Pitch = Mathf.MoveTowards(currentRecoil_Pitch, recoverTarget_Pitch, weaponType.RecoverAngularVelocity_Pitch() * currentRecoilMultiplier * Time.deltaTime);
            currentRecoil_Yaw = Mathf.MoveTowards(currentRecoil_Yaw, recoverTarget_Yaw, weaponType.RecoverAngularVelocity_Yaw() * currentRecoilMultiplier * Time.deltaTime); // ���[�̃��R�C���񕜊p�x���X�V
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

    #region ADS�֘A

    public void SetADS(bool ADSflag)
    {
        isADS= ADSflag;

        // �J�����񂹁EFOV�؂�ւ�
        targetOffset = ADSflag ? adsShoulderOffset : normalShoulderOffset;
        targetDistance = ADSflag ? adsDistance : normalDistance;
        targetFOV = ADSflag ? adsFOV : normalFOV;


        // ���x�ƃ��R�C���{��
        currentSensitivity = ADSflag ? adsSensitivity : normalSensitivity;
        currentRecoilMultiplier = ADSflag ? ADSRecoilMultiplier : 1f; // ADS���̃��R�C���{����K�p


        //currentRecoilMultiplier = isADS ? adsRecoilMultiplier : normalRecoilMultiplier;

        // �T�C�gUI
        //if (adsSightUI != null)
        //    adsSightUI.SetActive(isADS);
    }

    void ADStransition()
    {
        // ��ԏ���
        thirdPersonFollow.ShoulderOffset = Vector3.Lerp(
            thirdPersonFollow.ShoulderOffset,
            targetOffset,
            Time.deltaTime * offsetLerpSpeed
        );

        thirdPersonFollow.CameraDistance = Mathf.Lerp(
            thirdPersonFollow.CameraDistance,
            targetDistance,
            Time.deltaTime * offsetLerpSpeed
        );

        virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(
            virtualCamera.m_Lens.FieldOfView,
            targetFOV,
            Time.deltaTime * fovLerpSpeed
        );

    }

    void InitializeADS()
    {
        isADS = false;
        targetOffset = normalShoulderOffset;
        targetDistance = normalDistance;
        targetFOV = normalFOV;
        currentSensitivity = normalSensitivity;
        currentRecoilMultiplier= 1f;

    }

    public void CancelADS()
    {
        InitializeADS();
        thirdPersonFollow.ShoulderOffset= normalShoulderOffset;
        thirdPersonFollow.CameraDistance = normalDistance;
        virtualCamera.m_Lens.FieldOfView = normalFOV;
        currentSensitivity = normalSensitivity;
        currentRecoilMultiplier = 1f;



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


    // TPS�J������Transform���擾���郁�\�b�h
    //cinemachine collider�ɓ����@�\�������̂ŁA������͎g�p���Ȃ��I�I�I�I�I
    void ControlDistanceWithCollision()
    {
        Debug.Log("ControlDistanceWithCollision called"); // �f�o�b�O�p���O�o��
        Vector3 rayStartPos = CalculateCameraPosition(cameraTarget, thirdPersonFollow.ShoulderOffset,adsDistance);

        Vector3 rayEndPos = CalculateCameraPosition(cameraTarget, thirdPersonFollow.ShoulderOffset, normalDistance);


        Vector3 direction = rayEndPos- rayStartPos ; // �J�����̈ʒu����^�[�Q�b�g�̈ʒu�ւ̕����x�N�g�����v�Z

        RaycastHit hit;


        if (Physics.Raycast(rayStartPos , direction/ direction.magnitude, out hit, direction.magnitude, obstacleLayer))
        {
            // hitLayers �Ɋ܂܂�郌�C���[�̃I�u�W�F�N�g�������o

            Debug.Log($"Obstacle detected at distance: {hit.distance}"); // ��Q�����o�̃f�o�b�O�p���O�o��
            thirdPersonFollow.CameraDistance = hit.distance - obstacleBuffer; // �J�����̋�������Q���܂ł̋�������o�b�t�@���������l�ɐݒ�
            


        }
        else
        {
            // ��Q�����Ȃ��ꍇ�̓J�����̈ʒu���X�V
            
            thirdPersonFollow.CameraDistance= targetDistance; // targetDistance���J�����̋����ɐݒ�
        }   




    }

    Vector3 CalculateCameraPosition(Transform target,Vector3 shoulderOffset,float cameraDistance)
    {
        // 1) ���[�J����Ԃł̃I�t�Z�b�g�x�N�g�����쐬
        //    X : ���i�E���������j
        //    Y : �㉺�i����������j
        //    Z : ���s�i�w��F-cameraDistance�j
        var localOffset = new Vector3(
            shoulderOffset.x,
            shoulderOffset.y,
            shoulderOffset.z - cameraDistance
        );

        // 2) target �̉�]���g���ă��[�J�������[���h�����ɕϊ�
        //    TransformDirection �͉�]�݂̂�K�p
        Vector3 worldOffset = target.TransformDirection(localOffset);

        // 3) ���[���h���W��̍ŏI�ʒu���v�Z
        return target.position + worldOffset;
    }

}