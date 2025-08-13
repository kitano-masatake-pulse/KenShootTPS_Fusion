using UnityEngine;
using Fusion;
using System;

public abstract class WeaponBase : NetworkBehaviour
{
    //protected WeaponLocalState localState;
    protected abstract WeaponType weapon { get; }

    public PlayerAvatar playerAvatar;


    public WeaponType weaponType => weapon;

    //public TPSCameraController playerCamera; // ������TPS�J����
    public float fireDistance = 100f;
    public abstract LayerMask PlayerLayer { get; }
    public abstract LayerMask ObstructionLayer { get; }

    public int currentMagazine;
    public int currentReserve;

    [SerializeField] protected GameObject LineOfFirePrefab; // �ː��̃v���n�u


    public override void Spawned()
    {
        playerAvatar = GetComponentInParent<PlayerAvatar>(); //�e��PlayerAvatar���擾
    }


    public void InitializeAmmo()
    {
        currentMagazine = weaponType.MagazineCapacity();
        currentReserve = weaponType.ReserveCapacity();
        Debug.Log($"Weapon {weaponType.GetName()} initialized with Magazine: {currentMagazine}, Reserve: {currentReserve}");
    }


    public virtual void CalledOnUpdate(PlayerInputData localInputData, InputBufferStruct inputBuffer , WeaponActionState currentAction)
    {
        //Debug.Log("CalledOnUpdate() called"); //�f�o�b�O�p���O�o��
        //UpdateSpreadGauge(-liftingConvergenceRate * Time.deltaTime, -randomConvergenceRate * Time.deltaTime); //�e�̊g�U������������
    }

    public virtual bool CanReload(PlayerInputData localInputData, InputBufferStruct inputBuffer, WeaponActionState currentAction)
    {
        bool inputCondition =  weapon.IsReloadable() && inputBuffer.reload;

        bool stateCondition =
            currentAction == WeaponActionState.Idle; // ���݂̃A�N�V�������A�C�h����Ԃł��邱�Ƃ��m�F

        bool bulletCondition = currentMagazine < weaponType.MagazineCapacity() && currentReserve > 0;   


        //Debug.Log($"CanReload() called. Current Magazine: {currentMagazine}, Current Reserve: {currentReserve}");
        return inputCondition && stateCondition && bulletCondition;
    }

    public virtual bool CanFire(PlayerInputData localInputData, InputBufferStruct inputBuffer, WeaponActionState currentAction)
    {
        return false; //�f�t�H���g�ł͔��˂ł��Ȃ�

    }

    public virtual bool CanChangeWeapon(PlayerInputData localInputData, InputBufferStruct inputBuffer, WeaponActionState currentAction)
    {
        bool inputCondition = localInputData.weaponChangeScroll != 0f; // �X�N���[���z�C�[���̓��͂����邩�ǂ������m�F

        bool stateCondition = 
            currentAction != WeaponActionState.Reloading||
            currentAction != WeaponActionState.Stun; 

        //Debug.Log($"CanChangeWeapon() called. Current Action: {currentAction}");
        return currentAction == WeaponActionState.Idle; // ���݂̃A�N�V�������A�C�h����Ԃł��邱�Ƃ��m�F
    }

    public virtual void FireDown()
    {
      
        Debug.Log($"{weaponType.GetName()} fired down! Current Magazine: {currentMagazine}, Current Reserve: {currentReserve}");
    }

    public virtual void FinishReload()
    {

        
        int currentMagazine = this.currentMagazine;
        int currentReserve = this.currentReserve;
        int magazineCapacity = weaponType.MagazineCapacity();
        int reloadededAmmo = Mathf.Min(currentReserve, magazineCapacity - currentMagazine); //�����[�h�����e��

        this.currentMagazine += reloadededAmmo; //�}�K�W���Ƀ����[�h���ꂽ�e���ǉ�
        this.currentReserve -= reloadededAmmo; //���U�[�u���烊���[�h���ꂽ�e������炷

        playerAvatar.InvokeAmmoChanged();
    }



    public virtual void Fire()
    {
        //FireRay();
        //localState.ConsumeAmmo(weaponType);

    
        Debug.Log($"{weaponType.GetName()} fired! Current Magazine: {currentMagazine}, Current Reserve: {currentReserve}");
    }

    public virtual void FireUp()
    {
        Debug.Log($"{weaponType.GetName()} fired up! Current Magazine: {currentMagazine}, Current Reserve: {currentReserve}");
    }


    public virtual bool IsMagazineEmpty()
    {
        return currentMagazine <= 0;
    }


    public virtual bool IsMagazineFull()
    {
        return currentMagazine >=  weaponType.MagazineCapacity() ;
    }



    public virtual void CauseDamage(LagCompensatedHit hit, int weaponDamage)
    {

        //���������Ώۂ�PlayerHitbox�������Ă�����_���[�W����
        if (hit.Hitbox is PlayerHitbox playerHitbox)
        {
            PlayerRef targetPlayerRef = playerHitbox.hitPlayerRef;
            PlayerRef myPlayerRef = Object.InputAuthority;
            Debug.Log($"Player {myPlayerRef} hit Player {targetPlayerRef} with {weaponDamage} damage");
            PlayerHP targetHP = playerHitbox.GetComponentInParent<PlayerHP>();
            targetHP.RPC_RequestDamage(myPlayerRef, weaponDamage);
        }
        else

        {
            Debug.Log($"Couldn't Get playerHitbox, but{hit.Hitbox} ");
        }

    }


    protected virtual void OnEmptyAmmo()
    {
        Debug.Log("�J�`�b�i�e�؂�SE�j");
    }

    public virtual void SetADS(bool ADSflag)
    {
       
    }


    public virtual void ResetOnChangeWeapon()
    {


    }


    public void GenerateLineOfFireGorAllClients(Vector3 startPoint, Vector3 EndPoint)
    {

        GenerateLineOfFire(startPoint, EndPoint); // LineOfFire�𐶐�
        //�S�N���C�A���g��LineOfFire�𐶐�����RPC�𑗐M
        RPC_RequestLineOfFire(startPoint, EndPoint);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_RequestLineOfFire(Vector3 startPoint, Vector3 EndPoint, RpcInfo rpcInfo = default)
    {
        RPC_ApplyLineOfFire(startPoint, EndPoint, rpcInfo.Source); // LineOfFire�𐶐�����RPC���Ăяo��
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_ApplyLineOfFire(Vector3 startPoint, Vector3 EndPoint, PlayerRef shooter, RpcInfo rpcInfo = default)
    {
        if (shooter != Runner.LocalPlayer)
        { 
        
        GenerateLineOfFire(startPoint, EndPoint); // LineOfFire�𐶐�
        }
    }


    public void GenerateLineOfFire(Vector3 startPoint,Vector3  EndPoint)
    { 
        GameObject LineOfFireInstance =  Instantiate(LineOfFirePrefab, startPoint, Quaternion.identity); // LineOfFire�̃C���X�^���X�𐶐�

        LineOfFireInstance.GetComponent<LineOfFire>().SetLinePoints(startPoint, EndPoint); // LineOfFire�̎n�_�ƏI�_��ݒ�

    }

    
}
