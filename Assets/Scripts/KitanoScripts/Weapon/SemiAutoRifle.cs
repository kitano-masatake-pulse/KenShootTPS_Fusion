using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SemiAutoRifle : WeaponBase
{

     protected override WeaponType weapon=> WeaponType.SemiAutoRifle; // ����̎�ނ��w��

    private Transform bulletFiringTransform; // Raycast�̌����̈ʒu���w�肷��Transform�BADS�̗L���ɂ���ĕς��

    [SerializeField] private Transform muzzleTransform; //weapon�ɂ�����e���B�����ߎ��ɂ͂������猂��
    public Transform TPSCameraTransform;// TPS�J������Transform�BPlayerAvatar����ݒ肳���BADS���ɂ͂������猂��

    [SerializeField] private LayerMask playerLayer;
    public override LayerMask PlayerLayer => playerLayer;

    [SerializeField] private LayerMask obstructionLayer;
    public override LayerMask ObstructionLayer => obstructionLayer;

    float rayDrawingDuration = 1 / 60f; // Ray�̕`�掞��(1/60�b)

    CharacterController avatarCharacterController; // �L�����N�^�[�R���g���[���[���擾���邽�߂̕ϐ�

    bool isADS = false;
    float reloadTimer = 0f; // �����[�h�^�C�}�[
    float reloadWaitTime = 1.5f; // �����[�h�ɂ����鎞��(�b)
    bool isWaitingForReload = false; // �����[�h�ҋ@�����ǂ����̃t���O

    #region �e�̊g�U(�X�v���b�h)
    //�e�̊g�U�Ɋւ���p�����[�^
    [SerializeField] float liftingSpreadGauge = 0f;
    [SerializeField] float randomSpreadGauge = 0f; // Spread�̌v�Z�Ɏg���A�ˎ��Ԃ�~�ς���ϐ��A�ő��1

    float liftingSpreadRate = 0.1f; // Spread�̊g�U���x(spreadGauge��1�����Ƃ����瑝���邩)
    float randomSpreadRate = 0.1f; // Spread�̊g�U���x(spreadGauge��1�����Ƃ����瑝���邩)
    float liftingConvergenceRate = 0.3f; // Spread�̎������x(preadGauge���b�Ԃ����猸�邩)
    float randomConvergenceRate = 0.3f; // Spread�̎������x(preadGauge���b�Ԃ����猸�邩)

    //spraedGauge,moveSpeed�ɑ΂���g�U�̌��E�l
    [SerializeField] float liftingSpreadLimit = 1f;
    [SerializeField] float randomSpreadLimit = 1f;
    [SerializeField] float runninngSpreadLimit = 1f;


    //spraedGauge,,moveSpeed�ɑ΂���g�U�̔{��(degree/gauge)([x,y]=[pitch,yaw])
    [SerializeField] Vector2 liftingSpreadMultiplier = new Vector2(0, 0f);
    [SerializeField] Vector2 randomSpreadMultiplier = new Vector2(0, 0);
    [SerializeField] Vector2 runninngSpreadMultiplier = new Vector2(3f, 6f);

    //�X�v���b�h�p�^�[�������߂邽�߂̃V�[�h�l
    int seed_RandomSpreadRadius = 11510; //�����_���X�v���b�h�̔��a�̃V�[�h�l
    int seed_RandomSpreadAngle = 11091; //�����_���X�v���b�h�̊p�x�̃V�[�h�l
    int seed_RunningSpreadRadius = 05971;
    int seed_RunningSpreadAngle = 17116; //�����j���O�X�v���b�h�̊p�x�̃V�[�h�l

    List<float> randomPattern_RandomSpreadRadius = new List<float>(); //�����_���X�v���b�h�̔��a�̃p�^�[��
    List<float> randomPattern_RandomSpreadAngle = new List<float>(); //�����_���X�v���b�h�̊p�x�̃p�^�[��
    List<float> randomPattern_RunningSpreadRadius = new List<float>(); //�����j���O�X�v���b�h�̔��a�̃p�^�[��
    List<float> randomPattern_RunningSpreadAngle = new List<float>(); //�����j���O�X�v���b�h�̊p�x�̃p�^�[��

    [SerializeField] float ADSspreadReduction = 0.8f; //ADS�����ǂ����̃t���O
    int spreadPatternIndex = 0; //�X�v���b�h�̃p�^�[���̃C���f�b�N�X



    #endregion


    [Header("���֌W")]
    [SerializeField] private string  fireClipKey = "Weapon_Fire_SemiAutoRifle"; //�ˌ����̃N���b�v�L�[
    [SerializeField] private float fireClipVolume = 1f; 
    [SerializeField] private float fireClipStartTime = 0f; 

    // Start is called before the first frame update
    public override void Spawned()
    {
        int magazineCapacity = weaponType.MagazineCapacity(); //�}�K�W���̗e�ʂ��擾

        //�����_���p�^�[���𐶐�
        randomPattern_RandomSpreadRadius = GenerateRandomPattern(seed_RandomSpreadRadius, magazineCapacity, 0f, 1f); //�����_���X�v���b�h�̔��a�̃p�^�[���𐶐�
        randomPattern_RandomSpreadAngle = GenerateRandomPattern(seed_RandomSpreadAngle, magazineCapacity, 0f, 2 * Mathf.PI); //�����_���X�v���b�h�̊p�x�̃p�^�[���𐶐�
        randomPattern_RunningSpreadRadius = GenerateRandomPattern(seed_RunningSpreadRadius, magazineCapacity, 0f, 1f); //�����j���O�X�v���b�h�̔��a�̃p�^�[���𐶐�
        randomPattern_RunningSpreadAngle = GenerateRandomPattern(seed_RunningSpreadAngle, magazineCapacity, 0f, 2 * Mathf.PI); //�����j���O�X�v���b�h�̊p�x�̃p�^�[���𐶐�

        avatarCharacterController = GetComponentInParent<CharacterController>(); //�e�̃L�����N�^�[�R���g���[���[���擾

        bulletFiringTransform = isADS ? TPSCameraTransform : muzzleTransform; //ADS��Ԃɉ����Ēe�̔��ˈʒu��؂�ւ�


    }

    public override void CalledOnUpdate(PlayerInputData localInputData, InputBufferStruct inputBuffer, WeaponActionState currentAction)
    {

        //ADS
        if (CanADS(localInputData, inputBuffer, currentAction))
        {
            playerAvatar.SwitchADS();
        }

        if (isWaitingForReload)
        {
            reloadTimer += Time.deltaTime; //�����[�h�^�C�}�[���X�V
            if (reloadTimer >= reloadWaitTime) //�����[�h���Ԃ��o�߂�����
            {
                isWaitingForReload = false; //�����[�h�ҋ@�t���O������
                reloadTimer = 0f; //�����[�h�^�C�}�[�����Z�b�g

                FinishReload(); //�����[�h�����������Ăяo��
                Debug.Log($"Reloaded {weaponType.GetName()}! Current Magazine: {currentMagazine}, Current Reserve: {currentReserve}");
            }



        }

        //�����[�h�����𖞂����Ă��烊���[�h
        if (CanReload(localInputData, inputBuffer, currentAction))
        {
            isWaitingForReload = true; //�����[�h�ҋ@�t���O�𗧂Ă�
            //Reload(); //�����[�h�������Ăяo��
            playerAvatar.Reload();
            Debug.Log($"Reloading {weaponType.GetName()}! Current Magazine: {currentMagazine}, Current Reserve: {currentReserve}");
            return; //�����[�h������ȍ~�̏����͍s��Ȃ�
        }

        //�ˌ������𖞂����Ă�����ˌ�

        else if (CanFire(localInputData, inputBuffer, currentAction)) //�A�˒��Ȃ�
        {
            if (IsMagazineEmpty())
            {
              //�}�K�W������Ȃ�ˌ��ł��Ȃ��̂ŉ������Ȃ�
                Debug.Log($"Cannot fire {weaponType.GetName()}! Magazine is empty. Current Magazine: {currentMagazine}, Current Reserve: {currentReserve}");
                return;
            }
            else 
            {
                //Fire(); 
                playerAvatar.FireAction(); //PlayerAvatar�̎ˌ��������Ăяo��
                Debug.Log($"Firing {weaponType.GetName()} stay! Current Magazine: {currentMagazine}, Current Reserve: {currentReserve}");
                return;

            }
                
        }

    }

    public bool CanADS(PlayerInputData localInputData, InputBufferStruct inputBuffer, WeaponActionState currentAction)
    {
        bool stateCondition =
            currentAction == WeaponActionState.Idle ||
            currentAction == WeaponActionState.Firing;


        return localInputData.ADSPressedDown && stateCondition; 
        //ADS�{�^����������Ă��āA���݂̃A�N�V�������A�C�h����Ԃł��邱�Ƃ��m�F

    }

    public override bool CanFire(PlayerInputData localInputData, InputBufferStruct inputBuffer, WeaponActionState currentAction)
    {
        //���ˉ\���ǂ����𔻒�
        return localInputData.FirePressedDown && currentAction == WeaponActionState.Idle ; 
        //�A�˒����e������ꍇ�ɔ��ˉ\
    }


    public override void Fire()
    {
        base.FireDown();
        currentMagazine--;

        //�ˌ����̍Đ�
        PlayFireSEForAllClients();


        Vector3 spreadDirection =
            SpreadRaycastDirection
            (muzzleTransform.forward,
            liftingSpreadGauge,
            randomSpreadGauge,
            avatarCharacterController.velocity.magnitude,
            spreadPatternIndex,
            isADS); //�ː��̊g�U���v�Z


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


        //�ː��̉���
        Vector3 rayEnd = Vector3.zero;
        rayEnd = origin + direction * fireDistance; // �q�b�g�|�C���g���Ȃ��ꍇ�͌��̒����܂ł�Ray��`��

        if (hit.Point != null && hit.Point != Vector3.zero)
        {
            rayEnd = hit.Point; // �q�b�g�����|�C���g������ꍇ�͂����܂ł�Ray��`��
            Debug.Log("Hit Point: " + rayEnd);
        }
        else
        {
            rayEnd = origin + direction * fireDistance;  // �q�b�g�|�C���g���Ȃ��ꍇ�͌��̒����܂ł�Ray��`��
            Debug.Log("No Hit Point, using fireDistance: " + rayEnd);

        }

        base.GenerateLineOfFireGorAllClients(origin, rayEnd); //���e�ʒu��Ray�𐶐�
        Debug.Log($"GenerateLineOfFire from {origin} to {rayEnd} with hit point: {hit.Point}");



        if (RaycastLinePoolManager.Instance != null)
        {

            RaycastLinePoolManager.Instance.ShowRay(origin, rayEnd, Color.red, rayDrawingDuration);
            Debug.Log("Raycast Line drawn from " + origin + " to " + rayEnd);
        }



        Debug.Log("Hit?" + hit.GameObject);
        //���e���� 
        if (hit.GameObject != null)
        {
            Debug.Log("Hit!" + hit.GameObject);

            if (BulletMarkGenerator.Instance != null)
            {
                BulletMarkGenerator.Instance.GenerateBulletMark(hit.Point); //���e�ʒu�ɒe���𐶐�

            }
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


    private Vector3 SpreadRaycastDirection(Vector3 direction, float liftingSpreadParam, float randomSpreadParam, float moveSpeed, int spreadIndex, bool ADSflag)
    {
        Vector3 spreadDirection = Vector3.zero;




        //���t�e�B���O�X�v���b�h(�A�ˎ��Ԃɂ���ď�����ւԂ��)
        Vector2 liftingSpredDir = Vector2.zero; //���t�e�B���O�X�v���b�h�̃x�N�g��

        liftingSpredDir = Mathf.Clamp(liftingSpreadParam, 0, liftingSpreadLimit) * liftingSpreadMultiplier; //X�������̃X�v���b�h���v�Z

        //�����_���X�v���b�h(�A�ˎ��Ԃɂ���ă����_���ɂԂ��)
        Vector2 randomSpreadDir = Vector2.zero; //�����_���X�v���b�h�̃x�N�g��

        float radius_rand = randomPattern_RandomSpreadRadius[spreadIndex]; //�����_���X�v���b�h�̔��a���擾
        float angle_rand = randomPattern_RandomSpreadAngle[spreadIndex]; //�����_���X�v���b�h�̊p�x���擾

        //���a�Ɗp�x�̗������烉���_���X�v���b�h���v�Z
        randomSpreadDir.x = radius_rand * Mathf.Cos(angle_rand) * randomSpreadGauge * randomSpreadMultiplier.x;
        randomSpreadDir.y = radius_rand * Mathf.Sin(angle_rand) * randomSpreadGauge * randomSpreadMultiplier.y;


        //�����j���O�X�v���b�h(�ړ����x�ɉ����ă����_���ɂԂ��)
        Vector2 runningSpreadDir = Vector2.zero; //�����j���O�X�v���b�h�̃x�N�g��

        float moveSpeedClamp = Mathf.Clamp(moveSpeed, 0, runninngSpreadLimit); //�ړ����x�𐧌�
        float radius_run = randomPattern_RunningSpreadRadius[spreadIndex]; //�����j���O�X�v���b�h�̔��a���擾
        float angle_run = randomPattern_RunningSpreadAngle[spreadIndex]; //�����j���O�X�v���b�h�̊p�x���擾

        runningSpreadDir.x = radius_run * Mathf.Cos(angle_run) * moveSpeedClamp * runninngSpreadMultiplier.x;
        runningSpreadDir.y = radius_run * Mathf.Sin(angle_run) * moveSpeedClamp * runninngSpreadMultiplier.y;

        float ADSmultiplier = ADSflag ? ADSspreadReduction : 1f; //ADS���̓X�v���b�h�𔼕��ɂ���


        //���v���Ďː��̊g�U���v�Z
        Vector2 totalSpreadDir = (liftingSpredDir + randomSpreadDir + runningSpreadDir) * ADSmultiplier; //�����I�ȃX�v���b�h�̃x�N�g��
        Debug.Log($"Total Spread Direction: {totalSpreadDir}"); //�f�o�b�O�p���O�o��

        Quaternion spreadRot = Quaternion.Euler(-totalSpreadDir.x, totalSpreadDir.y, 0f); //�X�v���b�h�̉�]���v�Z(���̖���pitch�͐������])

        spreadDirection = spreadRot * direction; //���̎ː������ɃX�v���b�h�̉�]��K�p

        //����direction�Ƃ̓��ς��Ƃ�
        float dotProduct = Vector3.Dot(direction.normalized, spreadDirection.normalized); //���̎ː������Ƃ̓��ς��v�Z
        Debug.Log($"total Dot Product: {dotProduct}"); //�f�o�b�O�p���O�o��


        return spreadDirection;
    }

    //�X�v���b�h�̃p�^�[���𐶐����郁�\�b�h
    List<float> GenerateRandomPattern(int seed, int count, float min, float max)
    {
        List<float> patternList = new List<float>(); //�p�^�[�����i�[���郊�X�g
        patternList.Clear(); //���X�g���N���A

        System.Random rand = new System.Random(seed); //�V�[�h�l��ݒ�
        for (int i = 0; i < count; i++)
        {
            float randomValue = (float)rand.NextDouble(); //0����1�͈̔͂Ń����_���Ȕ��a�𐶐�
            randomValue = Mathf.Lerp(min, max, randomValue); //�ŏ��l�ƍő�l�̊Ԃŕ��
            patternList.Add(randomValue); //���X�g�ɒǉ�
        }

        return patternList;


    }

    public override void SetADS(bool ADSflag)
    {
        isADS = ADSflag; //ADS��Ԃ��X�V
        Debug.Log($"SemiAutoRifle ADS state changed: {isADS}");
        bulletFiringTransform = isADS ? TPSCameraTransform : muzzleTransform; //ADS��Ԃɉ����Ēe�̔��ˈʒu��؂�ւ�

    }

    public override void ResetOnChangeWeapon()
    {
         reloadTimer = 0f;
        isWaitingForReload = false;
    }

    #region ���֌W
    void PlayFireSEForAllClients()
    {

        if (AudioManager.Instance == null || string.IsNullOrEmpty(fireClipKey)) { return; } // AudioManager�����݂��A�N���b�v�L�[���ݒ肳��Ă���ꍇ

        if (Runner.LocalPlayer==Object.InputAuthority)
        {
            SoundHandle SEHandle = AudioManager.Instance.PlaySound(fireClipKey, SoundCategory.Weapon, fireClipStartTime, fireClipVolume, SoundType.OneShot, this.transform.position);
            RPC_RequestPlayFireSE(); //RPC���Ăяo���đS�N���C�A���g�ɉ����Đ�������
        }
        
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_RequestPlayFireSE(RpcInfo  rpcinfo=default)
    {
        if (AudioManager.Instance == null || string.IsNullOrEmpty(fireClipKey)) { return; } // AudioManager�����݂��A�N���b�v�L�[���ݒ肳��Ă���ꍇ
        RPC_ApplyPlayFireSE(rpcinfo.Source); //RPC���Ăяo���ĉ����Đ�
        Debug.Log("RPC_PlayFireSE called on " + Runner.LocalPlayer);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_ApplyPlayFireSE(PlayerRef sourcePlayer)
    {
        if (Runner.LocalPlayer != sourcePlayer)
        {
            if(AudioManager.Instance == null || string.IsNullOrEmpty(fireClipKey)) { return; } // AudioManager�����݂��A�N���b�v�L�[���ݒ肳��Ă���ꍇ

            SoundHandle SEHandle = 
                AudioManager.Instance.PlaySound
                (fireClipKey, 
                SoundCategory.Weapon, 
                fireClipStartTime, 
                fireClipVolume, 
                SoundType.OneShot, 
                this.transform.position
                );
        }

    }
    #endregion

}
