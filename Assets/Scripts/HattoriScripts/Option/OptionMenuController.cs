using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionMenuController : MonoBehaviour
{
    [SerializeField] GameObject rootPanel; // メニューのルート（Canvas内のパネル）
    [SerializeField] OptionMenuUI optionMenuUI; // オプションメニューのUIコンポーネント
    public bool IsOpen => rootPanel.activeSelf;

    private void OnEnable()
    {
        UIInputHub.OnToggleMenu -= Toggle; // メニューのトグルイベントを購読解除
        UIInputHub.OnToggleMenu += Toggle; // メニューのトグルイベントを購読
    }
    private void OnDisable()
    {
        UIInputHub.OnToggleMenu -= Toggle; // メニューのトグルイベントを購読解除
    }

    public void Toggle() { if (IsOpen) Close(); else Open(); }

    public void Open()
    {
        rootPanel.SetActive(true);
        CursorManager.Instance.RequestUI(this);
        LocalInputHandler.isOpenMenu = true; // メニューが開いている状態を設定
    }

    public void Close()
    {
        rootPanel.SetActive(false);
        OptionsManager.Instance.Cancel(); 
        optionMenuUI.RefreshUI();
        CursorManager.Instance.ReleaseUI(this);
        LocalInputHandler.isOpenMenu = false; // メニューが閉じている状態を設定

    }
}
