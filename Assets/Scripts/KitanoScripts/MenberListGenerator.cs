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

        Debug.Log($"GenerateMenberList called. runner:{_runner}");
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
        if (_runner != null)
        { Debug.Log($"NetworkRunner is not null. Runner:{_runner}"); }

        if (_runner.IsServer == false)
        {
            Debug.LogWarning("NetworkRunner is not a server. Cannot spawn member list.");
            return;
        }
        _runner.Spawn(
        memberListPrefab,
        Vector3.zero,
        Quaternion.identity,
        PlayerRef.None,
        // ← ここが OnBeforeSpawned デリゲート
        (runner, spawnedObj) => {
            spawnedObj.transform.SetParent(this.transform, false);
        }
    );

    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
