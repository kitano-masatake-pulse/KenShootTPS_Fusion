using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class BattleEndProcessor : NetworkBehaviour
{
    [SerializeField] private FadeUI fadeUI;
    [Header("���ɑJ�ڂ���V�[��(�f�t�H���gBattleScene)")]
    public SceneType nextScene = SceneType.Result;
    public event Action OnGameEnd;

    private void OnEnable()
    {
        GameManager.Instance.OnTimeUp -= HandleBattleEnd;
        GameManager.Instance.OnTimeUp += HandleBattleEnd;
    }
    private void OnDisable()
    {
        GameManager.Instance.OnTimeUp -= HandleBattleEnd;
    }

    private void HandleBattleEnd()
    {
        //�z�X�g���ł̂ݎ��s
        if (!Runner.IsServer) return;
        //GameManager�̃X�R�A�������擾
        var scoreList = GameManager.Instance.GetSortedScores();

        //�f�[�^�ڏ��p�I�u�W�F�N�g�𐶐�
        var scoreTransferObj = new GameObject("ScoreTransferObject");
        var scoreTransfer = scoreTransferObj.AddComponent<ScoreTransfer>();
        scoreTransfer.SetScores(scoreList);
        DontDestroyOnLoad(scoreTransferObj);

        RPC_EndBattle();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_EndBattle()
    {
        //�����I���C�x���g�𔭉�
        OnGameEnd?.Invoke();
        //�����I�����̃v���C���[����
        //�S�v���C���[(�����̃v���C���[)�̍s�����~
        NetworkObject myPlayer = GameManager.Instance.GetMyPlayer();
        PlayerAvatar playerAvatar = myPlayer.GetComponent<PlayerAvatar>();
        playerAvatar.IsDuringWeaponAction = true;
        playerAvatar.IsImmobilized = true;
        StartCoroutine(FadeToBlack(3f));
    }

    //�R���[�`��
    private IEnumerator FadeToBlack(float duration)
    {
        yield return fadeUI.FadeAlpha(0f, 1f, duration);
        //���U���g��ʂ֑J��
        if (Runner.IsServer)
        {
            string sceneName = nextScene.ToSceneName();
            //�V�[���J��
            Runner.SetActiveScene(sceneName);
        }
        
    }

}
