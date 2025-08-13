using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BattleBGMPlayer : MonoBehaviour
{
    [Header("BGM Settings")]
    [SerializeField] private string bgmClipkey;
    [SerializeField] private float bgmVolume = 1f;
    [SerializeField] private bool isLoop = true;
    [Header("Sound Settings")]
    [SerializeField] private string countdownSoundKey;
    [SerializeField] private float countdownSoundVolume = 1f;
    [SerializeField] private string timeUpSoundKey;
    [SerializeField] private float timeUpSoundVolume = 1f;
    void OnEnable()
    {
        GameManager2.OnManagerInitialized -= SubscribePlaySound;
        GameManager2.OnManagerInitialized += SubscribePlaySound;
    }
    void OnDisable()
    {
        GameManager2.OnManagerInitialized -= SubscribePlaySound;
        if(GameManager2.Instance != null)
        {
            GameManager2.Instance.OnCountDownBattleStart -= PlayCountdownSE;
            GameManager2.Instance.OnTimeUp -= StopBGM;
            GameManager2.Instance.OnTimeUp -= PlayTimeUpSE;
        }
    }
    private void SubscribePlaySound()
    {
        GameManager2.Instance.OnCountDownBattleStart += PlayCountdownSE;
        GameManager2.Instance.OnTimeUp += StopBGM;
        GameManager2.Instance.OnTimeUp += PlayTimeUpSE;
    }

    private void PlayCountdownSE(float countDown)
    {
        if (countdownSoundKey != null)
        {
            AudioManager.Instance.PlaySound(countdownSoundKey,SoundCategory.System, 0f, countdownSoundVolume, SoundType.OneShot);
        }
        StartCoroutine(PlayBGMAfterDelay(countDown));
    }
    private IEnumerator PlayBGMAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayBGM();
    }
    private void PlayTimeUpSE()
    { 
        if (timeUpSoundKey != null)
        {
            AudioManager.Instance.PlaySound(timeUpSoundKey, SoundCategory.System, 0f, timeUpSoundVolume, SoundType.OneShot);
        }
    }

    private void PlayBGM()
    {
        SoundType soundType = isLoop ? SoundType.Loop : SoundType.OneShot;
        if (bgmClipkey != null)
        {
            AudioManager.Instance.PlayBgm(bgmClipkey, bgmVolume, SoundCategory.BGM, soundType);

        }
    }
    private void StopBGM()
    {
        if (bgmClipkey != null)
        {
            AudioManager.Instance.StopBgm();
        }
    }
}
