using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshProを使用する場合は、TextMeshProパッケージが必要です
public class NotificationMessage : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText; // TextMeshProのテキストコンポーネントを使用する場合
    private bool isHost = false; // ホストかどうかのフラグ

    public void SetText(string message, bool state)
    {
        isHost = state; // ホストフラグを設定
        if (messageText != null)
        {
            messageText.text = message;
        }
        else
        {
            Debug.LogWarning("MessageText is not assigned in NotificationMessage.");
        }

        //ホストの場合はテキストを黄色に変更
        if (isHost)
        {
            messageText.color = Color.yellow; // ホストのメッセージは黄色
        }
        else
        {
            messageText.color = Color.white; // 通常のメッセージは白
        }
    }
}
