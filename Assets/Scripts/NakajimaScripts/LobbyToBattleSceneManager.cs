using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class LobbyToBattleSceneManager : NetworkBehaviour
{
 

    private NetworkRunner runner;

    private void Start()
    {
        // ���݂̃V�[���ɑ��݂��Ă��� NetworkRunner �������Ŏ擾
        runner = FindObjectOfType<NetworkRunner>();
        if (runner == null)
        {
            Debug.LogError("NetworkRunner ��������܂���B�V�[���ɔz�u����Ă��܂����H");
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
