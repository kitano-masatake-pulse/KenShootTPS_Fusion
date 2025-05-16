using UnityEngine;

public class DebugConsole : MonoBehaviour
{
    private string log = "";
    private Vector2 scrollPosition;

    private GUIStyle textStyle;
    private GUIStyle textAreaStyle;

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;

       

        
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string message, string stackTrace, LogType type)
    {
        log += message + "\n";
        if (log.Length > 10000)
        {
            log = log.Substring(log.Length - 10000); // ��������ꍇ�͐؂�
        }

        // �����X�N���[��
        scrollPosition.y = float.MaxValue;
    }

    void OnGUI()
    {


        if (textStyle == null)
        {
            textStyle = new GUIStyle(GUI.skin.label);
            textStyle.fontSize = 14;
            textStyle.normal.textColor = Color.black;
        }

        if (textAreaStyle == null)
        {
            textAreaStyle = new GUIStyle(GUI.skin.box);
            textAreaStyle.normal.background = Texture2D.blackTexture;
        }

        Rect outerRect = new Rect(10, 10, 700, 400); // �\���G���A

        // �\���ɕK�v�ȍ����������v�Z
        float contentHeight = textStyle.CalcHeight(new GUIContent(log), outerRect.width - 20);
        Rect innerRect = new Rect(0, 0, outerRect.width - 20, contentHeight);

        scrollPosition = GUI.BeginScrollView(outerRect, scrollPosition, innerRect, false, true);
        GUI.Label(innerRect, log, textStyle);
        GUI.EndScrollView();
    }
}
