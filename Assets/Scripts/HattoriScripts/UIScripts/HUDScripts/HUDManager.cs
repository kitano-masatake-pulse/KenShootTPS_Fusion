using UnityEngine;

public class HUDManager : MonoBehaviour
{
    [Header("�eHUD�p�l��")]
    [SerializeField] private HPPanel hp;
    [SerializeField] private ScorePanel score;
    [SerializeField] private WeaponPanel weapon;
    [SerializeField] private TimerPanel timer;
    [SerializeField] private ADSCrossHair adsCrossHair; // ADS�p�̃N���X�w�A�p�l��


    private void OnEnable()
    {   
        PlayerNetworkState.OnLocalPlayerSpawned += PlayerHUDInitialize;
        PlayerAvatar.OnWeaponSpawned += WeaponHUDInitialize;
        GameManager.OnManagerInitialized += GameHUDInitialize;
    }
    private void OnDisable()
    {
        PlayerNetworkState.OnLocalPlayerSpawned -= PlayerHUDInitialize;
        PlayerAvatar.OnWeaponSpawned -= WeaponHUDInitialize;
        GameManager.OnManagerInitialized -= GameHUDInitialize;
        CleanupAll();
    }

    private void PlayerHUDInitialize(PlayerNetworkState pState)
    {
        hp.Initialize(pState, null);

    }

    private void WeaponHUDInitialize(PlayerAvatar wState)
    {
        weapon.Initialize(null, wState);
        adsCrossHair.Initialize(null, wState); // ADS�N���X�w�A�̏�����
    }

    private void GameHUDInitialize()
    {
        // GameManager�����������ꂽ��ɌĂяo�����
        Debug.Log("GameHUDInitialize called");
        score.Initialize(null, null);
        timer.Initialize(null, null);
        
    }

    private void CleanupAll()
    {
        hp.Cleanup();
        score.Cleanup();
        weapon.Cleanup();
        timer.Cleanup();
        adsCrossHair.Cleanup(); // ADS�N���X�w�A�̃N���[���A�b�v



    }
}
