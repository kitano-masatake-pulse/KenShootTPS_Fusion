using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

// ���X�|�[���Ǘ��N���X
public class RespawnManager : NetworkBehaviour
{
    // �V���O���g���C���X�^���X
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

    //�z�X�g�Ƀ��X�|�[���v��
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestRespawn(NetworkObject playerObject)
    {
        // ���X�|�[���v�����󂯎�����v���C���[�̃I�u�W�F�N�g���L�����m�F
        if (playerObject == null || !playerObject.IsValid)
        {
            Debug.LogWarning("RPC_RequestRespawn: Invalid player object.");
            return;
        }
        Debug.Log($"RPC_RequestRespawn: Player {playerObject.InputAuthority} requested respawn.");
        // ���X�|�[�����������s
        InitializePlayerInHost(playerObject);
    }

    private void InitializePlayerInHost(NetworkObject playerObject)
    {
        //�z�X�g�̂ݎ��s
        if(!Object.HasStateAuthority) return;        

        //HP�̏������E���G��
        var playerState = playerObject.GetComponent<PlayerNetworkState>();
        if (playerState != null)
        {
            Debug.Log($"InitializePlayerInHost�F {playerObject.InputAuthority} HP initialized.");
            playerState.SetInvincible(true); 
            playerState.InitializeHP();
        }
        
        //RPC���Ăяo���āA�N���C�A���g���ł����X�|�[�����������s
        RPC_InitializePlayerInAll(playerObject);

    }

    //�N���C�A���g���̃��X�|�[������
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_InitializePlayerInAll(NetworkObject playerObject)
    {
        Debug.Log($"InitializePlayerInAll: Player {playerObject.InputAuthority} Local Initialized");

        if (playerObject != null && playerObject.IsValid)
        {
            // �v���C���[�̃A�j���[�V�������A�C�h����Ԃ�
            var animator = playerObject.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play("Idle");
            }
        }
        

        //Collider �L����(���C���[�؂�ւ�)
        foreach (var col in playerObject.GetComponentsInChildren<Collider>())
            col.gameObject.layer = LayerMask.NameToLayer("Player");
        //Hitbox �L����(���C���[�؂�ւ�)
        foreach (var hitbox in playerObject.GetComponentsInChildren<PlayerHitbox>())
            hitbox.gameObject.layer = LayerMask.NameToLayer("PlayerHitbox");
        var target = playerObject.GetComponentInChildren<PlayerWorldUIController>(true);
        if (target != null)
        {
            target.gameObject.SetActive(true); // �l�[���^�O���ĕ\��
        }
    }
    //���X�|�[���n�_�̎擾
    

    //�z�X�g���̃��X�|�[���I�����̏���
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RespawnEnd(NetworkObject playerObject)
    {
        if (playerObject == null || !playerObject.IsValid)
        {
            Debug.LogWarning("RPC_RespawnEnd: Invalid player object.");
            return;
        }
        Debug.Log($"RPC_RespawnEnd: Player {playerObject.InputAuthority} respawn finished.");
        // �v���C���[�̖��G��Ԃ�����
        var playerState = playerObject.GetComponent<PlayerNetworkState>();
        if (playerState != null)
        {
            playerState.SetInvincible(false);
        }
    }
}
