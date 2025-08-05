using UnityEngine;
using System.Collections.Generic;
using Fusion;

//�O�Օ`��X�v���N�g
public class TrajectoryDrawer : MonoBehaviour
{
    public LineRenderer lineRenderer;
    //�O�Ղ̕`��_�̐�
    private int resolution = 3000;
    //�O�Ղ̎��ԊԊu
    private float timeStep = 1/60f;
    //�ǂ̃��C���[�}�X�N�ɋO�Ղ̓����蔻�肪���邩
    public LayerMask collisionMask;

    //�O�Ղ̏I���_�ɕ\���X���v���n�u
    public GameObject impactMarkerPrefab;
    //�Փ˂����ʒu�ɕ\������}�[�J�[�̃C���X�^���X
    private GameObject impactMarkerInstance;
    //�O�Ղ̏Փ˓_�������������ǂ����̃t���O
    private bool impactPointFound = false;

    private void Awake()
    {
        // �������F�C���X�^���X�����i1�����j
        if (impactMarkerPrefab != null)
        {
            impactMarkerInstance = Instantiate(impactMarkerPrefab);
            // �ŏ��͔�\��
            impactMarkerInstance.SetActive(false); 
        }
    }


    // �O�Օ`��֐��i������ʒu,����������Ƒ傫���j
    public void RaycastDrawTrajectory(Vector3 startPos, Vector3 velocity)
    {

        Debug.Log("SphereCastDrawTrajectory called with startPos: " + startPos + ", velocity: " + velocity);
        // �O�Ղ̕`��_�̐���ݒ�
        lineRenderer.positionCount = resolution;
        // �d�͎擾
        Vector3 gravity = Physics.gravity;
        // �O�Ղ̏Փ˃t���O�͓��� false
        impactPointFound = false;

        // �`�悷��񐔂��肩����
        for (int i = 0; i < resolution; i++)
        {
            // ��_���Ƃ̎��Ԃ̌v�Z
            float t = i * timeStep;
            // �O�Ղ̈ʒu���v�Z
            Vector3 point = startPos + velocity * t + 0.5f * gravity * t * t;
            // LineRenderer�̕`��
            lineRenderer.SetPosition(i, point);

            // �Փ˗\���i�I�v�V�����j
            if (i > 0)
            {
                // �O�̕`��_�̈ʒu���擾
                Vector3 prev = lineRenderer.GetPosition(i - 1);
                // ���݂̕`��X�̑O�̕`��X�̈ʒu�̍������擾
                Vector3 direction = point - prev;
                // �x�N�g���̒������v�Z����
                float distance = direction.magnitude;

                // ���C�L���X�g�i���݂̕`��_�ʒu, �����𐳋K��, �Փ˃I�u�W�F�N�g�擾, �x�N�g���̒���, �Փ˗L���ɂ���I�u�W�F�N�g�}�X�N�j
                if (Physics.Raycast(point, direction.normalized, out RaycastHit hit, distance, collisionMask))
                {
                    // ��̕`��_���擾���A�`�悷��
                    lineRenderer.positionCount = i + 1;
                    lineRenderer.SetPosition(i, hit.point);
                    // �C���X�^���X�����݂��Ă����
                    if (impactMarkerInstance != null)
                    {
                        // �}�[�J�[��\������
                        impactMarkerInstance.SetActive(true);
                        // �}�[�J�[�̈ʒu���Փ˓_�ɐݒ�
                        impactMarkerInstance.transform.position = hit.point;
                        // �}�[�J�[�̉�]���Փ˖ʂ̖@���ɍ��킹��
                        impactMarkerInstance.transform.rotation = Quaternion.LookRotation(hit.normal);
                        // �}�[�J�[�t���O�� true �ɐݒ�
                        impactPointFound = true;
                    }
                    // �Փ˓_�����������̂ŁA���[�v�𔲂���
                    break;
                }
            }
        }
        // �Փ˓_��������Ȃ������ꍇ�A�}�[�J�[���\���ɂ���
        if (!impactPointFound && impactMarkerInstance != null)
        {
            impactMarkerInstance.SetActive(false);
        }   
    }



    public void HideTrajectory()
    {
        // �O�Ղ̕`��_�����Z�b�g
        lineRenderer.positionCount = 0;
        // �}�[�J�[���\���ɂ���
        if (impactMarkerInstance != null)
            impactMarkerInstance.SetActive(false);
    }
}
