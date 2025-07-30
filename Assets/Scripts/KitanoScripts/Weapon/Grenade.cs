using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : WeaponBase
{
    protected override WeaponType weapon => WeaponType.Grenade; // ����̎�ނ��w��


    [SerializeField] private LayerMask playerLayer;
    public override LayerMask PlayerLayer => playerLayer;

    [SerializeField] private LayerMask obstructionLayer;
    public override LayerMask ObstructionLayer => obstructionLayer;


    [SerializeField] private float directHitRadius = 1f;// ��������̔��a
    [SerializeField] private float blastHitRadius = 5f; // �����̔��a

    [SerializeField] private float minBlastDamage = 20f; // �����̍ŏ��_���[�W
    

    [SerializeField] private float damageDuration = 1f; // ��������(�����莞��)
    [SerializeField] private float explosionDelay = 3f; // �����܂ł̒x������

    [SerializeField] private Transform explosionCenter; // �����̒��S�ʒu

    private float rayDrawingDuration=1f; // Ray�̕`�掞��
    bool isAttackActive = false; // �U�����A�N�e�B�u���ǂ����̃t���O

    //Raycast�̕������v�Z���邽�߂̕ϐ�
    [SerializeField]float cornRayAngleDeg = 30f; // �~���`�̊p�x
    [SerializeField]int cornRayNum = 10; // �~���`�̕��ˏ��Ray�̖{��


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }



    public override void FireDown()
    {
        base.FireDown();
        Debug.Log($"{weaponType.GetName()} fired down!");
        // �����܂ł̒x�����Ԃ�҂�
        StartCoroutine(DamageRoutine());
    }

    public override void FireUp()
    {
        
    }


    //��������U������(���e�ɕt����@�\)

    private IEnumerator DamageRoutine()
    {
        //�����ɔ����܂ő҂����


        float elapsed = 0f;
        List<HitboxRoot> alreadyDamagedPlayers = new List<HitboxRoot>(); // ���łɃ_���[�W��^�����v���C���[���L�^���郊�X�g(���̃t���[���Ń_���[�W���m�肵���l���܂�)
        isAttackActive = true; // �U�������L���ɂ���

        // �����͈͂̕`��
        if (OverlapSphereVisualizer.Instance != null)
        {
            OverlapSphereVisualizer.Instance.ShowSphere(explosionCenter.position, blastHitRadius, rayDrawingDuration, "Sword Attack Area", Color.blue); // �U������͈̔͂���������
        }
        else
        {
            Debug.LogWarning("OverlapSphereVisualizer.Instance is null! Please ensure it is set up in the scene.");
        }

        while (elapsed < damageDuration)
        {
            CollisionDetection(alreadyDamagedPlayers);

            elapsed += Time.deltaTime;
            yield return null;

        }
        //�_���[�W�p�̃��X�g���N���A
        alreadyDamagedPlayers.Clear();
        isAttackActive = false; // �U������𖳌��ɂ���
    }

    void CollisionDetection(List<HitboxRoot> alreadyDamaged)
    {

        var hits = new List<LagCompensatedHit>();
        int hitCount = Runner.LagCompensation.OverlapSphere( // �U��������s��
            explosionCenter.position,
            blastHitRadius,
            Object.InputAuthority,
            hits,
            playerLayer,
            //HitOptions.IgnoreInputAuthority
            HitOptions.None // HitOptions.None���g�p���āA���ׂẴq�b�g���擾����
            );

        



        if (hitCount > 0)
        {
           
            Dictionary< LagCompensatedHit,float> damagedHitsWithDistance = new Dictionary<LagCompensatedHit, float>();//���̃t���[���Ń_���[�W��^����v���C���[�Ƃ��̃q�b�g���
            foreach (var hit in hits)
            {



                // ���������Ώۂ�PlayerHitbox�������Ă������Q���̔���
                if (hit.Hitbox is PlayerHitbox playerHitbox)
                {
                    HitboxRoot targetPlayerRoot = playerHitbox.Root; 

                    if (alreadyDamaged.Contains(targetPlayerRoot)) // ���Ƀ_���[�W��^�����v���C���[�Ȃ�X�L�b�v
                    {
                        Debug.Log($"Already hit player {targetPlayerRoot}, skipping damage.");
                        continue;
                    }

                    if (TryRaycastCornRadialAndGetDistance(hit,out LagCompensatedHit raycastHit , out float hitDistance))// �~���`��Raycast���s��
                    {
                        alreadyDamaged.Add(targetPlayerRoot); // �q�b�g�����v���C���[��Root���L�^
                        damagedHitsWithDistance[hit] = hitDistance; // �_���[�W��^�����q�b�g���A�q�b�g�������L�^
                    }
                }
                else
                {
                    Debug.Log("Hit but not a PlayerHitbox: " + hit.GameObject);
                }
            }

            foreach (var kv in damagedHitsWithDistance)
            {
                CauseDamage(kv.Key, GetDamageByDistance(kv.Value));
            }

            
            damagedHitsWithDistance.Clear();



        }
        else
        {
            Debug.Log("No hits detected in OverlapSphere!");
        }




    }

    // �~���`��Raycast���s��
    bool TryRaycastCornRadialAndGetDistance(LagCompensatedHit sphereHit, out LagCompensatedHit raycastHit, out float minHitDistance)
    {
        raycastHit = default; // ������
        minHitDistance = float.PositiveInfinity;
        Vector3 explosionDirection = sphereHit.GameObject.transform.position - explosionCenter.position; // ���̕������v�Z(��X�A��e����spine���Q�Ƃ���悤�ɂ���)
        List<Vector3> rayDirections = CornRaycastDirections(explosionDirection, cornRayAngleDeg, cornRayNum); // 30�x�̉~���`�̕�����4�{��Ray����ˏ�ɔ�΂�

        foreach (var direction in rayDirections)
        {
            Runner.LagCompensation.Raycast(
           explosionCenter.position,
           direction,
           blastHitRadius,   // �����͈̔͂Ń��C�L���X�g
           Object.InputAuthority,
           out LagCompensatedHit hitResult,
           playerLayer | obstructionLayer, // ������s�����C���[�𐧌�����B�v���C���[�Ə�Q���̃��C���[���w��
           HitOptions.IgnoreInputAuthority

            );


            //Debug.DrawRay(explosionCenter.position, direction * blastHitRadius, Color.blue, rayDrawingDuration);


            if (RaycastLinePoolManager.Instance != null)
            {
                Vector3 rayEnd = Vector3.zero;
                
                //rayEnd = explosionCenter.position + direction * blastHitRadius; // �q�b�g�|�C���g���Ȃ��ꍇ�͌��̒����܂ł�Ray��`��


                if (hitResult.Point != null)
                {
                    rayEnd = hitResult.Point; // �q�b�g�����|�C���g������ꍇ�͂����܂ł�Ray��`��
                }
                else
                {
                    rayEnd = explosionCenter.position + direction * blastHitRadius; // �q�b�g�|�C���g���Ȃ��ꍇ�͔����͈͂܂ł�Ray��`��

                }

                RaycastLinePoolManager.Instance.ShowRay(explosionCenter.position, rayEnd, Color.blue, rayDrawingDuration);
            }


            //�ŋߐڂ̒��e�ʒu���X�V 
            if (hitResult.GameObject != null && ((1 << hitResult.GameObject.layer) & playerLayer) != 0 && hitResult.Distance <minHitDistance)
            {
                raycastHit = hitResult; // �q�b�g��������Ԃ�
                minHitDistance = hitResult.Distance; // �q�b�g�����������X�V

            }


            Debug.Log($"Raycast direction: {direction}");
        }


        if (minHitDistance <= blastHitRadius)
        { 
            return true; // �����ꂩ��Ray���q�b�g�����ꍇ��true��Ԃ�

        }
        else
        {
            Debug.Log("No hits detected!");
            return false; // �ǂ�Ray���q�b�g���Ȃ������ꍇ��false��Ԃ�
        }

        


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
                directions.Add(e.normalized); // �K�v�Ȃ璷��a�ɑ�����
            }


            return directions;
        }

    private int GetDamageByDistance(float distance)
    {

        if (distance <= directHitRadius) return weaponType.Damage();
        if (distance >= blastHitRadius) return 0;
        float t = (distance - directHitRadius) / (blastHitRadius - directHitRadius);
        return Mathf.RoundToInt(Mathf.Lerp(weaponType.Damage(), minBlastDamage, t));
    }

    //�����܂ōU������(���e�ɕt����@�\)


    //void OnDrawGizmos()
    //{
    //    if (isAttackActive)
    //    {

    //        // Gizmos���g�p���čU������͈̔͂�����
    //        Gizmos.color = Color.blue;
    //        Gizmos.DrawWireSphere( explosionCenter.position, blastHitRadius);

    //    }

    //}

}





