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

     NetworkCharacterControllerPrototype characterController;


    [SerializeField]
    private Transform cameraTarget;

    public Transform CameraTarget => cameraTarget;

    //private NetworkCharacterControllerPrototype characterController;

    //private void Awake()
    //{
    //    characterController = GetComponent<NetworkCharacterControllerPrototype>();
    //}

    public override void Spawned()
    {
        SetNickName($"Player({Object.InputAuthority.PlayerId})");


        characterController=GetComponent<NetworkCharacterControllerPrototype>();

        if (HasInputAuthority)
        {
            FindObjectOfType<TPSCameraController>().SetCameraToMyAvatar(this);
        }



    }


    public override void FixedUpdateNetwork()
    {

        Debug.Log("NetworkInput");
        if (GetInput(out NetworkInputData data))
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
            headObject.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

            // �L������Y����]��K�p
            Quaternion yRot = Quaternion.Euler(0, transform.eulerAngles.y, 0);

            // ���͕����̃x�N�g���𐳋K������
            //data.wasdInputDirection.Normalize();

            Vector3 moveDirection = yRot * data.wasdInputDirection.normalized;  // ���͕����̃x�N�g���𐳋K������
            // ���͕������ړ������Ƃ��Ă��̂܂ܓn��
            characterController.Move(moveDirection);
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