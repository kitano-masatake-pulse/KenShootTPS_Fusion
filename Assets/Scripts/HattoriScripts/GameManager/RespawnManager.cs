using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class RespawnManager : NetworkBehaviour
{
    //�V���O���g��
    public static RespawnManager Instance { get; private set; }

    private void Awake()
    {
        // �V���O���g���̃C���X�^���X��ݒ�
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // �V�[�����ׂ��ŃI�u�W�F�N�g��ێ�
        }
        else
        {
            Destroy(gameObject); // ���ɑ��݂���ꍇ�͐V�����C���X�^���X��j��
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
        RespawnPlayer(playerObject);
    }
    //�v���C���[�̃��X�|�[������
    private void RespawnPlayer(NetworkObject playerObject)
    {
        //�z�X�g�̂�
        if(!Object.HasStateAuthority)
        {
            return;
        }
        if (playerObject != null && playerObject.IsValid)
        {
            // �v���C���[�̈ʒu�����X�|�[���n�_�ɐݒ�
            var playerAvatar = playerObject.GetComponent<PlayerAvatar>();
            if (playerAvatar != null)
            {
                Debug.Log($"RespawnPlayer�F {playerObject.InputAuthority} at initial spawn point.");
                playerAvatar.TeleportToInitialSpawnPoint(GetRespawnPoint());
                //�t���O�̃��Z�b�g???������ƌ�ł����ƌ���ׂ�
                playerAvatar.IsHoming = false;
                playerAvatar.SetFollowingCameraForward(true);
            }

            //HP�̏�����
            var playerState = playerObject.GetComponent<PlayerNetworkState>();
            if (playerState != null)
            {
                Debug.Log($"RespawnPlayer�F {playerObject.InputAuthority} HP initialized.");
                playerState.SetInvincible(true); // ���G��Ԃɂ���
                playerState.InitializeHP();
            }

        }

        //RPC���Ăяo���āA�N���C�A���g���ł����X�|�[�����������s
        RPC_RespawnPlayerClient(playerObject);

    }

    //�N���C�A���g���̃��X�|�[������
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_RespawnPlayerClient(NetworkObject playerObject)
    {
        Debug.Log($"RPC_RespawnPlayerClient: Player {playerObject.InputAuthority} Local Initialized");
        // �N���C�A���g���ł̃��X�|�[������
        if (playerObject != null && playerObject.IsValid)
        {
            // �v���C���[�̃A�j���[�V���������Z�b�g
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
    public Vector3 GetRespawnPoint()
    {
        var randomValue = UnityEngine.Random.insideUnitCircle * 5f;
        var spawnPosition = new Vector3(randomValue.x, 5f, randomValue.y);
        return spawnPosition;
    }
}
