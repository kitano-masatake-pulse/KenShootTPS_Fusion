using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SemiAutoRifle : WeaponBase
{

     protected override WeaponType weapon=> WeaponType.SemiAutoRifle; // ����̎�ނ��w��

    [SerializeField] private Transform muzzleTransform; // �e��(=Raycast�̌���)�̈ʒu���w�肷��Transform

    [SerializeField] private LayerMask playerLayer;
    public override LayerMask PlayerLayer => playerLayer;

    [SerializeField] private LayerMask obstructionLayer;
    public override LayerMask ObstructionLayer => obstructionLayer;

    float rayDrawingDuration = 1 / 60f; // Ray�̕`�掞��(1/60�b)

    public override void FireDown()
    {
        base.FireDown();

        // �����ɃZ�~�I�[�g���C�t���̓��L�̏�����ǉ�
        GunRaycast(muzzleTransform.position, muzzleTransform.forward);
        Debug.Log($"{weaponType.GetName()} fired down in SemiAutoRifle!");
    }


    void GunRaycast(Vector3 origin, Vector3 direction)
    {
        Runner.LagCompensation.Raycast(
              origin,
              direction,
              fireDistance,
              Object.InputAuthority,
              out var hit,
             playerLayer | obstructionLayer, //������s�����C���[�𐧌�����
              HitOptions.IgnoreInputAuthority);

        if (RaycastLinePoolManager.Instance != null)
        {
            Vector3 rayEnd = Vector3.zero;
            
            //rayEnd = origin + direction * fireDistance; // �q�b�g�|�C���g���Ȃ��ꍇ�͌��̒����܂ł�Ray��`��

            if (hit.Point != null)
            {
                rayEnd = hit.Point; // �q�b�g�����|�C���g������ꍇ�͂����܂ł�Ray��`��
            }
            else
            {
                rayEnd = origin + direction * fireDistance;  // �q�b�g�|�C���g���Ȃ��ꍇ�͌��̒����܂ł�Ray��`��

            }

            RaycastLinePoolManager.Instance.ShowRay(origin, rayEnd, Color.red, rayDrawingDuration);
        }


        Debug.Log("Hit?" + hit.GameObject);
        //���e���� 
        if (hit.GameObject != null)
        {
            Debug.Log("Hit!" + hit.GameObject);

            //���������Ώۂ�PlayerHitbox�������Ă�����_���[�W����
            if (hit.Hitbox is PlayerHitbox playerHitbox)
            {

                CauseDamage(hit, weaponType.Damage());
            }
            else
            {
                Debug.Log("Hit! but not Player");
            }


        }

    }
        // Start is called before the first frame update
        void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
