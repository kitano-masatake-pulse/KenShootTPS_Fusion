using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTester : MonoBehaviour
{
    SoundHandle _handle;
    [SerializeField] private SceneType nextSceneType = SceneType.HattoriTitle;
    public void OnClickSEButton()
    {
        AudioManager.Instance.SetSoundVolume(_handle, 0.2f);
    }

    public void OnClickBGMButton()
    {
            _handle = AudioManager.Instance.PlaySound("jingle_25", SoundCategory.BGM);
    }

    public void OnClickBGMButton1()
    {
        
            _handle = AudioManager.Instance.PlaySound("bgm_cyber13", SoundCategory.System);
            //AudioManager.Instance.FadeSound(_handle, 0.1f, 10f);

    }


    public void OnClickNextButton()
    {
        AudioManager.Instance.StopAll();
        SceneTransitionManager.Instance.ChangeScene(nextSceneType);
    }
}
