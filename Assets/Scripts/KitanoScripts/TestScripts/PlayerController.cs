using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private Text idText;

    public override void Spawned()
    {
        // 自分のPlayerRefから確定的なIDを生成（例：1000番台）
        int playerId = 1000 + Object.InputAuthority.RawEncoded;

        // 表示用テキストに反映
        idText.text = $"ID: {playerId:D4}";
    }

    void Update()
    {
        // 名前プレートがカメラの方向を向くように
        idText.transform.rotation = Quaternion.LookRotation(idText.transform.position - Camera.main.transform.position);
    }
}
