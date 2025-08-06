using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// リスポーン管理クラス
public class RespawnManager : NetworkBehaviour
{
    // シングルトンインスタンス
    public static RespawnManager Instance { get; private set; }

    [SerializeField]
    private List<Vector3> respawnPoints =
    new List<Vector3>{
        new Vector3(-9, 2, -93), // リスポーン地点1
        new Vector3(94, 5, 14), // リスポーン地点2
        new Vector3(27, 0, 91), // リスポーン地点3
        new Vector3(-97, 5, 7.5f), // リスポーン地点4
        new Vector3(-40, 5, -55), // リスポーン地点5
        new Vector3(49, 5, 18), // リスポーン地点6
        new Vector3(-20, 3, 64), // リスポーン地点7
        new Vector3(57.5f, 9.5f, -44), // リスポーン地点8
    }; // リスポーン地点のリスト
    [SerializeField]
    private float mapScale = 1.0f;

    private HashSet<int> reservedSpawnIndices = new HashSet<int>();

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

    public override void FixedUpdateNetwork()
    {
        //ハッシュセットのリセット
        reservedSpawnIndices.Clear();
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

        Vector3 respawnPoint = GetRespawnPoint(playerObject.InputAuthority);

        

        //HPの初期化・無敵化
        var playerState = playerObject.GetComponent<PlayerNetworkState>();
        if (playerState != null)
        {
            Debug.Log($"InitializePlayerInHost： {playerObject.InputAuthority} HP initialized.");
            playerState.SetInvincible(true); 
            playerState.InitializeHP();
        }
        
        //RPCを呼び出して、クライアント側でもリスポーン処理を実行
        RPC_InitializePlayerInAll(playerObject, respawnPoint);

    }

    //クライアント側のリスポーン処理
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_InitializePlayerInAll(NetworkObject playerObject, Vector3 respawnPoint)
    {
        Debug.Log($"InitializePlayerInAll: Player {playerObject.InputAuthority} Local Initialized");

        var playerAvatar = playerObject.GetComponent<PlayerAvatar>();
        if (playerAvatar != null)
        {
            playerAvatar.TeleportToInitialSpawnPoint(respawnPoint);
        }

        if (playerObject != null && playerObject.IsValid)
        {
            playerAvatar.SetActionAnimationPlayList(ActionType.Respawn,Runner.SimulationTime);
            Debug.Log($"RPC_InitializePlayerInAll: Player {playerObject.InputAuthority} action animation set to Respawn.");

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
        // プレイヤーの無敵状態を3秒後に解除
        var playerState = playerObject.GetComponent<PlayerNetworkState>();
        if (playerState != null)
        {
            playerState.SetInvincible(true, 5.0f);
        }
    }

    // リスポーン地点を取得するメソッド
    private Vector3 GetRespawnPoint(PlayerRef respawnPlayer)
    {
        if(!Runner.IsServer)
        {
            Debug.LogError("GetRespawnPoint: This method should only be called on the server.");
            return Vector3.zero; // サーバーでない場合はデフォルトの位置を返す
        }
        // 他のプレイヤーの位置を取得
        List<Vector3> otherPlayerPositions = GetOtherPlayerPositions(respawnPlayer);
        if (otherPlayerPositions.Count == 0)
        {
            Debug.LogWarning("No other player positions found. Using default respawn point.");
            // 他のプレイヤーがいない場合はランダムなスポーン地点を使用
            return respawnPoints[Random.Range(0, respawnPoints.Count-1)] * mapScale;
        }
        //各スポーン地点のスコア計算
        List<(Vector3 point, float score)> scoredSpawnPoints = new();

        var scaledRespawnPoints = respawnPoints.Select(p => p * mapScale).ToList();

        foreach (var point in scaledRespawnPoints)
        {
            float closestSqrDistance = float.MaxValue;
            foreach (var otherPos in otherPlayerPositions)
            {
                // 各スポーン地点と他のプレイヤーの距離を計算
                float sqrDistance = (point - otherPos).sqrMagnitude;
                if (sqrDistance < closestSqrDistance)
                {
                    closestSqrDistance = sqrDistance;
                }
            }
            scoredSpawnPoints.Add((point, closestSqrDistance));
        }

        //距離が最も遠い順にソート
        scoredSpawnPoints.Sort((a, b) => b.score.CompareTo(a.score));
        
        //予約されていない地点を探す
        int chosenIndex = FindAvailableSpawnIndex(scoredSpawnPoints.Select(p => p.point).ToList());
        
        return scoredSpawnPoints[chosenIndex].point;
    }

    private List<Vector3> GetOtherPlayerPositions(PlayerRef respawnPlayer)
    {
        List<Vector3> positions = new List<Vector3>();
        foreach (var player in Runner.ActivePlayers)
        {
            if(player != respawnPlayer)
            {
                if(Runner.TryGetPlayerObject(player, out NetworkObject playerObject) && playerObject != null)
                {
                    var playerTransform = playerObject.transform;
                    if (playerTransform != null)
                    {
                        positions.Add(playerTransform.position);
                        Debug.Log($"Found player position: {playerTransform.position} for player {player}");
                    }
                }
            } 
        }

        return positions;
    }

    //予約スポーン地点の管理
    private int FindAvailableSpawnIndex(List<Vector3> spawnPoints)
    {
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (!reservedSpawnIndices.Contains(i))
            {
                reservedSpawnIndices.Add(i);
                return i;
            }
        }

        //空いてない場合は最初の地点を返す
        return 0;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestTeleportSpawnPoint(PlayerRef spawnPlayer)
    {
        // スポーン地点を取得
        Vector3 respawnPoint = GetRespawnPoint(spawnPlayer);
        RPC_TeleportSpawnPoint(spawnPlayer, respawnPoint);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_TeleportSpawnPoint(PlayerRef spawnPlayer, Vector3 respawnPoint)
    {
        Debug.Log($"RPC_TeleportSpawnPoint: Local = Player?:{Runner.LocalPlayer == spawnPlayer}");
        Debug.Log($"RPC_TeleportSpawnPoint: Try Get ? : {Runner.TryGetPlayerObject(spawnPlayer, out NetworkObject o)}");

        if (Runner.LocalPlayer == spawnPlayer &&
            Runner.TryGetPlayerObject(spawnPlayer, out NetworkObject playerObject) && 
            playerObject != null)
        {
            var playerAvatar = playerObject.GetComponent<PlayerAvatar>();
            if (playerAvatar != null)
            {
                playerAvatar.TeleportToInitialSpawnPoint(respawnPoint);
            }
            else
            {
                Debug.LogWarning($"RPC_TeleportSpawnPoint: PlayerAvatar component not found for player {spawnPlayer}.");
            }
        }
        else
        {
                       Debug.LogWarning($"RPC_TeleportSpawnPoint: Player {spawnPlayer} not found or does not have input authority.");
        }
    }



}
