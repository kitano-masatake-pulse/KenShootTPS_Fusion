using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// リスポーン管理クラス
public class RespawnManager : NetworkBehaviour
{
    // シングルトンインスタンス
    public static RespawnManager Instance { get; private set; }
    [Serializable]
    private struct RespawnPoint
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public RespawnPoint(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }
    }
    [SerializeField]
    private List<RespawnPoint> respawnPoints =
    new List<RespawnPoint>
    {
        new RespawnPoint(new Vector3(46, 5, -29), Quaternion.Euler(0, 90, 0)), // リスポーン地点1
        new RespawnPoint(new Vector3(94, 5, 14), Quaternion.Euler(0, 180, 0)), // リスポーン地点2
        new RespawnPoint(new Vector3(27, 0, 91), Quaternion.Euler(0, 90, 0)), // リスポーン地点3
        new RespawnPoint(new Vector3(3, 0, 25), Quaternion.Euler(0, -90, 0)), // リスポーン地点4
        new RespawnPoint(new Vector3(50, 4, 23), Quaternion.Euler(0, -90, 0)), // リスポーン地点5
        new RespawnPoint(new Vector3(25, 1, 67), Quaternion.Euler(0, -135, 0)), // リスポーン地点6
        new RespawnPoint(new Vector3(-26, 4, -15), Quaternion.Euler(0, 45, 0)), // リスポーン地点7
        new RespawnPoint(new Vector3(6, 4, -35), Quaternion.Euler(0, 30, 0)), // リスポーン地点8
        

    };


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

        RespawnPoint respawnPoint = GetRespawnPoint(playerObject.InputAuthority);

        

        //HPの初期化・無敵化
        var playerState = playerObject.GetComponent<PlayerNetworkState>();
        if (playerState != null)
        {
            Debug.Log($"InitializePlayerInHost： {playerObject.InputAuthority} HP initialized.");
            playerState.SetInvincible(true); 
            playerState.InitializeHP();
        }
        
        //RPCを呼び出して、クライアント側でもリスポーン処理を実行
        RPC_InitializePlayerInAll(playerObject, respawnPoint.Position, respawnPoint.Rotation);

    }

    //クライアント側のリスポーン処理
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_InitializePlayerInAll(NetworkObject playerObject,Vector3 respawnPosition,Quaternion respawnRotation)
    {
        Debug.Log($"InitializePlayerInAll: Player {playerObject.InputAuthority} Local Initialized");

        var playerAvatar = playerObject.GetComponent<PlayerAvatar>();
        if (playerAvatar != null)
        {
            playerAvatar.HideMesh(); // メッシュを非表示にする
            playerAvatar.TeleportToInitialSpawnPoint(respawnPosition, respawnRotation);
            playerAvatar.ShowMesh(); // メッシュを表示する

            //内部的に無理やりcurrrentWeaponを変更する
            playerAvatar.ForceWeaponChange(WeaponType.AssaultRifle);
    


            }

        if (playerObject != null)
        {


            playerAvatar.SetActionAnimationPlayList(ActionType.Respawn,Runner.SimulationTime);
            Debug.Log($"RPC_InitializePlayerInAll: Player {playerObject.InputAuthority} action animation set to Respawn.");

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
        if (playerObject == null)
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
    private RespawnPoint GetRespawnPoint(PlayerRef respawnPlayer)
    {
        if(!Runner.IsServer)
        {
            Debug.LogError("GetRespawnPoint: This method should only be called on the server.");
            return new RespawnPoint(); // サーバーでない場合はデフォルトの位置を返す
        }
        // 他のプレイヤーの位置を取得
        List<Vector3> otherPlayerPositions = GetOtherPlayerPositions(respawnPlayer);
        if (otherPlayerPositions.Count == 0)
        {
            otherPlayerPositions.Add(Vector3.zero); // 他のプレイヤーがいない場合は原点からの距離で計算
        }
        //各スポーン地点のスコア計算(地点、スコア、元のインデックス)
        List<(Vector3 point, float score, int index)> scoredSpawnPoints = new();

        var scaledRespawnPoints = respawnPoints.Select(p => p.Position * mapScale).ToList();

        int index = 0;
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
            scoredSpawnPoints.Add((point, closestSqrDistance,index));
            index++;
        }

        //距離が最も遠い順にソート
        scoredSpawnPoints.Sort((a, b) => b.score.CompareTo(a.score));
        
        //予約されていない地点を探す
        int chosenIndex = FindAvailableSpawnIndex(scoredSpawnPoints.Select(p => p.point).ToList());

        //元々のインデックスからスポーン地点を取得
        return respawnPoints[scoredSpawnPoints[chosenIndex].index];
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

        //空いてない場合はランダムな地点を選ぶ
        return UnityEngine.Random.Range(0,respawnPoints.Count);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestTeleportSpawnPoint(PlayerRef spawnPlayer)
    {
        // スポーン地点を取得
        RespawnPoint respawnPoint = GetRespawnPoint(spawnPlayer);
        RPC_TeleportSpawnPoint(spawnPlayer, respawnPoint.Position, respawnPoint.Rotation);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_TeleportSpawnPoint(PlayerRef spawnPlayer, Vector3 respawnPosition, Quaternion respownRotation)
    {
        Debug.Log($"RPC_TeleportSpawnPoint: Local = Player?:{Runner.LocalPlayer == spawnPlayer}");
        Debug.Log($"RPC_TeleportSpawnPoint: Try Get ? : {Runner.TryGetPlayerObject(spawnPlayer, out NetworkObject o)}");

        if (Runner.TryGetPlayerObject(spawnPlayer, out NetworkObject playerObject) && 
            playerObject != null)
        {
            var playerAvatar = playerObject.GetComponent<PlayerAvatar>();
            if (playerAvatar != null)
            {
                playerAvatar.HideMesh(); // メッシュを非表示にする
                playerAvatar.TeleportToInitialSpawnPoint(respawnPosition,respownRotation);
                playerAvatar.ShowMesh(); // メッシュを表示する
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
