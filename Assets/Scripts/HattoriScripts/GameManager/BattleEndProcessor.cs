using System.Collections;
using System;
using UnityEngine;
using Fusion;

public class BattleEndProcessor : NetworkBehaviour
{
    //�V���O���g����
    public static BattleEndProcessor Instance { get; private set; }
    [SerializeField] private SceneChangeFade fadeUI;
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
        fadeUI = FindObjectOfType<SceneChangeFade>();
        if (fadeUI == null)
        {
            Debug.LogError("FadeUI component not found in the scene. Please ensure it is present.");
            return;
        }
        
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
        StartCoroutine(FadeSceneChange(3f));
    }

    //�R���[�`��
    private IEnumerator FadeSceneChange(float duration)
    {
        yield return fadeUI.FadeAlpha(0f, 1f, duration);

        //�Ó]���̏���
        yield return Resources.UnloadUnusedAssets();        
        GC.Collect();
        GC.WaitForPendingFinalizers();

        //���U���g��ʂ֑J��
        if (Runner.IsServer)
        {
            string sceneName = nextScene.ToSceneName();
            //�V�[���J��
            yield return Runner.SetActiveScene(sceneName);
        }
        
    }

}
