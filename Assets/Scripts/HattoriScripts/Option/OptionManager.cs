using UnityEngine;
using System;


public class OptionsManager : MonoBehaviour
{
    public static OptionsManager Instance;

    public OptionData CurrentData; //適用中
    public OptionData WorkingData;　//編集中

    public Action<OptionData> OnApplied;


    void Awake()
    {
        if (Instance == null) { Instance = this; this.transform.SetParent(null);　 DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
        Load();
        WorkingData = CurrentData;//初期状態は適用中のデータをコピー
        OnApplied?.Invoke(CurrentData);

    }

    private void OnEnable()
    {
        SceneTransitionManager.OnSceneLoad -= ApplySceneFirst;
        SceneTransitionManager.OnSceneLoad += ApplySceneFirst;

    }
    private void OnDisable()
    {
        SceneTransitionManager.OnSceneLoad -= ApplySceneFirst;
    }

    public void Load()
    {
        var s = OptionData.Default();
        s.mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1.0f);
        s.invertY = PlayerPrefs.GetInt("InvertY", 0) == 1;
        s.invertX = PlayerPrefs.GetInt("InvertX", 0) == 1;
        s.masterVolume = PlayerPrefs.GetFloat("MasterVolume", s.masterVolume);
        s.bgmVolume = PlayerPrefs.GetFloat("BgmVolume", s.bgmVolume);
        s.systemVolume = PlayerPrefs.GetFloat("SystemVolume", s.systemVolume);
        s.weaponVolume = PlayerPrefs.GetFloat("WeaponVolume", s.weaponVolume);
        s.actionVolume = PlayerPrefs.GetFloat("ActionVolume", s.actionVolume);
        CurrentData = s;
    }

    public void ApplyAndSave()
    {
        Debug.Log($"Option Before:{CurrentData.mouseSensitivity}");
        Debug.Log($"Option After:{WorkingData.mouseSensitivity}");
        CurrentData = WorkingData; //編集中のデータを適用中に反映
        PlayerPrefs.SetFloat("MouseSensitivity", CurrentData.mouseSensitivity);
        PlayerPrefs.SetInt("InvertY", CurrentData.invertY ? 1 : 0);
        PlayerPrefs.SetInt("InvertX", CurrentData.invertX ? 1 : 0);
        PlayerPrefs.SetFloat("MasterVolume", CurrentData.masterVolume);
        PlayerPrefs.SetFloat("BgmVolume", CurrentData.bgmVolume);
        PlayerPrefs.SetFloat("SystemVolume", CurrentData.systemVolume);
        PlayerPrefs.SetFloat("WeaponVolume", CurrentData.weaponVolume);
        PlayerPrefs.SetFloat("ActionVolume", CurrentData.actionVolume);
        PlayerPrefs.Save();
        OnApplied?.Invoke(CurrentData); //適用中のデータが変更されたことを通知
    }


    public void Cancel()
    {
        WorkingData = CurrentData; //編集中のデータを適用中に戻す
    }

    public void ResetToDefault()
    {
        WorkingData = OptionData.Default(); //デフォルト値にリセット
    }
    //シーン読み込み時にも現在の設定を適応する
    public void ApplySceneFirst(SceneType sceneType)
    {
        OnApplied?.Invoke(CurrentData);
    }
}
