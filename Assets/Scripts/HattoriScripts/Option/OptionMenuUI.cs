using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;

public class OptionMenuUI : MonoBehaviour
{
    [SerializeField] private OptionMenuController optionMenuController; // OptionMenuControllerへの参照
    [Header("UI要素")]
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Toggle invertYToggle;
    [SerializeField] private Toggle invertXToggle;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider systemVolumeSlider;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Slider weaponVolumeSlider;
    [SerializeField] private Slider actionVolumeSlider;
    [SerializeField] private Button applyButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button returnButton;

    [SerializeField] private GameObject QuitDialog;
    [Header("戻り先のシーン")]
    [SerializeField] private SceneType sceneType; // 戻り先のシーンの種類を指定するための変数

    void OnEnable()
    {
        SceneTransitionManager.OnSceneLoad -= ChangeSceneMenu;
        SceneTransitionManager.OnSceneLoad += ChangeSceneMenu;
    }
    void OnDisable()
    {
        SceneTransitionManager.OnSceneLoad -= ChangeSceneMenu;
    }

    private void Start()
    {
        returnButton.gameObject.SetActive(false); // 初期状態では非表示
        QuitDialog.SetActive(false); // QuitDialogを非表示にする
        RefreshUI();
    }

    private void ChangeSceneMenu(SceneType sceneType)
    {
        Debug.Log($"ChangeSceneMenu: {sceneType.ToSceneName()}");
        if (sceneType.ToSceneName().Contains("Title") || sceneType.ToSceneName().Contains("Battle"))
        {
            returnButton.gameObject.SetActive(false); 
        }
        else
        {
            returnButton.gameObject.SetActive(true); 
        }
    }

    //UIの見た目を編集中データと再同期
    public void RefreshUI()
    {
        var s = OptionsManager.Instance.WorkingData;
        mouseSensitivitySlider.SetValueWithoutNotify(s.mouseSensitivity);
        invertYToggle.SetIsOnWithoutNotify(s.invertY);
        invertXToggle.SetIsOnWithoutNotify(s.invertX);
        masterVolumeSlider.SetValueWithoutNotify(s.masterVolume);
        systemVolumeSlider.SetValueWithoutNotify(s.systemVolume);
        bgmVolumeSlider.SetValueWithoutNotify(s.bgmVolume);
        weaponVolumeSlider.SetValueWithoutNotify(s.weaponVolume);
        actionVolumeSlider.SetValueWithoutNotify(s.actionVolume);
    }


    public void OnMouseSensitivitySlider(float value)
    {
        Debug.Log($"MouseSense : {value}");
        var s = OptionsManager.Instance.WorkingData;
        s.mouseSensitivity = value;
        OptionsManager.Instance.WorkingData = s;
    }
    public void OnInvertYToggle(bool on)
    {
        var s = OptionsManager.Instance.WorkingData;
        s.invertY = on;
        OptionsManager.Instance.WorkingData = s;
    }
    public void OnInvertXToggle(bool on)
    {
        var s = OptionsManager.Instance.WorkingData;
        s.invertX = on;
        OptionsManager.Instance.WorkingData = s;
    }
    
    public void OnMasterSlider(float value)
    {
        var s = OptionsManager.Instance.WorkingData;
        s.masterVolume = value;
        OptionsManager.Instance.WorkingData = s;

    }
    public void OnSystemSlider(float value)
    {
        var s = OptionsManager.Instance.WorkingData;
        s.systemVolume = value;
        OptionsManager.Instance.WorkingData = s;
    }
    public void OnBgmSlider(float value)
    {
        var s = OptionsManager.Instance.WorkingData;
        s.bgmVolume = value;
        OptionsManager.Instance.WorkingData = s;
    }
    public void OnWeaponSlider(float value)
    {
        var s = OptionsManager.Instance.WorkingData;
        s.weaponVolume = value;
        OptionsManager.Instance.WorkingData = s;
    }
    public void OnActionSlider(float value)
    {
        var s = OptionsManager.Instance.WorkingData;
        s.actionVolume = value;
        OptionsManager.Instance.WorkingData = s;
    }

    public void OnApplyButton() {OptionsManager.Instance.ApplyAndSave(); RefreshUI();}
    public void OnCancelButton(){OptionsManager.Instance.Cancel(); RefreshUI(); }
    public void OnResetButton(){OptionsManager.Instance.ResetToDefault(); RefreshUI();}

    public void OnReturnButton()
    {
        SceneTransitionManager.Instance.ChangeScene(sceneType, true); // シーンを変更
        optionMenuController.Close(); 
    }

    public void OnShowQuitDialog()
    {
        QuitDialog.SetActive(true); // QuitDialogを表示
    }
    // QuitDialogの表示

    public void OnQuitGameButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // エディタ上での停止
#else

        Application.Quit(); // アプリケーションを終了
#endif
        Debug.Log("Application closed.");
    }
    public void OnCancelQuitDialog()
    {
        QuitDialog.SetActive(false); // QuitDialogを非表示
    }
}
