using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// ���X�|�[���Ǘ��N���X
public class RespawnManager : NetworkBehaviour
{
    // �V���O���g���C���X�^���X
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
        new RespawnPoint(new Vector3(-9, 2, -93), Quaternion.Euler(0, 90, 0)), // ���X�|�[���n�_1
        new RespawnPoint(new Vector3(94, 5, 14), Quaternion.Euler(0, 180, 0)), // ���X�|�[���n�_2
        new RespawnPoint(new Vector3(27, 0, 91), Quaternion.Euler(0, 90, 0)), // ���X�|�[���n�_3
        new RespawnPoint(new Vector3(-97, 5, 7.5f), Quaternion.Euler(0, 0, 0)), // ���X�|�[���n�_4
        new RespawnPoint(new Vector3(-40, 5, -55), Quaternion.Euler(0, 45, 0)), // ���X�|�[���n�_5
        new RespawnPoint(new Vector3(49, 5, 18), Quaternion.Euler(0, 90, 0)), // ���X�|�[���n�_6
        new RespawnPoint(new Vector3(-20, 3, 64), Quaternion.Euler(0, 135, 0)), // ���X�|�[���n�_7
        new RespawnPoint(new Vector3(57.5f, 9.5f, -44), Quaternion.Euler(0, -135, 0)), // ���X�|�[���n�_8

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
        //�n�b�V���Z�b�g�̃��Z�b�g
        reservedSpawnIndices.Clear();
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

        RespawnPoint respawnPoint = GetRespawnPoint(playerObject.InputAuthority);

        

        //HP�̏������E���G��
        var playerState = playerObject.GetComponent<PlayerNetworkState>();
        if (playerState != null)
        {
            Debug.Log($"InitializePlayerInHost�F {playerObject.InputAuthority} HP initialized.");
            playerState.SetInvincible(true); 
            playerState.InitializeHP();
        }
        
        //RPC���Ăяo���āA�N���C�A���g���ł����X�|�[�����������s
        RPC_InitializePlayerInAll(playerObject, respawnPoint.Position, respawnPoint.Rotation);

    }

    //�N���C�A���g���̃��X�|�[������
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_InitializePlayerInAll(NetworkObject playerObject,Vector3 respawnPosition,Quaternion respawnRotation)
    {
        Debug.Log($"InitializePlayerInAll: Player {playerObject.InputAuthority} Local Initialized");

        var playerAvatar = playerObject.GetComponent<PlayerAvatar>();
        if (playerAvatar != null)
        {
            playerAvatar.HideMesh(); // ���b�V�����\���ɂ���
            playerAvatar.TeleportToInitialSpawnPoint(respawnPosition, respawnRotation);
            playerAvatar.ShowMesh(); // ���b�V����\������

            //�����I�ɖ������currrentWeapon��ύX����
            playerAvatar.ForceWeaponChange(WeaponType.AssaultRifle);
    


            }

        if (playerObject != null)
        {


            playerAvatar.SetActionAnimationPlayList(ActionType.Respawn,Runner.SimulationTime);
            Debug.Log($"RPC_InitializePlayerInAll: Player {playerObject.InputAuthority} action animation set to Respawn.");

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
        if (playerObject == null)
        {
            Debug.LogWarning("RPC_RespawnEnd: Invalid player object.");
            return;
        }
        Debug.Log($"RPC_RespawnEnd: Player {playerObject.InputAuthority} respawn finished.");
        // �v���C���[�̖��G��Ԃ�3�b��ɉ���
        var playerState = playerObject.GetComponent<PlayerNetworkState>();
        if (playerState != null)
        {
            playerState.SetInvincible(true, 5.0f);
        }
    }

    // ���X�|�[���n�_���擾���郁�\�b�h
    private RespawnPoint GetRespawnPoint(PlayerRef respawnPlayer)
    {
        if(!Runner.IsServer)
        {
            Debug.LogError("GetRespawnPoint: This method should only be called on the server.");
            return new RespawnPoint(); // �T�[�o�[�łȂ��ꍇ�̓f�t�H���g�̈ʒu��Ԃ�
        }
        // ���̃v���C���[�̈ʒu���擾
        List<Vector3> otherPlayerPositions = GetOtherPlayerPositions(respawnPlayer);
        if (otherPlayerPositions.Count == 0)
        {
            Debug.LogWarning("No other player positions found. Using default respawn point.");
            // ���̃v���C���[�����Ȃ��ꍇ�̓����_���ȃX�|�[���n�_���g�p
            var randomIndex = UnityEngine.Random.Range(0, respawnPoints.Count);
            Vector3 respownPosition = respawnPoints[randomIndex].Position * mapScale;
            Quaternion respownRotation = respawnPoints[randomIndex].Rotation;
            return new RespawnPoint(respownPosition, respownRotation);
        }
        //�e�X�|�[���n�_�̃X�R�A�v�Z(�n�_�A�X�R�A�A���̃C���f�b�N�X)
        List<(Vector3 point, float score, int index)> scoredSpawnPoints = new();

        var scaledRespawnPoints = respawnPoints.Select(p => p.Position * mapScale).ToList();

        int index = 0;
        foreach (var point in scaledRespawnPoints)
        {
            float closestSqrDistance = float.MaxValue;
            foreach (var otherPos in otherPlayerPositions)
            {
                // �e�X�|�[���n�_�Ƒ��̃v���C���[�̋������v�Z
                float sqrDistance = (point - otherPos).sqrMagnitude;
                if (sqrDistance < closestSqrDistance)
                {
                    closestSqrDistance = sqrDistance;
                }
            }
            scoredSpawnPoints.Add((point, closestSqrDistance,index));
            index++;
        }

        //�������ł��������Ƀ\�[�g
        scoredSpawnPoints.Sort((a, b) => b.score.CompareTo(a.score));
        
        //�\�񂳂�Ă��Ȃ��n�_��T��
        int chosenIndex = FindAvailableSpawnIndex(scoredSpawnPoints.Select(p => p.point).ToList());

        //���X�̃C���f�b�N�X����X�|�[���n�_���擾
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

    //�\��X�|�[���n�_�̊Ǘ�
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

        //�󂢂ĂȂ��ꍇ�̓����_���Ȓn�_��I��
        return UnityEngine.Random.Range(0,respawnPoints.Count);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_RequestTeleportSpawnPoint(PlayerRef spawnPlayer)
    {
        // �X�|�[���n�_���擾
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
                playerAvatar.HideMesh(); // ���b�V�����\���ɂ���
                playerAvatar.TeleportToInitialSpawnPoint(respawnPosition,respownRotation);
                playerAvatar.ShowMesh(); // ���b�V����\������
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
