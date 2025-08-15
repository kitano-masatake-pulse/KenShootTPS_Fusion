using Fusion;
using UnityEngine;

[RequireComponent(typeof(NetworkTransform))]
public class TeleportPositionSync : NetworkBehaviour
{
    [SerializeField, Tooltip("移動速度")]

    private NetworkTransform netTransform;

    public override void Spawned()
    {
        netTransform = GetComponent<NetworkTransform>();

        // クライアントから入力を送る
        if (Object.HasInputAuthority)
            Runner.ProvideInput = true;
    }

    void Update()
    {
        if (!Object.HasInputAuthority) return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        transform.Translate(new Vector3(h, 0, v) * Time.deltaTime * 3f);
    }

    public override void FixedUpdateNetwork()
    {
        //// まずホスト側で物理的に位置を更新
        //if (Object.HasStateAuthority)
        //{
        //    // ここでホストが「補間なし」のテレポート同期を実行
        //    netTransform.TeleportToPosition(transform.position);
        //}
    }
}
