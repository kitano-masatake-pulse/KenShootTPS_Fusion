using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerInputData
{
    public Vector3 wasdInput; //���K�����Ă��Ȃ��ړ�����
    public bool JumpPressedDown;
    public bool FirePressedDown;
    public bool FirePressedStay;
    public bool FirePressedUp; 
    public bool ReloadPressedDown; //�����[�h�{�^���������ꂽ���ǂ���
    public float weaponChangeScroll;
    public bool ADSPressedDown; //ADS�{�^���������ꂽ���ǂ���
}

public struct UIInputData
{
    public bool ScoreBoardPressdDown;
    public bool ScoreBoardPressdUp;
}

public class CameraInputData
{
    public Vector2  mouseMovement ;
    public bool cursorLockButton;
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
        input.ADSPressedDown = Input.GetMouseButtonDown(1); // �E�N���b�N��ADS

        return input;
    }

    public static UIInputData CollectUIInput()
    {
        UIInputData input;
        input.ScoreBoardPressdDown = Input.GetKeyDown(KeyCode.Tab);
        input.ScoreBoardPressdUp = Input.GetKeyUp(KeyCode.Tab);
        return input;
    }
    public static CameraInputData CollectCameraInput()
    {
        CameraInputData input = new CameraInputData(); // Initialize the variable to avoid CS0165
        input.mouseMovement = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        input.cursorLockButton = Input.GetKeyDown(KeyCode.Escape); // ���N���b�N�ŃJ�[�\�����b�N
        return input;
    }


}