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
}


// This class is responsible for handling local player input.
public static class LocalInputHandler 
{
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

        return input;
    }
}
