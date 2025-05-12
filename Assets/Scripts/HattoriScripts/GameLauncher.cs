using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class GameLauncher : MonoBehaviour
{
    [SerializeField]
    private NetworkRunner networkRunnerPrefab;

    private NetworkRunner networkRunner;

    private async void Start()
    {
        // NetworkRunner�𐶐�����
        networkRunner = Instantiate(networkRunnerPrefab);
        // StartGameArgs�ɓn�����ݒ�ŁA�Z�b�V�����ɎQ������
        var result = await networkRunner.StartGame(new StartGameArgs {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = "KenShootTPS_Fusion_DeathMatch",
            SceneManager = networkRunner.GetComponent<NetworkSceneManagerDefault>()
        });

        if (result.Ok)
        {
            Debug.Log("�����I");
        }
        else
        {
            Debug.Log("���s�I");
        }
    }
}
