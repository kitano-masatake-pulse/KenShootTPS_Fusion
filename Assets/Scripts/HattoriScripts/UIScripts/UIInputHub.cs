using UnityEngine;
using UnityEngine.EventSystems;
using System;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class UIInputHub : MonoBehaviour
{
    public static UIInputHub Instance;

    public UIInputData Last { get; private set; }

    public static event System.Action OnToggleMenu;       // イベント購読用
    public static event System.Action OnScoreBoardDown;
    public static event System.Action OnScoreBoardUp;
    public static event System.Action OnStartPressed;

    void Awake()
    {
        if (Instance == null) { Instance = this; this.transform.SetParent(null); DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    void Update()
    {
        var input = LocalInputHandler.CollectUIInput();
        Last = input;
        if (input.ToggleMenu) OnToggleMenu?.Invoke();

        if(LocalInputHandler.isOpenMenu) return; // メニューが開いている場合は下記のUI入力を無視
        
        if (input.ScoreBoardPressedDown) OnScoreBoardDown?.Invoke();
        if (input.ScoreBoardPressedUp) OnScoreBoardUp?.Invoke();
        if (input.StartPressedDown) OnStartPressed?.Invoke();
    }

    
}
