using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove_Local : MonoBehaviour
{

    [SerializeField]
    private GameObject headObject;


    [SerializeField]
    private GameObject bodyObject;

    CharacterController characterController;

    public float moveSpeed = 3f;
    [SerializeField]
    private float gravity = -9.81f;

    private Transform tpsCameraTransform;

    private Vector3 velocity; //��ɏd�͂Ɏg�p


    // Start is called before the first frame update
    void Start()
    {
        TPSCameraController tpsCameraController = FindObjectOfType<TPSCameraController>();

        tpsCameraTransform = tpsCameraController.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }




    public void ChangeTransformLocally()
    {
        // if (!HasInputAuthority) { return; }

        Vector3 cameraForward = tpsCameraTransform.forward;

        Vector3 bodyForward = new Vector3(cameraForward.x, 0f, cameraForward.z).normalized;
        // ���[�J���v���C���[�̈ړ�����



        if (bodyForward.sqrMagnitude > 0.0001f)
        {
            // �v���C���[�{�̂̌������J���������ɉ�]
            bodyObject.transform.forward = bodyForward;
        }

        headObject.transform.up = cameraForward.normalized; // �J�����̕����𓪂̌����ɐݒ�(�A�o�^�[�̓��̎��ɂ���ĕς��邱��)


        Vector3 inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        Vector3 moveDirection = Quaternion.LookRotation(bodyForward, Vector3.up) * inputDirection.normalized;





        // �d�͂����Z�i�������ȗ�����Ε����j
        velocity.y += gravity * Time.deltaTime;
        // �⓹�Ή��FMove�͎����Œn�`�̌X�΂ɍ��킹�Ă����
        characterController.Move((moveDirection * moveSpeed + velocity) * Time.deltaTime);
        // ���n���Ă���Ȃ�d�̓��Z�b�g
        if (characterController.isGrounded)
        {
            velocity.y = 0;
        }
    }


}
