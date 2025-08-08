using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class GrenadeBomb : NetworkBehaviour
{
    WeaponType weaponType=WeaponType.Grenade; // ����̎�ނ�ێ�����ϐ�(WeaponType�X�N���v�g���Q�Ƃ���)


    PlayerRef throwPlayer;

    // Start is called before the first frame update
    [SerializeField] private LayerMask playerLayer;
    public LayerMask PlayerLayer => playerLayer;

    [SerializeField] private LayerMask obstructionLayer;
    public LayerMask ObstructionLayer => obstructionLayer;

    [SerializeField] private float directHitRadius = 1f; // ��������̔��a
    [SerializeField] private float blastHitRadius = 5f; // �����̔��a

    [SerializeField] private float minBlastDamage = 20f; // �����̍ŏ��_���[�W


    [SerializeField] private float damageDuration = 1f; // ��������(�����莞��)
    [SerializeField] private float explosionDelay = 3.5f; // �����܂ł̒x������


    private float rayDrawingDuration = 1f; // Ray�̕`�掞��


    //Raycast�̕������v�Z���邽�߂̕ϐ�
    [SerializeField] float cornRayAngleDeg = 30f; // �~���`�̊p�x
    [SerializeField] int cornRayNum = 10; // �~���`�̕��ˏ��Ray�̖{��



    public override void Spawned()
    {
        //base.Spawned();
        // Initialization code here, if needed

        if (HasStateAuthority)
        {
            StartCoroutine(DamageCoroutine());
        }
    }

    private IEnumerator DamageCoroutine()
    {
        yield return new WaitForSeconds(explosionDelay); // �����܂ł̒x�����Ԃ�҂�

        float elapsed = 0f;
        List<HitboxRoot> alreadyDamagedPlayers = new List<HitboxRoot>(); // ���łɃ_���[�W��^�����v���C���[���L�^���郊�X�g(���̃t���[���Ń_���[�W���m�肵���l���܂�)


        // �����͈͂̕`��
        if (OverlapSphereVisualizer.Instance != null)
        {
            OverlapSphereVisualizer.Instance.ShowSphere(this.transform.position, blastHitRadius, rayDrawingDuration, "Sword Attack Area", Color.blue); // �U������͈̔͂���������
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

        Runner.Despawn(this.Object); // ������ɃI�u�W�F�N�g��j������

    }

    void CollisionDetection(List<HitboxRoot> alreadyDamaged)
    {

        var hits = new List<LagCompensatedHit>();
        Debug.Log($"GrenadeBomb CollisionDetection called. Already damaged count: {alreadyDamaged.Count},Runner:{Runner!=null}");
        int hitCount = Runner.LagCompensation.OverlapSphere( // �U��������s��
            this.transform.position,
            blastHitRadius,
            Object.InputAuthority,
            hits,
            playerLayer,
            //HitOptions.IgnoreInputAuthority
            HitOptions.None // HitOptions.None���g�p���āA���ׂẴq�b�g���擾����
            );



        Debug.Log($"GrenadeBomb OverlapSphere hit count: {hitCount}"); // �q�b�g�������O�ɏo��

        if (hitCount > 0)
        {

            Dictionary<LagCompensatedHit, float> damagedHitsWithDistance = new Dictionary<LagCompensatedHit, float>();//���̃t���[���Ń_���[�W��^����v���C���[�Ƃ��̃q�b�g���
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

                    if (TryRaycastCornRadialAndGetDistance(hit, out LagCompensatedHit raycastHit, out float hitDistance))// �~���`��Raycast���s��
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
        Vector3 explosionDirection = sphereHit.GameObject.transform.position - this.transform.position; // ���̕������v�Z(��X�A��e����spine���Q�Ƃ���悤�ɂ���)
        List<Vector3> rayDirections = CornRaycastDirections(explosionDirection, cornRayAngleDeg, cornRayNum); // 30�x�̉~���`�̕�����4�{��Ray����ˏ�ɔ�΂�

        foreach (var direction in rayDirections)
        {
            Runner.LagCompensation.Raycast(
           this.transform.position,
           direction,
           blastHitRadius,   // �����͈̔͂Ń��C�L���X�g
           Object.InputAuthority,
           out LagCompensatedHit hitResult,
           playerLayer | obstructionLayer, // ������s�����C���[�𐧌�����B�v���C���[�Ə�Q���̃��C���[���w��
           HitOptions.IgnoreInputAuthority

            );


            //Debug.DrawRay(this.transform.position, direction * blastHitRadius, Color.blue, rayDrawingDuration);


            if (RaycastLinePoolManager.Instance != null)
            {
                Vector3 rayEnd = Vector3.zero;

                //rayEnd = this.transform.position + direction * blastHitRadius; // �q�b�g�|�C���g���Ȃ��ꍇ�͌��̒����܂ł�Ray��`��


                if (hitResult.Point != null)
                {
                    rayEnd = hitResult.Point; // �q�b�g�����|�C���g������ꍇ�͂����܂ł�Ray��`��
                }
                else
                {
                    rayEnd = this.transform.position + direction * blastHitRadius; // �q�b�g�|�C���g���Ȃ��ꍇ�͔����͈͂܂ł�Ray��`��

                }

                RaycastLinePoolManager.Instance.ShowRay(this.transform.position, rayEnd, Color.blue, rayDrawingDuration);
            }


            //�ŋߐڂ̒��e�ʒu���X�V 
            if (hitResult.GameObject != null && ((1 << hitResult.GameObject.layer) & playerLayer) != 0 && hitResult.Distance < minHitDistance)
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


    public  void CauseDamage(LagCompensatedHit hit, int weaponDamage)
    {

        //���������Ώۂ�PlayerHitbox�������Ă�����_���[�W����
        if (hit.Hitbox is PlayerHitbox playerHitbox)
        {
            PlayerRef targetPlayerRef = playerHitbox.hitPlayerRef;
            PlayerRef myPlayerRef = Object.InputAuthority;
            Debug.Log($"Player {myPlayerRef} hit Player {targetPlayerRef} with {weaponDamage} damage");
            PlayerHP targetHP = playerHitbox.GetComponentInParent<PlayerHP>();
            Debug.Log($"GrenadeBomb throwPlayer {throwPlayer}");
            Debug.Log($"GrenadeBomb InputAuthority {Object.InputAuthority}");
            targetHP.RPC_RequestDamage(myPlayerRef, weaponDamage);
        }
        else

        {
            Debug.Log($"Couldn't Get playerHitbox, but{hit.Hitbox} ");
        }

    }

    private int GetDamageByDistance(float distance)
    {

        if (distance <= directHitRadius) return weaponType.Damage();
        if (distance >= blastHitRadius) return 0;
        float t = (distance - directHitRadius) / (blastHitRadius - directHitRadius);
        return Mathf.RoundToInt(Mathf.Lerp(weaponType.Damage(), minBlastDamage, t));
    }


    public void SetThrowPlayer(PlayerRef playerRef)
    {
        throwPlayer = playerRef; // �������v���C���[��PlayerRef��ݒ肷��
        Debug.Log($"GrenadeBomb: SetThrowPlayer called with PlayerRef: {playerRef}");
    }




}
