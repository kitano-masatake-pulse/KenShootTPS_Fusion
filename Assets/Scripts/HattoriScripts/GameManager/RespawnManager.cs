using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class RespawnManager : NetworkBehaviour
{
    //シングルトン
    public static RespawnManager Instance { get; private set; }

    private void Awake()
    {
        // シングルトンのインスタンスを設定
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // シーンを跨いでオブジェクトを保持
        }
        else
        {
            Destroy(gameObject); // 既に存在する場合は新しいインスタンスを破棄
        }
    }

    //ホストにリスポーン要求
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestRespawn(NetworkObject playerObject)
    {
        // リスポーン要求を受け取ったプレイヤーのオブジェクトが有効か確認
        if (playerObject == null || !playerObject.IsValid)
        {
            Debug.LogWarning("RPC_RequestRespawn: Invalid player object.");
            return;
        }
        Debug.Log($"RPC_RequestRespawn: Player {playerObject.InputAuthority} requested respawn.");
        // リスポーン処理を実行
        RespawnPlayer(playerObject);
    }
    //プレイヤーのリスポーン処理
    private void RespawnPlayer(NetworkObject playerObject)
    {
        //ホストのみ
        if(!Object.HasStateAuthority)
        {
            return;
        }
        if (playerObject != null && playerObject.IsValid)
        {
            // プレイヤーの位置をリスポーン地点に設定
            var playerAvatar = playerObject.GetComponent<PlayerAvatar>();
            if (playerAvatar != null)
            {
                Debug.Log($"RespawnPlayer： {playerObject.InputAuthority} at initial spawn point.");
                playerAvatar.TeleportToInitialSpawnPoint(GetRespawnPoint());
                //フラグのリセット???ちょっと後でちゃんと見るべき
                playerAvatar.IsHoming = false;
                playerAvatar.SetFollowingCameraForward(true);
            }

            //HPの初期化
            var playerState = playerObject.GetComponent<PlayerNetworkState>();
            if (playerState != null)
            {
                Debug.Log($"RespawnPlayer： {playerObject.InputAuthority} HP initialized.");
                playerState.SetInvincible(true); // 無敵状態にする
                playerState.InitializeHP();
            }

        }

        //RPCを呼び出して、クライアント側でもリスポーン処理を実行
        RPC_RespawnPlayerClient(playerObject);

    }

    //クライアント側のリスポーン処理
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_RespawnPlayerClient(NetworkObject playerObject)
    {
        Debug.Log($"RPC_RespawnPlayerClient: Player {playerObject.InputAuthority} Local Initialized");
        // クライアント側でのリスポーン処理
        if (playerObject != null && playerObject.IsValid)
        {
            // プレイヤーのアニメーションをリセット
            var animator = playerObject.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play("Idle");
            }
        }
        //Collider 有効化(レイヤー切り替え)
        foreach (var col in playerObject.GetComponentsInChildren<Collider>())
            col.gameObject.layer = LayerMask.NameToLayer("Player");
        //Hitbox 有効化(レイヤー切り替え)
        foreach (var hitbox in playerObject.GetComponentsInChildren<PlayerHitbox>())
            hitbox.gameObject.layer = LayerMask.NameToLayer("PlayerHitbox");
        var target = playerObject.GetComponentInChildren<PlayerWorldUIController>(true);
        if (target != null)
        {
            target.gameObject.SetActive(true); // ネームタグを再表示
        }
    }
    //リスポーン地点の取得
    public Vector3 GetRespawnPoint()
    {
        var randomValue = UnityEngine.Random.insideUnitCircle * 5f;
        var spawnPosition = new Vector3(randomValue.x, 5f, randomValue.y);
        return spawnPosition;
    }
}
