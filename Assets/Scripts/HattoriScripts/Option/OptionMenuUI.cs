using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionMenuUI : MonoBehaviour
{
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


    private void Start()
    {
        RefreshUI();
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

}
