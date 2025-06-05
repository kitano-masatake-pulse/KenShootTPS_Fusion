using UnityEngine;

public class HUDManager : MonoBehaviour
{
    [Header("各HUDパネル")]
    [SerializeField] private HPPanel hp;
    [SerializeField] private ScorePanel score;
    [SerializeField] private WeaponPanel weapon;
    [SerializeField] private TimerPanel timer;
    [SerializeField] private RespawnPanel resp;


    private void OnEnable()
    {   
        PlayerNetworkState.OnLocalPlayerSpawned += PlayerHUDInitialize;
        WeaponLocalState.OnWeaponSpawned += WeaponHUDInitialize;
        GameManager.OnGameManagerSpawned += GameHUDInitialize;
    }
    private void OnDisable()
    {
        PlayerNetworkState.OnLocalPlayerSpawned -= PlayerHUDInitialize;
        WeaponLocalState.OnWeaponSpawned -= WeaponHUDInitialize;
        GameManager.OnGameManagerSpawned -= GameHUDInitialize;
        CleanupAll();
    }

    private void PlayerHUDInitialize(PlayerNetworkState pState)
    {
        hp.Initialize(pState, null);
    }

    private void WeaponHUDInitialize(WeaponLocalState wState)
    {
        weapon.Initialize(null, wState);
    }

    private void GameHUDInitialize()
    {
        // GameManagerが初期化された後に呼び出される
        Debug.Log("GameHUDInitialize called");
        score.Initialize(null, null);
        resp.Initialize(null, null);
        timer.Initialize(null, null);
    }

    private void CleanupAll()
    {
        hp.Cleanup();
        score.Cleanup();
        weapon.Cleanup();
        resp.Cleanup();
        timer.Cleanup();
    }
}
