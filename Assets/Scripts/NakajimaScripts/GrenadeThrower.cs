using UnityEditor;
using UnityEngine;

// �O���l�[�h�𓊂��邽�߂̃X�v���N�g
public class GrenadeThrower : MonoBehaviour
{
    // �O���l�[�h�̃v���n�u
    public GameObject grenadePrefab;
    // ������ʒu
    public Transform throwPoint;
    // ������͂̑傫��
    private float throwForce = 10f;
    // �O�Օ`��̃X�N���v�g
    public TrajectoryDrawer trajectoryDrawer;
    // �A�j���[�^�\�^�ϐ�
    public Animator animator;
    // ����������x�N�g��
    Vector3 throwDirection;
    // ������͂̑傫��
    Vector3 velocity;

    // �������������ǂ����̃t���O
    private bool isAimingReady;
    private bool hasEnterThrowOnce;
    void Update()
    {

        // ���������������[�V�����̊�LineRenderer��\��
        // �{�^����������ł��A�A�j���[�V�����͏��������̂ŁA�t���O����}��
        if (animator.GetCurrentAnimatorStateInfo(1).IsName("Prepare to throw loop"))
        {
            Debug.Log("Throwing grenade Loop...");
            //�@������������擾����
            throwDirection = throwPoint.forward;
            //�@������͂̑傫�����v�Z����
            velocity = throwDirection * throwForce;

            // �֐��Ăяo���i������ʒu,����������Ƒ傫���j
            trajectoryDrawer.RaycastDrawTrajectory(throwPoint.position, velocity);
            isAimingReady = true;
        }

        if (animator.GetCurrentAnimatorStateInfo(1).IsName("Throw"))
        {
            if (!hasEnterThrowOnce)
            {
                // �O���l�[�h�𓊂���
                ThrowGrenade(velocity);
                // �O�Ղ��\���ɂ���
                trajectoryDrawer.HideTrajectory();
                hasEnterThrowOnce = true;
                isAimingReady = false;
            }
        }
        else
        {
            hasEnterThrowOnce = false;
        }

        // �}�E�X�̍��{�^���������ꂽ���̏���
        if (Input.GetMouseButtonUp(0) && isAimingReady)
        {
            //// Update���\�b�h����Debug.Log�s���ȉ��̂悤�ɕύX
            //Debug.Log($"Throwing grenade... isAimingReady: {isAimingReady}");
            //// �O���l�[�h�𓊂���
            //ThrowGrenade(velocity);
            //// �O�Ղ��\���ɂ���
            //trajectoryDrawer.HideTrajectory();
        }

    }

    void ThrowGrenade(Vector3 velocity)
    {
        // �O���l�[�h�̃C���X�^���X�𐶐�
        GameObject grenade = Instantiate(grenadePrefab, throwPoint.position, Quaternion.identity);
        // �O���l�[�h��Rigidbody�R���|�[�l���g���擾���A������͂�ݒ�
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        rb.velocity = velocity;
    }

    void OnArmRaised()
    {

    }
    
}