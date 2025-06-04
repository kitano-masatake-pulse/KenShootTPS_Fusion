using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class Sword : WeaponBase
{
    [SerializeField]private LayerMask playerLayer;
    [SerializeField]private LayerMask obstructionMask;
    Transform myHeadTransform;
    Transform myPlayerTransform;
    PlayerAvatar myPlayerAvatar;


    

    [SerializeField] private float attackActiveTime = 0.5f; // �����蔻�肪�o�鎞��

    


    
    private bool isAttackActive = false; // �U�����肪�o�Ă��邩�ǂ���

    private void Start()
    {
        myPlayerAvatar= GetComponentInParent<PlayerAvatar>();
        myHeadTransform = myPlayerAvatar.headObject.transform;

    }



    // Update is called once per frame
    void Update()
    {

    }


    public override void FireDown()
    {
      
        base.FireDown();
            StartAttack(); // �U���J�n

    }


    //void StartHoming( Transform targetTransform)
    //{
    //    if (targetTransform != null)
    //    {
    //        // �����������^�[�Q�b�g�̕����ɐݒ�
    //        Vector3 toTarget = (targetTransform.position - transform.position).normalized;

    //        myPlayerTransform.forward = new Vector3( toTarget.x,0f, toTarget.z ).normalized; // �v���C���[�̌������^�[�Q�b�g�����ɐݒ�(PlayerAvatar��������ق����悳����)

    //        homingMoveDirection = toTarget; // ���݂̈ړ��������X�V
    //        isHoming = true; // �z�[�~���O���J�n
    //        StartCoroutine(HomingCoroutine()); // �z�[�~���O���Ԃ̊Ǘ����J�n

    //        myPlayerAvatar.isFollowingCameraForward = false; // �J�����̌����ɒǏ]���Ȃ��悤�ɐݒ�
    //    }
    //}


    //void Homing( Vector3 origin ,Transform targetTransform )
    //{


    //    Vector3 toTarget = (targetTransform.position - origin);
    //    toTarget.y = 0f; // Y���̐������[���ɂ��Đ����ʏ�̕������擾
    //    toTarget.Normalize(); // ���K�����ĕ����x�N�g���ɂ���

    //    // ���̈ړ���������G�ւ̕����Ƃ̍����p�x�Ōv�Z
    //    float angleToTarget = Vector3.Angle(homingMoveDirection, toTarget);

    //    // �p�x��������ꍇ�͕␳
    //    if (angleToTarget > 0f)
    //    {
    //        // ��]�p�̐����i�ő�maxTurnAnglePerFrame�x�j
    //        float t = Mathf.Min(1f, maxTurnAnglePerFrame / angleToTarget);
    //        homingMoveDirection = Vector3.Slerp(homingMoveDirection, toTarget, t);
    //    }



    //}

    ////�z�[�~���O���Ԃ̊Ǘ�
    //IEnumerator HomingCoroutine()
    //{
    //    yield return new WaitForSeconds(homingTime);

    //    EndHoming(); // �z�[�~���O�I������
    //}


    //void EndHoming()
    //{
    //    isHoming = false; // �z�[�~���O���I��
    //    myPlayerAvatar.isFollowingCameraForward = true; // �J�����̌����ɒǏ]����悤�ɐݒ�

    //}




    public void StartAttack()
    {
       

    }

    //public void EndAttack()
    //{
    //    isAttackActive = false; // �U��������I��
    //    StartCoroutine(PostAttackDelayCoroutine()); // �U����̍d�����Ԃ̊Ǘ����J�n
    //}




    ////�U�����肪�o�鎞�Ԃ̊Ǘ�
    //IEnumerator AttakingCoroutine()
    //{
    //    yield return new WaitForSeconds(attackActiveTime);
    //    isHoming = false; // �z�[�~���O���I��
    //    EndAttack(); // �z�[�~���O�I������
    //}


    ////�U����̍d�����Ԃ̊Ǘ�
    //IEnumerator PostAttackDelayCoroutine()
    //{
    //    yield return new WaitForSeconds(postAttackDelayTime);
    //    isHoming = false; // �z�[�~���O���I��
    //    EndHoming(); // �z�[�~���O�I������
    //}

    //public void NotHomingAttack()
    //{
    //    // �z�[�~���O���Ă��Ȃ��ꍇ�̍U������
    //}



    //public  bool TryGetClosestTargetInRange(Vector3 origin, Vector3 forward, float range, float fovAngle, out Transform targetTransform)
    //{


    //    Collider[] hits = Physics.OverlapSphere(origin, range, playerLayer); // �v���C���[�̃��C���[�}�X�N���g�p���ċ߂��̓G�����o
    //    float closestDistance = Mathf.Infinity;
    //    targetTransform = null;
    //    float minDistance = Mathf.Infinity;




    //    foreach (Collider hit in hits)
    //    {
    //        Transform enemyTransform = hit.transform;
    //        Transform enemyHeadTransform = enemyTransform.GetComponent<PlayerAvatar>().headObject.transform;

    //        Vector3 toEnemy = (enemyTransform.position - origin);
    //        float angle = Vector3.Angle(forward, toEnemy.normalized); // �K�v�ɉ����� forward ���v���C���[�� forward �ɕύX
    //        float distance = toEnemy.magnitude;



    //        if (angle > fovAngle / 2f || distance > range)
    //        {
    //            continue;
    //        }


    //        // Raycast�����`�F�b�N
    //        Vector3 from = myHeadTransform.position;
    //        Vector3 to = enemyHeadTransform.position;
    //        Vector3 direction = (to - from).normalized;
    //        float rayDistance = Vector3.Distance(from, to);

    //        if (Physics.Raycast(from, direction, out RaycastHit hitInfo, rayDistance, obstructionMask))
    //        {
    //            // Ray���r���ŉ����ɓ������Ă����王�����ʂ��Ă��Ȃ�
    //            continue;
    //        }

    //        if (distance < closestDistance)
    //        {
    //            closestDistance = distance;
    //            targetTransform = enemyTransform;
    //        }


    //    }

    //    return targetTransform != null;
    //}






}
