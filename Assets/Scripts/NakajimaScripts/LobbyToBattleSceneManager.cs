using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class LobbyToBattleSceneManager : NetworkBehaviour
{
 

    private NetworkRunner runner;

    private void Start()
    {
        // 現在のシーンに存在している NetworkRunner を自動で取得
        runner = FindObjectOfType<NetworkRunner>();
        if (runner == null)
        {
            Debug.LogError("NetworkRunner が見つかりません。シーンに配置されていますか？");
        }
    }
    public void SwitchToBattle()
    {
        if (runner != null && runner.IsServer)
        {
            string sceneName = SceneType.Battle.ToSceneName();
            runner.SetActiveScene(sceneName);
        }
    }
}
