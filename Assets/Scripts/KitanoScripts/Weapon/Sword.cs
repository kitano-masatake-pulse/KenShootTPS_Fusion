using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Fusion.NetworkCharacterController;
using static UnityEngine.UI.Image;

public class Sword : WeaponBase
{
    protected override WeaponType weapon => WeaponType.Sword; // ����̎�ނ��w��


    [SerializeField] private LayerMask playerLayer;
    public override LayerMask PlayerLayer => playerLayer;

    [SerializeField] private LayerMask obstructionLayer;
    public override LayerMask ObstructionLayer => obstructionLayer;


    [SerializeField] Transform swordRoot;//����̔���̒��S�ʒu
    float swordLength = 1f; // ����̔���̔��a
    float timeUntilAttack= 1f; // ���[�V�����J�n����U������܂ł̎���
    int attackFrameCount = 1; // �U������̃t���[����


    //Raycast�̕������v�Z���邽�߂̕ϐ�
    float cornRayAngleDeg = 30f; // �~���`�̊p�x
    int cornRayNum = 10; // �~���`�̕��ˏ��Ray�̖{��

    private float rayDrawingDuration = 0.5f; // Ray�̕`�掞��


    PlayerAvatar myPlayerAvatar;


    

    [SerializeField] private float attackActiveTime = 1f; // �����蔻�肪�o�鎞��

    
    


    
    private bool isAttackActive = false; // �U�����肪�o�Ă��邩�ǂ���

    private void Start()
    {
        

    }



    // Update is called once per frame
    void Update()
    {

    }


    public override void FireDown()
    {
      
        //base.FireDown();
        StartCoroutine(CollisionDetectionCoroutine()); // �U������̃R���[�`�����J�n
    }


    IEnumerator CollisionDetectionCoroutine()
    {
        Debug.Log("Start Attack Collision Detection Coroutine!");
        yield return new WaitForSeconds(timeUntilAttack); // ���[�V�����J�n����U������܂ł̎��Ԃ�҂�
        isAttackActive = true; // �U�������L���ɂ���
        GenerateCollisionDetection(); // �U������𐶐�
        Debug.Log("Attack Collision Detection Generated!");
        yield return new WaitForSeconds(rayDrawingDuration); // ���[�V�����J�n����U������܂ł̎��Ԃ�҂�
        isAttackActive = false; // �U������𖳌��ɂ���


    }


    // �U������𐶐����郁�\�b�h
    public void GenerateCollisionDetection()
    {
        var hits = new List<LagCompensatedHit>();
        int hitCount=Runner.LagCompensation.OverlapSphere( // �U��������s��
            swordRoot.position, 
            swordLength, 
            Object.InputAuthority, 
            hits, 
            playerLayer,
            //HitOptions.IgnoreInputAuthority
            HitOptions.IgnoreInputAuthority // HitOptions.None���g�p���āA���ׂẴq�b�g���擾����
            );

        if (OverlapSphereVisualizer.Instance != null)
        {
            OverlapSphereVisualizer.Instance.ShowSphere(swordRoot.position, swordLength, rayDrawingDuration, "Sword Attack Area", Color.red); // �U������͈̔͂���������
        }
        else
        {
            Debug.LogWarning("OverlapSphereVisualizer.Instance is null! Please ensure it is set up in the scene.");
        }


        Debug.Log($"OverlapSphere hit count: {hitCount}");



        if (hitCount > 0)
        {
            List<HitboxRoot> damagedHitboxRoots = new List<HitboxRoot>(); // �q�b�g����Hitbox��Root���L�^���郊�X�g
            List<LagCompensatedHit> damagedHits = new List<LagCompensatedHit>(); // ���̃t���[���Ń_���[�W��^����q�b�g�����L�^���郊�X�g


            Debug.Log($"Detected {hitCount} hits!");
            foreach (var hit in hits)
            {



                // ���������Ώۂ�PlayerHitbox�������Ă������Q���̔���
                if (hit.Hitbox is PlayerHitbox playerHitbox)
                {
                    HitboxRoot targetPlayerRoot= playerHitbox.Root;

                    if(damagedHitboxRoots.Contains(targetPlayerRoot)) // ���Ƀ_���[�W��^�����v���C���[�Ȃ�X�L�b�v
                    {
                        Debug.Log($"Already hit player {targetPlayerRoot}, skipping damage.");
                        continue;
                    }

                    if (TryRaycastCornRadial(hit,out LagCompensatedHit raycastHit))// �~���`��Raycast���s��
                    {
                        damagedHitboxRoots.Add(targetPlayerRoot); // �q�b�g�����v���C���[��Root���L�^
                        damagedHits.Add(raycastHit);
                    }
                }
                else
                {
                    Debug.Log("Hit but not a PlayerHitbox: " + hit.GameObject);
                }
            }

            // damagedPlayerWithHit�Ɋ܂܂��v���C���[�Ƀ_���[�W��^����
            Debug.Log($"Damaged players count: {damagedHitboxRoots.Count}");

            foreach (var hit in damagedHits)
            {
                CauseDamage(hit, weaponType.Damage());
            }

            //�_���[�W�p�̃��X�g���N���A
            damagedHitboxRoots.Clear();
            damagedHits.Clear();

        }
        else 
        { 
            Debug.Log("No hits detected!");
        }

        
    }

    // �~���`��Raycast���s��
    bool TryRaycastCornRadial(LagCompensatedHit hit,out LagCompensatedHit raycastHit)
    {
        raycastHit= default; // ������
        Vector3 swordDirection = hit.GameObject.transform.position - swordRoot.position; // ���̕������v�Z(��X�A��e����spine���Q�Ƃ���悤�ɂ���)
        List<Vector3> rayDirections = CornRaycastDirections(swordDirection, cornRayAngleDeg, cornRayNum); // 30�x�̉~���`�̕�����4�{��Ray����ˏ�ɔ�΂�

        foreach (var direction in rayDirections)
        {
            Runner.LagCompensation.Raycast(
           swordRoot.position,
           direction,
           swordLength,   // ���̒����̋����Ń��C�L���X�g
           Object.InputAuthority,
           out LagCompensatedHit hitResult,
           playerLayer | obstructionLayer, // ������s�����C���[�𐧌�����B�v���C���[�Ə�Q���̃��C���[���w��
           HitOptions.IgnoreInputAuthority

            );


            if (RaycastLinePoolManager.Instance != null)
            { 
                Vector3 rayEnd= Vector3.zero;
                
                //rayEnd = swordRoot.position + direction * swordLength; // �q�b�g�|�C���g���Ȃ��ꍇ�͌��̒����܂ł�Ray��`��


                if (hit.Point != null)
                {
                    rayEnd = hit.Point; // �q�b�g�����|�C���g������ꍇ�͂����܂ł�Ray��`��
                }
                else
                {
                    rayEnd = swordRoot.position + direction * swordLength * 5; // �q�b�g�|�C���g���Ȃ��ꍇ�͌��̒����܂ł�Ray��`��

                }

                RaycastLinePoolManager.Instance.ShowRay(swordRoot.position,rayEnd, Color.red,rayDrawingDuration);
            }

            //Debug.DrawRay(swordRoot.position, direction * swordLength, Color.red, rayDrawingDuration);


            // ���C�L���X�g�̌��ʂ��m�F,������h���Ȃ��悤�ɒ���
            //���e���� 

            Debug.Log($"Raycast hitReslut Layer: {hitResult.GameObject.layer}");
            if (hitResult.GameObject != null && ((1<<hitResult.GameObject.layer) &playerLayer) !=0) // �v���C���[�̃��C���[�Ƀq�b�g�������m�F
            {

                Debug.Log("Hit!" + hitResult.GameObject);

                raycastHit=hitResult; // �q�b�g��������Ԃ�

                return true; // �q�b�g�����ꍇ��true��Ԃ�


            }


            Debug.Log($"Raycast direction: {direction}");
        }

        return false; // �ǂ�Ray���q�b�g���Ȃ������ꍇ��false��Ԃ�


    }

    //�Ώۂ̂����������A��΂�Raycast�̕������v�Z���郁�\�b�h
    List<Vector3> CornRaycastDirections(Vector3 axisDirection, float cornAngleDeg, int radialRayNum) // �����ƕ��ˏ��Ray�̖{��(�����̕����ɂ���΂��̂ŁA���v��[radialRayNum+1]�{�ɂȂ�)
    { 
        List<Vector3> directions = new List<Vector3>();


        // ���x�N�g���𐳋K��
        Vector3 axis = axisDirection.normalized;

        // �~���̊J���p�̔����i���W�A���ɕϊ��j
        float theta = cornAngleDeg * 0.5f * Mathf.Deg2Rad;

        // ���Ɛ����ȃx�N�g�������i�C�ӂ�OK�j
        Vector3 ortho = Vector3.Cross(axis, Vector3.up); // �O�ς��v�Z���Ă���
        if (ortho == Vector3.zero) ortho = Vector3.Cross(axis, Vector3.right);
        ortho.Normalize();

        // �~���̕\�ʏ��1�_�i������p�xtheta�ɉ�]�j
        Quaternion tilt = Quaternion.AngleAxis(theta * Mathf.Rad2Deg, Vector3.Cross(axis, ortho));
        Vector3 baseVec = tilt * axis;


        // �ŏ���1�{�͎������ɐݒ�
        directions.Add(axis);
        // ���Ԋu�ŉ�]����[radialRayNum]�쐬
        for (int i = 0; i < radialRayNum; i++)
        {
            float angleAroundAxis = (360f / radialRayNum) * i;
            Quaternion rot = Quaternion.AngleAxis(angleAroundAxis, axis);
            Vector3 e = rot * baseVec;
            directions.Add(e.normalized ); // �K�v�Ȃ璷��a�ɑ�����
        }


        return directions;
    }

    //void OnDrawGizmos()
    //{
    //    if (isAttackActive)
    //    {

    //        // Gizmos���g�p���čU������͈̔͂�����
    //        Gizmos.color = Color.red;
    //        Gizmos.DrawWireSphere(swordRoot.position, swordLength);
   
    //    }
        
    //}




}
