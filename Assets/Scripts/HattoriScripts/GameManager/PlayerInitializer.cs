using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerInitializer : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnEnable()
    {
        RespawnPanel.OnRespawnClicked += InitializePlayer;
    }

    private void OnDisable()
    {
        RespawnPanel.OnRespawnClicked -= InitializePlayer;
    }

    private void InitializePlayer()
    {
        //死亡したプレイヤーのローカル環境でのみ起こる処理
        //弾薬初期化
        NetworkObject myPlayer = GameManager.Instance.GetMyPlayer();
        myPlayer.GetComponent<PlayerAvatar>().InitializeAllAmmo();
    }

}
