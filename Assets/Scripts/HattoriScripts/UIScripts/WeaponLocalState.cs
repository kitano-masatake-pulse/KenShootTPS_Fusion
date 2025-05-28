using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.Playables;        

/// <summary>
/// 武器ごとのローカル弾薬管理と操作（発射・リロード・リザーブ補填）を行うコンポーネント
/// </summary>
public class WeaponLocalState : NetworkBehaviour
{
    [Header("武器リストと初期弾数設定")]
    [Tooltip("利用可能な武器の一覧（WeaponType enum）")]
    [SerializeField]
    private WeaponType[] availableWeapons = new[] {
        WeaponType.Sword,
        WeaponType.AssaultRifle,
        WeaponType.SemiAutoRifle,
        WeaponType.GrenadeLauncher
    };
    [Header("武器ごとのマガジン最大数")]
    [Tooltip("武器ごとに対応するマガジン最大弾数（配列長は availableWeapons と同じ）")]
    [SerializeField]
    private int[] magazineCapacities = new[] {
        0,    // Sword
        20,   // AssaultRifle
        5,    // SemiAutoRifle
        1     // GrenadeLauncher
    };
    [Header("武器ごとのリザーブ最大数")]
    [Tooltip("武器ごとに対応するリザーブ最大弾数（配列長は availableWeapons と同じ）")]
    [SerializeField]
    private int[] reserveCapacities = new[] {
        0,    // Sword
        100,  // AssaultRifle
        15,   // SemiAutoRifle
        5     // GrenadeLauncher
    };

    [Header("リロード設定")]
    [SerializeField] private float reloadDuration = 2.0f;

    [Header("依存コンポーネント")]
    [SerializeField] private PlayerNetworkState playerState;

    // 内部: 武器ごとの弾薬情報
    private struct AmmoData { public int Magazine; public int Reserve; }
    private Dictionary<WeaponType, AmmoData> ammoTable;

    // 現在選択中の武器
    private WeaponType currentWeapon;
    public WeaponType CurrentWeapon => currentWeapon;

    // クラス生成・弾薬データ変更時のイベント
    public static event Action<WeaponLocalState> OnWeaponSpawned;
    public event Action<int, int> OnAmmoChanged;
    public event Action<WeaponType> OnWeaponChanged;

    // リロード中フラグ
    private bool isReloading;

    public override void Spawned()
    {

        //自分以外はこのコンポーネントを使わない
        if (!HasInputAuthority) return;
         
        // テーブル初期化
        ammoTable = new Dictionary<WeaponType, AmmoData>();
        for (int i = 0; i < availableWeapons.Length; i++)
        {
            var w = availableWeapons[i];
            var magCap = magazineCapacities[i];
            var resCap = reserveCapacities[i];
            ammoTable[w] = new AmmoData { Magazine = magCap, Reserve = resCap };
        }
        // 初期武器セット
        if (availableWeapons.Length > 0)
            ChangeWeapon(availableWeapons[0]);

        OnWeaponSpawned?.Invoke(this);


    }

    /// <summary>
    /// 現在の弾薬データを取得
    /// </summary>
    public (int magazine, int reserve) GetCurrentAmmo()
    {
        var ad = ammoTable[currentWeapon];
        return (ad.Magazine, ad.Reserve);
    }

    /// <summary>
    /// 任意の武器の弾薬データを取得
    /// </summary>
    public (int magazine, int reserve) GetAmmo(WeaponType weapon)
    {
        if (ammoTable.TryGetValue(weapon, out var ad))
            return (ad.Magazine, ad.Reserve);
        throw new ArgumentException($"未設定の武器です: {weapon}");
    }

    /// <summary>
    /// 武器を切り替える
    /// </summary>
    public void ChangeWeapon(WeaponType newWeapon)
    {
        //自分以外はこのコンポーネントを使わない
        if (!HasInputAuthority) return;

        if (isReloading) return; // リロード中は切り替え不可
        if (!ammoTable.ContainsKey(newWeapon))
            throw new ArgumentException($"未設定の武器です: {newWeapon}");

        currentWeapon = newWeapon;
        // 武器変更イベント通知
        OnWeaponChanged?.Invoke(currentWeapon);
        // 弾薬表示更新
        var ad = ammoTable[currentWeapon];
        // 現在の武器の弾薬情報を通知
        OnAmmoChanged?.Invoke( ad.Magazine, ad.Reserve);

        // RPCでホストに切り替えをリクエスト
        //    ※HasInputAuthority を持つクライアントからのみ呼ぶ
        if (playerState != null && playerState.HasInputAuthority)
        {
            Debug.Log($"RPC呼び出し");
            playerState.RPC_RequestWeaponChange(currentWeapon);
        }else 
        {
            Debug.Log($"RPC呼び出せず");
        }
    }

    /// <summary>
    /// 発射できるかチェックし、可能ならマガジン1発分減算
    /// </summary>
    public bool TryFire()
    {
        //自分以外はこのコンポーネントを使わない
        if (!HasInputAuthority) return false; 

        if (isReloading) return false;
        var ad = ammoTable[currentWeapon];
        //武器種が Sword の場合はマガジンを減らさない
        if (currentWeapon == WeaponType.Sword) return true;
        // マガジンが空なら発射不可
        if (ad.Magazine <= 0) return false;
        // 発射処理（ここでは単純にマガジンを減らす）
        ad.Magazine--;
        ammoTable[currentWeapon] = ad;
        OnAmmoChanged?.Invoke(ad.Magazine, ad.Reserve);
        return true;
    }

    /// <summary>
    /// リロードを開始（コルーチン）
    /// </summary>
    public void StartReload()
    {
        //自分以外はこのコンポーネントを使わない
        if (!HasInputAuthority) return;

        if (isReloading) return;
        var ad = ammoTable[currentWeapon];
        if (ad.Reserve <= 0 || ad.Magazine >= magazineCapacities[Array.IndexOf(availableWeapons, currentWeapon)])
            return; // リロード不要またはリザーブ無し

        StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        yield return new WaitForSeconds(reloadDuration);

        var idx = Array.IndexOf(availableWeapons, currentWeapon);
        var cap = magazineCapacities[idx];
        var ad = ammoTable[currentWeapon];

        int needed = cap - ad.Magazine;
        int toReload = Mathf.Min(needed, ad.Reserve);

        ad.Magazine += toReload;
        ad.Reserve -= toReload;
        ammoTable[currentWeapon] = ad;
        isReloading = false;

        OnAmmoChanged?.Invoke(ad.Magazine, ad.Reserve);
    }

    /// <summary>
    /// リザーブ弾を追加
    /// </summary>
    /// 弾が追加されると、リザーブが最大値を超えないように調整されるが、どれだけ入ったかに関わらず、入れようと試みた弾は消滅する
    public void AddReserve(int amount)
    {
        //自分以外はこのコンポーネントを使わない
        if (!HasInputAuthority) return;

        // 武器種が Sword の場合はリザーブを増やさない
        if (currentWeapon == WeaponType.Sword) return;
        var ad = ammoTable[currentWeapon];

        int maxReserve = reserveCapacities[Array.IndexOf(availableWeapons, currentWeapon)];
        //リザーブが最大であれば追加しない
        if (ad.Reserve >= maxReserve) return;
        // 追加量が最大値を超える場合は調整
        if (ad.Reserve + amount > maxReserve)
        {
            amount = maxReserve - ad.Reserve; // 最大値を超えないように調整
        }

        ad.Reserve += amount;
        ammoTable[currentWeapon] = ad;
        OnAmmoChanged?.Invoke(ad.Magazine, ad.Reserve);
    }

    
}

