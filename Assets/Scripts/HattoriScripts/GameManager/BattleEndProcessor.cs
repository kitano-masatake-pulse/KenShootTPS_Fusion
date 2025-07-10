using System.Collections;
using System;
using UnityEngine;
using Fusion;

public class BattleEndProcessor : NetworkBehaviour
{
    //�V���O���g����
    public static BattleEndProcessor Instance { get; private set; }
    [Header("�����I�����A�t�F�[�h�A�E�g����܂ł̑ҋ@����")]
    [SerializeField] private float waitingTime = 3f;
    [Header("���ɑJ�ڂ���V�[��(�f�t�H���gBattleScene)")]
    public SceneType nextScene = SceneType.Result;
    public event Action OnBattleEnd;


    private void Awake()
    {
        //�V���O���g���̃C���X�^���X��ݒ�
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }   
    private void OnEnable()
    {
        GameManager.OnManagerInitialized -= SubscribeEvent;
        GameManager.OnManagerInitialized += SubscribeEvent;
    }

    public override void Spawned()
    {
        
    }

    private void SubscribeEvent()
    {
        GameManager.Instance.OnTimeUp -= HandleBattleEnd;
        GameManager.Instance.OnTimeUp += HandleBattleEnd;

    }


    private void OnDisable()
    {
        GameManager.OnManagerInitialized -= SubscribeEvent;
        if (GameManager.Instance == null) return;
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
        OnBattleEnd?.Invoke();
        //�����I�����̃v���C���[����
        //�S�v���C���[(�����̃v���C���[)�̍s�����~
        NetworkObject myPlayer = GameManager.Instance.GetMyPlayer();
        PlayerAvatar playerAvatar = myPlayer.GetComponent<PlayerAvatar>();
        playerAvatar.IsDuringWeaponAction = true;
        playerAvatar.IsImmobilized = true;
        StartCoroutine(watingCoroutine(waitingTime));
    }

    //�R���[�`��
    private IEnumerator watingCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        if(Runner.IsServer)
        {
            SceneTransitionManager.Instance.ChangeScene(nextScene);
        }
    }

}
