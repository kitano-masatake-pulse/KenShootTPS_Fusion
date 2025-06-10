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
        Debug.Log("PlayerInitializer: �e��A�A�j���[�V�������X�g������");
        //���S�����v���C���[�̃��[�J�����ł̂݋N���鏈��
        //�e�򏉊���
        NetworkObject myPlayer = GameManager.Instance.GetMyPlayer();
        PlayerAvatar playerAvatar = myPlayer.GetComponent<PlayerAvatar>();
        playerAvatar.InitializeAllAmmo();
        playerAvatar.ClearActionAnimationPlayList();

        //�z�X�g�Ƀ��X�|�[���v��
        RespawnManager.Instance.RPC_RequestRespawn(myPlayer);
    }

}
