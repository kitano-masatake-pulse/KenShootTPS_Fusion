using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssaultRifle : WeaponBase
{

    protected override WeaponType weapon => WeaponType.AssaultRifle; // ����̎�ނ��w��

    [SerializeField] private Transform muzzleTransform; // �e��(=Raycast�̌���)�̈ʒu���w�肷��Transform



    [SerializeField] private LayerMask playerLayer;
    public override LayerMask PlayerLayer => playerLayer;

    [SerializeField] private LayerMask obstructionLayer;
    public override LayerMask ObstructionLayer => obstructionLayer;


    float rayDrawingDuration=1/60f; // Ray�̕`�掞��(1/60�b)





    protected override void OnEmptyAmmo()
    {
        Debug.Log("�J�`�b�i�e�؂�SE�j");
    }



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
        GunRaycast(muzzleTransform.position, muzzleTransform.forward);

    }


    public override void Fire()
    { 
        base.Fire();
        GunRaycast(muzzleTransform.position, muzzleTransform.forward);
        // �����ɃA�T���g���C�t�����L�̔��ˏ�����ǉ����邱�Ƃ��ł��܂�
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

       if(RaycastLinePoolManager.Instance != null)
            {
            Vector3 rayEnd = Vector3.zero;
            rayEnd = origin + direction * fireDistance; // �q�b�g�|�C���g���Ȃ��ꍇ�͌��̒����܂ł�Ray��`��


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
}
