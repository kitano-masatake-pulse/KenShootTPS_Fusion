using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Fusion;

public class MenberListGenerator : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] NetworkObject memberListPrefab; // プレハブの参照

    void Awake()
    {
        // プレハブが設定されていない場合はエラーメッセージを表示
        if (memberListPrefab == null)
        {
            Debug.LogError("Member List Prefab is not assigned in the inspector.");
        }

        // NetworkRunnerの生成を待つために、OnNetworkRunnerGeneratedイベントを登録
        GameLauncher.OnNetworkRunnerGenerated -= GenerateMenberList;
        GameLauncher.OnNetworkRunnerGenerated += GenerateMenberList;
    }
    void GenerateMenberList(NetworkRunner _runner )
    { 

        if (memberListPrefab == null)
        {
            Debug.LogError("Member List Prefab is not assigned in the inspector.");
            return;
        }
        if (_runner == null)
        {
            Debug.LogError("NetworkRunner is null. Cannot spawn member list.");
            return;
        }
        if (_runner.IsServer == false)
        {
            Debug.LogWarning("This method should only be called on the server.");
            return;
        }

        NetworkObject menberlist = _runner.Spawn(
        memberListPrefab,
        //Vector3.zero,
        new Vector3(1, 1, 1), // 位置を指定
        Quaternion.identity,
        PlayerRef.None
        // ← ここが OnBeforeSpawned デリゲート
        //,(runner, spawnedObj) => {spawnedObj.transform.SetParent(this.transform, false); }

        

    );

        menberlist.gameObject.GetComponent<LobbyPingDisplay>().AddCallbackMe(_runner);

    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
