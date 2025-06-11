using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

// リスポーン管理クラス
public class RespawnManager : NetworkBehaviour
{
    // シングルトンインスタンス
    public static RespawnManager Instance { get; private set; }

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
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
        InitializePlayerInHost(playerObject);
    }

    private void InitializePlayerInHost(NetworkObject playerObject)
    {
        //ホストのみ実行
        if(!Object.HasStateAuthority) return;        

        //HPの初期化・無敵化
        var playerState = playerObject.GetComponent<PlayerNetworkState>();
        if (playerState != null)
        {
            Debug.Log($"InitializePlayerInHost： {playerObject.InputAuthority} HP initialized.");
            playerState.SetInvincible(true); 
            playerState.InitializeHP();
        }
        
        //RPCを呼び出して、クライアント側でもリスポーン処理を実行
        RPC_InitializePlayerInAll(playerObject);

    }

    //クライアント側のリスポーン処理
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_InitializePlayerInAll(NetworkObject playerObject)
    {
        Debug.Log($"InitializePlayerInAll: Player {playerObject.InputAuthority} Local Initialized");

        if (playerObject != null && playerObject.IsValid)
        {
            // プレイヤーのアニメーションをアイドル状態に
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
    

    //ホスト側のリスポーン終了時の処理
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RespawnEnd(NetworkObject playerObject)
    {
        if (playerObject == null || !playerObject.IsValid)
        {
            Debug.LogWarning("RPC_RespawnEnd: Invalid player object.");
            return;
        }
        Debug.Log($"RPC_RespawnEnd: Player {playerObject.InputAuthority} respawn finished.");
        // プレイヤーの無敵状態を解除
        var playerState = playerObject.GetComponent<PlayerNetworkState>();
        if (playerState != null)
        {
            playerState.SetInvincible(false);
        }
    }
}
