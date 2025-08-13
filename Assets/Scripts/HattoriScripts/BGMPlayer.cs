using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMPlayer : MonoBehaviour
{
    [SerializeField] private string bgmClipkey; 
    [SerializeField] private float bgmVolume = 1f;
    [SerializeField]private bool isLoop = true;

    private void OnEnable()
    {
        SceneTransitionManager.OnSceneLoad -= PlayBGM;
        SceneTransitionManager.OnSceneUnload -= StopBGM;
        SceneTransitionManager.OnSceneLoad += PlayBGM;
        SceneTransitionManager.OnSceneUnload += StopBGM;

    }
    private void OnDisable()
    {
        SceneTransitionManager.OnSceneLoad -= PlayBGM;
        SceneTransitionManager.OnSceneUnload -= StopBGM;
    }
    private void PlayBGM(SceneType sceneType)
    {
        SoundType soundType = isLoop ? SoundType.Loop : SoundType.OneShot;
        if (bgmClipkey != null)
        {
            AudioManager.Instance.PlayBgm(bgmClipkey,bgmVolume,SoundCategory.BGM,soundType);

        }
    }
    private void StopBGM(SceneType sceneType)
    {
        if (bgmClipkey != null)
        {
            AudioManager.Instance.StopBgm();
        }
    }
}
