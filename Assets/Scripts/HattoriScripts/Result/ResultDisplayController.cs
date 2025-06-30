using Fusion;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// ���ʉ�ʂŃv���C���[�̃X�R�A��\���E���񂷂�N���X
/// </summary>
public class ResultDisplayController : NetworkBehaviour,IAfterSpawned
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject scoreRowPrefab;
    [Networked]
    [Capacity(8)] // �ő�8�l�̃v���C���[��z��
    private NetworkDictionary<PlayerRef, PlayerScore> PlayerScores { get; }

    public override void Spawned()
    {

        //�J�[�\���Œ�𖳌���
        Cursor.lockState = CursorLockMode.None; 
        Cursor.visible = true;

        if (contentParent == null)
        {
            var obj = GameObject.Find("ScoreBars");
            if (obj != null)
            {
                contentParent = obj.transform;
            }
            else
            {
                Debug.LogError("ContentParent�^�O�̃I�u�W�F�N�g��������܂���ł����B");
            }
        }

        if (Runner.IsServer)
        {
            //�����Ă����X�R�A���擾
            ScoreTransfer scoreTransfer = FindObjectOfType<ScoreTransfer>();
            if (scoreTransfer == null)
            {
                Debug.LogError("ScoreTransfer component not found in the scene. Please ensure it is present.");
                return;
            }
            var sortedScores = scoreTransfer.GetScores();
            foreach (var score in sortedScores)
            {
                PlayerScores.Set(score.Key,score.Value); // �X�R�A�������ɒǉ�
            }
            //��ڂ��I������ScoreTransfer���폜
            Destroy(scoreTransfer.gameObject); 
        }


    }

    public void AfterSpawned()
    {
        // �X�R�A���������Ɍ��ʉ�ʂ�\��
        //�X�R�A�f�[�^���ƂɃX�R�A�s�𐶐�
        PlayerScore prevScore = default; 
        int prevRank = 1;
        int index = 0;
        foreach (var pair in GetSortedScores())
        {
            var playerRef = pair.Key;
            var score = pair.Value;
            int rank = 0;

            // ���ʌv�Z
            if (index == 0)
            {
                prevScore = score; 
                rank = 1; 
            }
            else
            {
                if (score.Kills == prevScore.Kills&&score.Deaths == prevScore.Deaths)
                {
                    // �O�̃X�R�A�Ɠ����Ȃ珇�ʂ�O��Ɠ����ɂ���
                    rank = prevRank; 
                }
                else
                {
                    // �O�̃X�R�A�ƈقȂ�ꍇ�͐V�������ʂ�ݒ�
                    rank = index + 1; 
                }
            }

            GameObject scoreRow;
            scoreRow = Instantiate(scoreRowPrefab, contentParent);
            UpdateResultRow(scoreRow, rank, playerRef, score);
            scoreRow.transform.SetSiblingIndex(index);

            // �O�̃X�R�A�Ə��ʂ��X�V
            prevScore = score;
            prevRank = rank; 
            index++;
        }
    }

    private void UpdateResultRow(GameObject row, int rank, PlayerRef playerRef, PlayerScore score)
    {
        row.transform.Find("RankImage/RankText").GetComponent<TMP_Text>().text = (rank).ToString();
        //Info : �v���C���[���̂�����ꍇ�͂�����������
        row.transform.Find("PlayerNameText").GetComponent<TMP_Text>().text = $"Player {playerRef.PlayerId}"; 
        row.transform.Find("KillScoreImage/KillCountText").GetComponent<TMP_Text>().text = score.Kills.ToString();
        row.transform.Find("DeathScoreImage/DeathCountText").GetComponent<TMP_Text>().text = score.Deaths.ToString();
        // �����̃X�R�A�s��ڗ������邽�߂Ƀn�C���C�g�\������
        if (playerRef == Runner.LocalPlayer)
        {
            row.transform.Find("ColorBar").gameObject.SetActive(true); 
        }
        else
        {
            row.transform.Find("ColorBar").gameObject.SetActive(false);
        }
    }

    public IReadOnlyList<KeyValuePair<PlayerRef, PlayerScore>> GetSortedScores()
    {
        var sortedScores = PlayerScores
         .OrderByDescending(kvp => kvp.Value.Kills)     // �L������������iko
         .ThenBy(kvp => kvp.Value.Deaths)               // �f�X�������Ȃ���
         .ThenBy(kvp => kvp.Key.RawEncoded)             // PlayerRef�̐��l����������
         .ToList();

        return sortedScores;
    }


}
