using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;



//�z�X�g���ŃL���f�X�����m�������ŃX�R�A�Ƃ��ĊǗ�
//Networked�v���p�e�B���g���āA�X�R�A�̕ύX���l�b�g���[�N��œ�������
public class GameManager : NetworkBehaviour
{
    // �V���O���g���C���X�^���X
    public static GameManager Instance { get; private set; }


    //�v���C���[�̃X�R�A���Ǘ����鎫��
    //�L�[: PlayerRef, �l: PlayerScore
    [Networked(OnChanged = nameof(ManagerScoreCallback))]
    [Capacity(8)]
    private NetworkDictionary<PlayerRef, PlayerScore> PlayerScores { get; }
    //�X�R�A�ύX�C�x���g
    public event Action<IEnumerable<KeyValuePair<PlayerRef, PlayerScore>>> OnManagerScoreChanged;


    //===========================================
    //����������
    //===========================================
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

    public override void Spawned()
    {
        if(Object.HasStateAuthority)
        {
            Debug.Log($"ScoreManager Spawned. Current Player Count: {Runner.ActivePlayers.Count()}");

            foreach (var playerRef in Runner.ActivePlayers)
            {
                InitializePlayerScore(playerRef);
            }
        }
    }
    private void InitializePlayerScore(PlayerRef playerRef)
    {
        PlayerScore playerScore = new PlayerScore(0, 0);
        PlayerScores.Set(playerRef, playerScore);
    }


    //===========================================
    //�z�X�g���ł̂݌Ă΂�郁�\�b�h�Q
    //===========================================


    //�v���C���[�����S�����Ƃ��ɌĂ΂�A�F�X�ȏ������s�����\�b�h
    public void NotifyDeath(PlayerRef victim, PlayerRef killer)
    {
        //�����ŃX�R�A�̍X�V���s��
        AddScore(victim, killer);

        //RPC�őS���[�J�����ł�victim�Ɏ��S������ʒm


    }

    //�v���C���[�̃X�R�A���X�V���郁�\�b�h
    public void AddScore(PlayerRef victim, PlayerRef killer = default)
    {
        if (!Object.HasStateAuthority) return;
        // �f�X��ǉ�
        if (PlayerScores.TryGet(victim, out PlayerScore victimScore))
        {
            victimScore.Deaths++;
            PlayerScores.Set(victim, victimScore);
        }
        // �L����ǉ�
        if (PlayerScores.TryGet(killer, out PlayerScore killerScore))
        {
            killerScore.Kills++;
            PlayerScores.Set(killer, killerScore);
        }
    }


    //===========================================
    //�S���[�J�����ŌĂ΂�郁�\�b�h�Q
    //===========================================
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_RaiseDeathEvent(PlayerRef victim, PlayerRef killer)
    {
        if(Runner.TryGetPlayerObject(victim, out var victimPlayer))
        {
            var victimState = victimPlayer.GetComponent<PlayerNetworkState>();
            if (victimState != null)
            {
                victimState.OnPlayerDeath(killer);
            }
        }
    }


    //�X�R�A���ύX���ꂽ�Ƃ��ɌĂ΂��R�[���o�b�N
    public static void ManagerScoreCallback(Changed<GameManager> changed)
    {
        // �X�R�A���ύX���ꂽ�Ƃ��ɃC�x���g�𔭉�
        changed.Behaviour.OnManagerScoreChanged?.Invoke(changed.Behaviour.GetAllScores());
        changed.Behaviour.PrintAllScore();
    }
    
    public bool TryGetPlayerScore(PlayerRef playerRef, out PlayerScore score)
    {
        return PlayerScores.TryGet(playerRef, out score);
    }

    public PlayerScore GetPlayerScore(PlayerRef playerRef)
    {
        if (PlayerScores.TryGet(playerRef, out var score))
        {
            return score;
        }
        return new PlayerScore(0, 0); // ���݂��Ȃ��ꍇ�̓f�t�H���g�l��Ԃ�
    }

    //�S�v���C���[���̎�����Ԃ�
    public�@IEnumerable<KeyValuePair<PlayerRef, PlayerScore>> GetAllScores()
    {
        return PlayerScores;
    }

    //===========================================
    //-----------------�f�o�b�O�p----------------
    //===========================================

    //�z�X�g���C�ӂ̃v���C���[�̃L���^�f�X�𑝌��ł��郁�\�b�h
    public void ModifyScore(PlayerRef target, int killsDelta, int deathsDelta)
    {
        if (!Object.HasStateAuthority) return;

        if (PlayerScores.TryGet(target, out var score))
        {
            // 0 �����ɂ͂��Ȃ��悤�� Clamp
            score.Kills = Mathf.Max(0, score.Kills + killsDelta);
            score.Deaths = Mathf.Max(0, score.Deaths + deathsDelta);
            PlayerScores.Set(target, score);
        }
    }
    //�S�v���C���[�̃X�R�A���f�o�b�O���O�ɏo�͂��郁�\�b�h
    public void PrintAllScore()
    {
        if (!Object.HasStateAuthority) return;
        foreach (var kvp in PlayerScores)
        {
            Debug.Log($"Player: {kvp.Key}, Kills: {kvp.Value.Kills}, Deaths: {kvp.Value.Deaths}");
        }
    }
}
