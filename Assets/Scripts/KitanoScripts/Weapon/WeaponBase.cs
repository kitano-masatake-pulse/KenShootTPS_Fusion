using UnityEngine;
using Fusion;
using System;

public abstract class WeaponBase : NetworkBehaviour
{
    //protected WeaponLocalState localState;
    protected abstract WeaponType weapon { get; }

    public PlayerAvatar playerAvatar;


    public WeaponType weaponType => weapon;

    //public TPSCameraController playerCamera; // 自分のTPSカメラ
    public float fireDistance = 100f;
    public abstract LayerMask PlayerLayer { get; }
    public abstract LayerMask ObstructionLayer { get; }

    public int currentMagazine;
    public int currentReserve;

    [SerializeField] protected GameObject LineOfFirePrefab; // 射線のプレハブ

    [Header("音関係")]
    [SerializeField] private string hitClipKey="Action_Hit_Damage"; // 爆発音のクリップ
    [SerializeField][Range(0f, 1f)] private float hitClipVolume = 1f; // 爆発音の音量


    public override void Spawned()
    {
        playerAvatar = GetComponentInParent<PlayerAvatar>(); //親のPlayerAvatarを取得
    }


    public void InitializeAmmo()
    {
        currentMagazine = weaponType.MagazineCapacity();
        currentReserve = weaponType.ReserveCapacity();
        Debug.Log($"Weapon {weaponType.GetName()} initialized with Magazine: {currentMagazine}, Reserve: {currentReserve}");
    }


    public virtual void CalledOnUpdate(PlayerInputData localInputData, InputBufferStruct inputBuffer , WeaponActionState currentAction)
    {
        //Debug.Log("CalledOnUpdate() called"); //デバッグ用ログ出力
        //UpdateSpreadGauge(-liftingConvergenceRate * Time.deltaTime, -randomConvergenceRate * Time.deltaTime); //弾の拡散を収束させる
    }

    public virtual bool CanReload(PlayerInputData localInputData, InputBufferStruct inputBuffer, WeaponActionState currentAction)
    {
        bool inputCondition =  weapon.IsReloadable() && inputBuffer.reload;

        bool stateCondition =
            currentAction == WeaponActionState.Idle; // 現在のアクションがアイドル状態であることを確認

        bool bulletCondition = currentMagazine < weaponType.MagazineCapacity() && currentReserve > 0;   


        //Debug.Log($"CanReload() called. Current Magazine: {currentMagazine}, Current Reserve: {currentReserve}");
        return inputCondition && stateCondition && bulletCondition;
    }

    public virtual bool CanFire(PlayerInputData localInputData, InputBufferStruct inputBuffer, WeaponActionState currentAction)
    {
        return false; //デフォルトでは発射できない

    }

    public virtual bool CanChangeWeapon(PlayerInputData localInputData, InputBufferStruct inputBuffer, WeaponActionState currentAction)
    {
        bool inputCondition = localInputData.weaponChangeScroll != 0f; // スクロールホイールの入力があるかどうかを確認

        bool stateCondition = 
            currentAction != WeaponActionState.Reloading||
            currentAction != WeaponActionState.Stun; 

        //Debug.Log($"CanChangeWeapon() called. Current Action: {currentAction}");
        return currentAction == WeaponActionState.Idle; // 現在のアクションがアイドル状態であることを確認
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
        int reloadededAmmo = Mathf.Min(currentReserve, magazineCapacity - currentMagazine); //リロードされる弾薬数

        this.currentMagazine += reloadededAmmo; //マガジンにリロードされた弾薬を追加
        this.currentReserve -= reloadededAmmo; //リザーブからリロードされた弾薬を減らす

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

      //  Debug.Log($"{weaponType.GetName()} fired up! Current Magazine: {currentMagazine}, Current Reserve: {currentReserve}");

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

        //当たった対象がPlayerHitboxを持っていたらダメージ処理
        if (hit.Hitbox is PlayerHitbox playerHitbox)
        {
            PlayerRef targetPlayerRef = playerHitbox.hitPlayerRef;
            PlayerRef myPlayerRef = Object.InputAuthority;
            Debug.Log($"Player {myPlayerRef} hit Player {targetPlayerRef} with {weaponDamage} damage");
            PlayerHP targetHP = playerHitbox.GetComponentInParent<PlayerHP>();
            targetHP.RPC_RequestDamage(myPlayerRef, weaponDamage);

            PlayHitSE(); //ヒット音を再生
        }
        else

        {
            Debug.Log($"Couldn't Get playerHitbox, but{hit.Hitbox} ");
        }

    }


    protected virtual void OnEmptyAmmo()
    {
        Debug.Log("カチッ（弾切れSE）");
    }

    public virtual void SetADS(bool ADSflag)
    {
       
    }

    void PlayHitSE()
    {
        SoundHandle SEHandle = AudioManager.Instance.PlaySound(hitClipKey, SoundCategory.Action);
        AudioManager.Instance.SetSoundVolume(SEHandle, hitClipVolume); // 音量を設定


    }


    public virtual void ResetOnChangeWeapon()
    {


    }


    public void GenerateLineOfFireGorAllClients(Vector3 startPoint, Vector3 EndPoint)
    {

        GenerateLineOfFire(startPoint, EndPoint); // LineOfFireを生成
        //全クライアントにLineOfFireを生成するRPCを送信
        RPC_RequestLineOfFire(startPoint, EndPoint);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_RequestLineOfFire(Vector3 startPoint, Vector3 EndPoint, RpcInfo rpcInfo = default)
    {
        RPC_ApplyLineOfFire(startPoint, EndPoint, rpcInfo.Source); // LineOfFireを生成するRPCを呼び出す
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All, HostMode = RpcHostMode.SourceIsHostPlayer, TickAligned = false)]
    public void RPC_ApplyLineOfFire(Vector3 startPoint, Vector3 EndPoint, PlayerRef shooter, RpcInfo rpcInfo = default)
    {
        if (shooter != Runner.LocalPlayer)
        { 
        
        GenerateLineOfFire(startPoint, EndPoint); // LineOfFireを生成
        }
    }


    public void GenerateLineOfFire(Vector3 startPoint,Vector3  EndPoint)
    { 
        GameObject LineOfFireInstance =  Instantiate(LineOfFirePrefab, startPoint, Quaternion.identity); // LineOfFireのインスタンスを生成

        LineOfFireInstance.GetComponent<LineOfFire>().SetLinePoints(startPoint, EndPoint); // LineOfFireの始点と終点を設定

    }

    
}
