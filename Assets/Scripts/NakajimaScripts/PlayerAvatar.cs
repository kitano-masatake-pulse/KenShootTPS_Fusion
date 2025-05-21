using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

public class PlayerAvatar : NetworkBehaviour
{
    [SerializeField]
    private TextMeshPro nameLabel;

    [SerializeField]
    private GameObject headObject;
    

    [SerializeField]
    private GameObject bodyObject;

    CharacterController characterController;

    public float moveSpeed = 3f;
    public float gravity = -9.81f;

    [SerializeField]
    private Transform cameraTarget;

    public Transform CameraTarget => cameraTarget;

    private Vector3 velocity; //��ɏd�͂Ɏg�p

    //private NetworkCharacterControllerPrototype characterController;

    //private void Awake()
    //{
    //    characterController = GetComponent<NetworkCharacterControllerPrototype>();
    //}



    public override void Spawned()
    {
        SetNickName($"Player({Object.InputAuthority.PlayerId})");


        
        characterController = GetComponent<CharacterController>();

        if (HasInputAuthority)
        {
            //�����̃A�o�^�[�Ȃ�ATPS�J�����ɕR�Â���
            FindObjectOfType<TPSCameraController>().SetCameraToMyAvatar(this);
        }



    }


    public override void FixedUpdateNetwork()
    {

        Debug.Log("NetworkInput");
        if ( GetInput(out NetworkInputData data))
        {


            Vector3 bodyForward = new Vector3(data.cameraForward.x, 0f, data.cameraForward.z).normalized;

            if (bodyForward.sqrMagnitude > 0.0001f)
            {
                // �v���C���[�{�̂̌������J���������ɉ�]
                bodyObject.transform.forward = bodyForward;
            }

            // cameraForward ���� pitch �����߂� ��W���O�ʂ������悤��
            float pitch = - Mathf.Asin(data.cameraForward.y) * Mathf.Rad2Deg + 90;
            
            // ������]���p�����Ɍ���
            //headObject.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

            //�̂Ɠ������@��forward�����Ă݂Ď�������
            Vector3 headUp = data.cameraForward.normalized;

            headObject.transform.up = headUp;

            // �L������Y����]��K�p
            // Quaternion yRot = Quaternion.Euler(data.cameraForward.x, 0f, data.cameraForward.z);
            // Quaternion yRot = Quaternion.Euler(data.cameraForward.x, 0f, data.cameraForward.z);

            // ���͕����̃x�N�g���𐳋K������
            //data.wasdInputDirection.Normalize();

            Vector3 moveDirection =Quaternion.LookRotation(bodyForward,Vector3.up) * data.wasdInputDirection;  // ���͕����̃x�N�g���𐳋K������
                                                                                                               // ���͕������ړ������Ƃ��Ă��̂܂ܓn��


            // �d�͂����Z�i�������ȗ�����Ε����j
           
            velocity.y += gravity * Runner.DeltaTime;

            // �⓹�Ή��FMove�͎����Œn�`�̌X�΂ɍ��킹�Ă����
            characterController.Move((moveDirection * moveSpeed + velocity) * Runner.DeltaTime);

            // ���n���Ă���Ȃ�d�̓��Z�b�g
            if (characterController.isGrounded)
            {
                velocity.y = 0;
            }

            //bodyObject.transform.Translate(moveDirection*moveSpeed*Runner.DeltaTime);
            //characterController.Move(moveDirection);
        }
    }
    private void LateUpdate()
    {
        // �v���C���[���̃e�L�X�g���A��ɃJ�����̐��ʌ����ɂ���
        nameLabel.transform.rotation = Camera.main.transform.rotation;
    }

    // �v���C���[�����e�L�X�g�ɐݒ肷��
    public void SetNickName(string nickName)
    {
        nameLabel.text = nickName;
    }
}