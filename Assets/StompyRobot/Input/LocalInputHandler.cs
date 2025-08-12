using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerInputData
{
    public Vector3 wasdInput; //正規化していない移動入力
    public bool JumpPressedDown;
    public bool FirePressedDown;
    public bool FirePressedStay;
    public bool FirePressedUp; 
    public bool ReloadPressedDown; //リロードボタンが押されたかどうか
    public float weaponChangeScroll;
    public bool ADSPressedDown; //ADSボタンが押されたかどうか
    public static PlayerInputData Default()
    {
        PlayerInputData input;
        input.wasdInput = Vector3.zero;
        input.JumpPressedDown = false;
        input.FirePressedDown = false;
        input.FirePressedStay = false;
        input.FirePressedUp = false;
        input.ReloadPressedDown = false;
        input.weaponChangeScroll = 0f;
        input.ADSPressedDown = false;
        return input;
    }
}

public struct UIInputData
{
    public bool ToggleMenu;
    public bool ScoreBoardPressedDown;
    public bool ScoreBoardPressedUp;
    public bool StartPressedDown; 
    public static UIInputData Default()
    {
        UIInputData input;
        input.ToggleMenu = false;
        input.ScoreBoardPressedDown = false;
        input.ScoreBoardPressedUp = false;
        input.StartPressedDown = false;
        return input;
    }
}

public struct CameraInputData
{
    public Vector2  mouseMovement ;
    public bool cursorLockButton;
    public static CameraInputData Default()
    {
        CameraInputData input;
        input.mouseMovement = Vector2.zero;
        input.cursorLockButton = false;
        return input;
    }
}

public struct DebugInputData
{
    public bool BattleEndPressedDown;
    public bool SuicidePressedDown;
    public static DebugInputData Default()
    {
        DebugInputData input;
        input.BattleEndPressedDown = false;
        input.SuicidePressedDown = false;
        return input;
    }
}


// This class is responsible for handling local player input.
public static class LocalInputHandler 
{
    public static bool isOpenMenu;

    public static PlayerInputData CollectInput()
    {
        PlayerInputData input;
        input.wasdInput = new Vector3(Input.GetAxis("Horizontal"), 0f,Input.GetAxis("Vertical"));
        input.JumpPressedDown = Input.GetKeyDown(KeyCode.Space);
        input.FirePressedDown = Input.GetMouseButtonDown(0);
        input.FirePressedStay = Input.GetMouseButton(0);
        input.FirePressedUp = Input.GetMouseButtonUp(0);
        input.ReloadPressedDown = Input.GetKeyDown(KeyCode.R);
        input.weaponChangeScroll = Input.GetAxis("Mouse ScrollWheel");
        input.ADSPressedDown = Input.GetMouseButtonDown(1); // 右クリックでADS

        return input;
    }

    public static UIInputData CollectUIInput()
    {
        UIInputData input;
        input.ToggleMenu = Input.GetKeyDown(KeyCode.Escape); 
        input.ScoreBoardPressedDown = Input.GetKeyDown(KeyCode.Tab);
        input.ScoreBoardPressedUp = Input.GetKeyUp(KeyCode.Tab);
        input.StartPressedDown = Input.GetKeyDown(KeyCode.Return); 
        return input;
    }
    public static CameraInputData CollectCameraInput()
    {
        CameraInputData input = new CameraInputData(); // Initialize the variable to avoid CS0165
        input.mouseMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        input.cursorLockButton = Input.GetKeyDown(KeyCode.Escape); // 中クリックでカーソルロック
        return input;
    }
    public static DebugInputData CollectDebugInput()
    {
        DebugInputData input;
        input.BattleEndPressedDown = Input.GetKeyDown(KeyCode.N); 
        input.SuicidePressedDown = Input.GetKeyDown(KeyCode.K); 
        return input;
    }

}