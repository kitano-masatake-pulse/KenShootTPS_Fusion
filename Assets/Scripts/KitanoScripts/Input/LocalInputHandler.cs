using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerInputData
{
    public Vector3 wasdInput; //³‹K‰»‚µ‚Ä‚¢‚È‚¢ˆÚ“®“ü—Í
    public bool JumpPressed;
    public bool FirePressedDown;
    public bool FirePressedStay;
    public float weaponChangeScroll;
}


// This class is responsible for handling local player input.
public static  class LocalInputHandler 
{
    public static PlayerInputData CollectInput()
    {
        PlayerInputData input;
        input.wasdInput = new Vector3(Input.GetAxis("Horizontal"), 0f,Input.GetAxis("Vertical"));
        input.JumpPressed = Input.GetKeyDown(KeyCode.Space);
        input.FirePressedDown = Input.GetMouseButtonDown(0);
        input.FirePressedStay = Input.GetMouseButton(0);
        input.weaponChangeScroll = Input.GetAxis("Mouse ScrollWheel");

        return input;
    }
}
