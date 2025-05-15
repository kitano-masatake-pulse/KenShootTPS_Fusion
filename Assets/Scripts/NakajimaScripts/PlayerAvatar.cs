using Fusion;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

public class PlayerAvatar : NetworkBehaviour
{
    [SerializeField]
    private TextMeshPro nameLabel;

    private NetworkCharacterControllerPrototype characterController;

    private void Awake()
    {
        characterController = GetComponent<NetworkCharacterControllerPrototype>();
    }

    public override void Spawned()
    {
        SetNickName($"Player({Object.InputAuthority.PlayerId})"); 
        
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
    }
    private void LateUpdate()
    {
        // プレイヤー名のテキストを、常にカメラの正面向きにする
        nameLabel.transform.rotation = Camera.main.transform.rotation;
    }

    // プレイヤー名をテキストに設定する
    public void SetNickName(string nickName)
    {
        nameLabel.text = nickName;
    }
}