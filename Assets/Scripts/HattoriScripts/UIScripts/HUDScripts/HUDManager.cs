using UnityEngine;

public class HUDManager : MonoBehaviour
{
    [Header("ŠeHUDƒpƒlƒ‹")]
    [SerializeField] private HPPanel hp;
    [SerializeField] private ScorePanel score;
    [SerializeField] private WeaponPanel weapon;
    [SerializeField] private TimerPanel timer;
    [SerializeField] private RespawnPanel resp;

    private PlayerNetworkState playerState;
    private WeaponLocalState weaponState;

    private void OnEnable()
    {
        PlayerNetworkState.OnLocalPlayerSpawned += PlayerHUDInitialize;
        WeaponLocalState.OnWeaponSpawned += WeaponHUDInitialize;
        TimerInitialize();
    }
    private void OnDisable()
    {
        PlayerNetworkState.OnLocalPlayerSpawned -= PlayerHUDInitialize;
        WeaponLocalState.OnWeaponSpawned -= WeaponHUDInitialize;
        CleanupAll();
    }

    private void PlayerHUDInitialize(PlayerNetworkState pState)
    {
        hp.Initialize(pState, null);
        score.Initialize(pState, null);
        resp.Initialize(pState, null);
    }

    private void WeaponHUDInitialize(WeaponLocalState wState)
    {
        weapon.Initialize(null, wState);
    }

    private void TimerInitialize()
    {
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
