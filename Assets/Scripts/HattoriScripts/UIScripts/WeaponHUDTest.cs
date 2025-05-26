using UnityEngine;

/// <summary>
/// LocalWeaponManager と HUDManager の動作テスト用コンポーネント
/// キー操作で武器変更、発射、リロード、リザーブ補填を試せます。
/// </summary>
public class WeaponHUDTest : MonoBehaviour
{
    [Header("参照コンポーネント（Inspector で設定可能）")]
    [SerializeField] private WeaponLocalState weaponManager;
    [SerializeField] private HUDManager hudManager;

    [Header("リザーブ補填テスト量")]
    [SerializeField] private int reserveAmount = 5;

    private void Start()
    {
        // シーン内にあれば自動取得
        if (weaponManager == null)
            weaponManager = FindObjectOfType<WeaponLocalState>();
        if (hudManager == null)
            hudManager = FindObjectOfType<HUDManager>();

        if (weaponManager == null || hudManager == null)
            Debug.LogError("WeaponHUDTest: LocalWeaponManager または HUDManager が見つかりません");
    }

    private void Update()
    {
        // 1/2/3 キーで武器切替
        if (Input.GetKeyDown(KeyCode.Alpha1))
            weaponManager.ChangeWeapon(WeaponType.AssaultRifle);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            weaponManager.ChangeWeapon(WeaponType.SemiAutoRifle);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            weaponManager.ChangeWeapon(WeaponType.GrenadeLauncher);
        if (Input.GetKeyDown(KeyCode.Alpha0))
            weaponManager.ChangeWeapon(WeaponType.Sword);

        // F キーで発射テスト
        if (Input.GetKeyDown(KeyCode.F))
            Debug.Log("TryFire returned: " + weaponManager.TryFire());

        // R キーでリロードテスト
        if (Input.GetKeyDown(KeyCode.R))
            weaponManager.StartReload();

        // T キーでリザーブ補填テスト
        if (Input.GetKeyDown(KeyCode.T))
            weaponManager.AddReserve(reserveAmount);
    }
}

