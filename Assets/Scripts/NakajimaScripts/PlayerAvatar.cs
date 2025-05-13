using Fusion;
using UnityEngine;
using TMPro;

public class PlayerAvatar : NetworkBehaviour
{
    [SerializeField] private TextMeshPro idText;

    private NetworkCharacterControllerPrototype characterController;

    private void Awake()
    {
        characterController = GetComponent<NetworkCharacterControllerPrototype>();
    }

    public override void Spawned()
    {
        // 自分のPlayerRefから確定的なIDを生成（例：1000番台）
        int playerId = 1000 + Object.InputAuthority.RawEncoded;
        // 表示用テキストに反映
        idText.text = $"ID: {playerId:D4}";

    }


    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            // 入力方向のベクトルを正規化する
            data.Direction.Normalize();
            // 入力方向を移動方向としてそのまま渡す
            characterController.Move(data.Direction);
        }
        idText.transform.rotation = Camera.main.transform.rotation;

    }
}